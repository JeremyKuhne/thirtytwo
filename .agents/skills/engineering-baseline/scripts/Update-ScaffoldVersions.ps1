<#
.SYNOPSIS
    Vets and pins the package versions used by New-DotnetRepo.ps1.

.DESCRIPTION
    Resolves the latest KNOWN-GOOD version of each managed package - never simply
    the latest. A candidate is rejected unless it is, per the policy in
    versions.json: old enough (minimum release age / quarantine), listed (not
    yanked), not deprecated, free of known advisories, and under an allowed
    license. This keeps freshly published - and therefore least-vetted - versions,
    including compromised ones, out of newly scaffolded repositories.

    Reports proposed changes by default. Use -Apply to write versions.json (review
    the diff before committing). Use -SmokeTest to authoritatively validate the
    pinned set against the real project layouts (a tool and a multi-target library
    that builds on net472), which is the ground-truth TFM-compatibility check.

.PARAMETER VersionsPath
    Path to the manifest. Defaults to versions.json next to this script.

.PARAMETER MinimumReleaseAgeDays
    Override the manifest's quarantine window for this run.

.PARAMETER Source
    NuGet v3 source. Defaults to the manifest's source (nuget.org).

.PARAMETER Apply
    Write the resolved versions back to versions.json and stamp lastVetted.

.PARAMETER SmokeTest
    After resolving (and applying), scaffold a tool and a multi-target library
    into temp folders and build them, to confirm the pinned set restores and
    builds on every targeted TFM.

.EXAMPLE
    .\Update-ScaffoldVersions.ps1                 # report only
    .\Update-ScaffoldVersions.ps1 -Apply -SmokeTest
#>

#Requires -Version 7.2
[CmdletBinding()]
param(
    [string] $VersionsPath = (Join-Path $PSScriptRoot 'versions.json'),
    [int]    $MinimumReleaseAgeDays,
    [string] $Source,
    [switch] $Apply,
    [switch] $SmokeTest,
    [Parameter(DontShow)] [switch] $SkipMain
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Resolved from the feed's v3 service index once $Source is known (see below).
$flat = $null
$reg  = $null

function Get-StableVersions ([string] $id) {
    $u = "$flat/$($id.ToLowerInvariant())/index.json"
    (Invoke-RestMethod $u -TimeoutSec 30).versions | Where-Object { $_ -notmatch '-' }
}

function Get-VersionMeta ([string] $id, [string] $v) {
    $lid  = $id.ToLowerInvariant()
    $leaf = Invoke-RestMethod "$reg/$lid/$v.json" -TimeoutSec 30
    $ce = $leaf.catalogEntry
    if ($ce -is [string]) { $ce = Invoke-RestMethod $ce -TimeoutSec 30 }

    $license = if ($ce.PSObject.Properties['licenseExpression'] -and $ce.licenseExpression) {
        $ce.licenseExpression
    } else {
        try {
            $nuspec = [xml]((Invoke-WebRequest "$flat/$lid/$v/$lid.nuspec" -TimeoutSec 30).Content)
            [string]$nuspec.package.metadata.license.'#text'
        } catch { $null }
    }

    [pscustomobject]@{
        Version    = $v
        Published  = [datetime]$ce.published
        Listed     = [bool]$ce.listed
        Deprecated = [bool]($ce.PSObject.Properties['deprecation'] -and $ce.deprecation)
        Vulnerable = [bool]($ce.PSObject.Properties['vulnerabilities'] -and $ce.vulnerabilities)
        License    = $license
    }
}

# True only when every SPDX identifier in the (possibly compound) expression is on
# the allowlist. NuGet returns SPDX expressions such as "MIT OR Apache-2.0" or
# "(MIT AND BSD-3-Clause)"; an exact match would wrongly reject those. Custom
# (LicenseRef) expressions cannot be validated and are rejected.
function Test-LicenseAllowed ([string] $license, [string[]] $allow) {
    if ([string]::IsNullOrWhiteSpace($license)) { return $false }
    if ($license -match '(?i)LicenseRef') { return $false }
    $ids = $license -split '(?i)\s+(?:AND|OR|WITH)\s+' |
        ForEach-Object { ($_ -replace '[()]', '').Trim().TrimEnd('+') } |
        Where-Object { $_ }
    if (-not $ids) { return $false }
    foreach ($id in $ids) { if ($allow -notcontains $id) { return $false } }
    return $true
}

# Newest stable version that passes every gate, scanning newest-first.
function Resolve-KnownGood ([string] $id, [int] $minAge, [string[]] $allow) {
    $versions = Get-StableVersions $id | Sort-Object {
        # Order by full numeric version, padding to four components and dropping any
        # build metadata, so 4-segment versions are not collapsed to three.
        $base = ($_ -split '[-+]', 2)[0]
        $n = @($base -split '\.' | ForEach-Object { [int]$_ })
        while ($n.Count -lt 4) { $n += 0 }
        [version]::new($n[0], $n[1], $n[2], $n[3])
    } -Descending
    foreach ($v in $versions) {
        try { $m = Get-VersionMeta $id $v } catch { continue }
        $age = [int]((Get-Date) - $m.Published).TotalDays
        $reasons = @()
        if (-not $m.Listed)  { $reasons += 'unlisted' }
        if ($m.Deprecated)   { $reasons += 'deprecated' }
        if ($m.Vulnerable)   { $reasons += 'advisory' }
        if ($age -lt $minAge) { $reasons += "age ${age}d<${minAge}d" }
        if (-not $m.License) { $reasons += 'license unknown' }
        elseif (-not (Test-LicenseAllowed $m.License $allow)) { $reasons += "license $($m.License)" }
        if ($reasons.Count -eq 0) {
            return [pscustomobject]@{ Version = $v; Age = $age; License = $m.License; Reason = '' }
        }
        Write-Verbose "$id $v rejected: $($reasons -join ', ')"
    }
    return [pscustomobject]@{ Version = $null; Age = $null; License = $null; Reason = 'no known-good version' }
}

# ---------------------------------------------------------------------------

if ($SkipMain) { return }

$doc    = Get-Content $VersionsPath -Raw | ConvertFrom-Json
$minAge = if ($PSBoundParameters.ContainsKey('MinimumReleaseAgeDays')) { $MinimumReleaseAgeDays } else { [int]$doc.policy.minimumReleaseAgeDays }
$allow  = @($doc.policy.allowedLicenses)
if (-not $Source) { $Source = [string]$doc.policy.source }

# Resolve the flat-container and registration endpoints from the feed's v3 service
# index so -Source actually targets the requested feed (not a hard-coded nuget.org).
$index = Invoke-RestMethod $Source -TimeoutSec 30
$flat = ($index.resources | Where-Object { $_.'@type' -eq 'PackageBaseAddress/3.0.0' } | Select-Object -First 1).'@id'
foreach ($rt in @('RegistrationsBaseUrl/3.6.0', 'RegistrationsBaseUrl/Versioned', 'RegistrationsBaseUrl/3.4.0', 'RegistrationsBaseUrl')) {
    $reg = ($index.resources | Where-Object { $_.'@type' -eq $rt } | Select-Object -First 1).'@id'
    if ($reg) { break }
}
if (-not $flat -or -not $reg) { throw "Could not resolve PackageBaseAddress/RegistrationsBaseUrl from the service index at $Source." }
$flat = $flat.TrimEnd('/')
$reg = $reg.TrimEnd('/')

Write-Host "Vetting scaffold package versions" -ForegroundColor White
Write-Host "  source:    $Source"
Write-Host "  min age:   $minAge days (quarantine)"
Write-Host "  licenses:  $($allow -join ', ')`n"

$rows = foreach ($prop in $doc.packages.PSObject.Properties) {
    $id = $prop.Name
    $current = [string]$prop.Value
    $kg = Resolve-KnownGood $id $minAge $allow
    $action = if (-not $kg.Version) { 'BLOCKED' }
              elseif ($kg.Version -eq $current) { 'current' }
              else { 'bump' }
    [pscustomobject]@{
        Package   = $id
        Current   = $current
        KnownGood = $kg.Version
        AgeDays   = $kg.Age
        License   = $kg.License
        Action    = $action
        Note      = $kg.Reason
    }
}

$rows | Format-Table Package, Current, KnownGood, AgeDays, License, Action -AutoSize | Out-String | Write-Host
$blocked = @($rows | Where-Object Action -eq 'BLOCKED')
if ($blocked) { Write-Host "BLOCKED (no known-good version): $($blocked.Package -join ', ')" -ForegroundColor Yellow }

if ($Apply) {
    foreach ($r in $rows) { if ($r.KnownGood) { $doc.packages.$($r.Package) = $r.KnownGood } }
    if ($blocked) {
        Write-Host "`nNOT stamping lastVetted - no known-good version for $($blocked.Package -join ', '); resolve those before claiming a full vet." -ForegroundColor Yellow
    } else {
        $doc.policy.lastVetted = (Get-Date).ToString('yyyy-MM-dd')
    }
    ($doc | ConvertTo-Json -Depth 8) + "`n" | Set-Content -LiteralPath $VersionsPath -Encoding utf8NoBOM -NoNewline
    Write-Host "`nApplied to $VersionsPath. Review the diff and commit deliberately." -ForegroundColor Green
} else {
    Write-Host "`nReport only. Re-run with -Apply to write versions.json." -ForegroundColor Cyan
}

if ($SmokeTest) {
    $scaffold = Join-Path $PSScriptRoot 'New-DotnetRepo.ps1'
    $cases = @(
        @{ Name = 'vettool'; WindowsOnly = $false; Args = @('-Archetype', 'tool', '-PackageId', 'VetTool', '-ToolCommandName', 'vettool', '-Description', 'compat probe', '-Owner', 'vet') }
        @{ Name = 'vetlib';  WindowsOnly = $true;  Args = @('-Archetype', 'multi-target', '-PackageId', 'Vet.Lib', '-Framework', 'net10.0', '-FrameworkLegacy', 'net472', '-Description', 'compat probe', '-Owner', 'vet') }
    )
    $smokeFailed = $false
    foreach ($c in $cases) {
        if ($c.WindowsOnly -and -not $IsWindows) {
            Write-Host "`nSmoke test ($($c.Name)) skipped - building net472 needs Windows (or reference-assembly packages)." -ForegroundColor DarkGray
            continue
        }
        $root = Join-Path ([IO.Path]::GetTempPath()) "vet-$($c.Name)-$(Get-Random)"
        Write-Host "`nSmoke test ($($c.Name)) -> $root" -ForegroundColor White
        try {
            $out = & pwsh -NoProfile -File $scaffold -Root $root -Name $c.Name @($c.Args) *>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Host "  SCAFFOLD FAILED for $($c.Name) (exit $LASTEXITCODE)" -ForegroundColor Red
                $out | Select-Object -Last 20 | Out-String | Write-Host
                $smokeFailed = $true
                continue
            }
            Push-Location $root
            try {
                $out = & dotnet build -c Release --nologo *>&1
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "  BUILD FAILED for $($c.Name) - the pinned set is not TFM-compatible" -ForegroundColor Red
                    $out | Select-Object -Last 20 | Out-String | Write-Host
                    $smokeFailed = $true
                }
                else { Write-Host "  OK ($($c.Name) builds on its targeted TFMs)" -ForegroundColor Green }
            } finally { Pop-Location }
        } finally {
            Remove-Item $root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    if ($smokeFailed) { throw "Smoke test failed: the pinned set did not scaffold and build on every targeted TFM." }
}
