<#
.SYNOPSIS
    Vets and pins the GitHub Actions referenced by the scaffold's workflow
    templates. The github-actions counterpart to Update-ScaffoldVersions.ps1
    (which pins NuGet packages in versions.json).

.DESCRIPTION
    Scans the workflow templates under template/.github/workflows for
    `uses: <owner>/<repo>[/<path>]@<ref> # vX.Y.Z` lines and, for every distinct
    action, resolves the newest STABLE release that was published at least
    -MinimumReleaseAgeDays ago. That quarantine window keeps freshly published -
    and therefore least-vetted, possibly compromised - action releases out of
    newly scaffolded repositories, mirroring the NuGet policy in versions.json.

    Keeping these version comments current is what stops a freshly scaffolded
    repo from opening a long list of day-one Dependabot action bumps: if the
    template already pins the newest vetted major, Dependabot has nothing to
    propose. Stale comments (for example checkout pinned at v4 while v7 ships)
    are exactly what produce that flood.

    Reports proposed changes by default. Use -Apply to rewrite the version
    comment (and any already-resolved 40-hex commit SHA) in every template. The
    `<SHA>` placeholder that the scaffold resolves at pin time is preserved; the
    refreshed comment is the version the pinner should resolve that placeholder
    to. Review the diff before committing.

    Requires the GitHub CLI (gh), authenticated, to read release metadata.

.PARAMETER WorkflowsDir
    Directory of workflow templates to scan. Defaults to
    template/.github/workflows next to this script.

.PARAMETER MinimumReleaseAgeDays
    Quarantine window in days. A release newer than this is skipped in favour of
    the newest release old enough to have been vetted. Default: 30.

.PARAMETER Apply
    Write the refreshed version comments (and resolved SHAs) back to the
    templates. Without it, only a report is printed.

.EXAMPLE
    .\Update-ScaffoldActions.ps1                 # report only
    .\Update-ScaffoldActions.ps1 -Apply          # refresh the templates
#>

#Requires -Version 7.2
[CmdletBinding()]
param(
    [string] $WorkflowsDir = (Join-Path $PSScriptRoot 'template/.github/workflows'),
    [int]    $MinimumReleaseAgeDays = 30,
    [switch] $Apply,
    [Parameter(DontShow)] [switch] $SkipMain
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Step ([string] $msg) { Write-Host "  $msg" -ForegroundColor Cyan }
function Warn ([string] $msg) { Write-Host "  WARN $msg" -ForegroundColor Yellow }

# uses: owner/repo[/sub/path]@<ref> # vX[.Y[.Z]][suffix]
$usesRegex = [regex]'^(?<pre>\s*(?:-\s+)?uses:\s*)(?<action>[^@\s]+)@(?<ref>\S+)(?<mid>\s+#\s+)(?<ver>v\d[^\s]*)\s*$'

# owner/repo from owner/repo[/sub/...] (codeql-action/init -> codeql-action).
function Get-ActionRepo ([string] $action) {
    $parts = $action -split '/'
    if ($parts.Count -lt 2) { return $null }
    "$($parts[0])/$($parts[1])"
}

# [version] from a vX[.Y[.Z]] tag, padded to three components.
function ConvertTo-SortableVersion ([string] $tag) {
    $base = ($tag.TrimStart('v') -split '[-+]', 2)[0]
    $n = @($base -split '\.' | ForEach-Object { [int]$_ })
    while ($n.Count -lt 3) { $n += 0 }
    [version]::new($n[0], $n[1], $n[2])
}

# Newest stable vX.Y.Z release published at least $minAge days ago. Falls back to
# tags (no publish date, so no age gate) for actions that ship without releases.
function Resolve-ActionVersion ([string] $repo, [int] $minAge) {
    $cutoff = (Get-Date).ToUniversalTime().AddDays(-$minAge)

    $releases = @(gh api "repos/$repo/releases" --paginate `
            --jq '.[] | select(.draft==false and .prerelease==false) | "\(.tag_name)\t\(.published_at)"' 2>$null)

    $candidates = @()
    foreach ($line in $releases) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $tag, $published = $line -split "`t", 2
        if ($tag -notmatch '^v\d+(\.\d+){0,2}$') { continue }
        $pub = [datetime]::Parse($published).ToUniversalTime()
        if ($pub -gt $cutoff) { continue }
        $candidates += [pscustomobject]@{ Tag = $tag; Published = $pub }
    }

    $usedFallback = $false
    if (-not $candidates) {
        # No qualifying releases; fall back to stable tags (age cannot be checked).
        $tags = @(gh api "repos/$repo/tags" --paginate --jq '.[].name' 2>$null)
        foreach ($tag in $tags) {
            if ($tag -notmatch '^v\d+(\.\d+){0,2}$') { continue }
            $candidates += [pscustomobject]@{ Tag = $tag; Published = $null }
        }
        $usedFallback = $true
    }
    if (-not $candidates) { return $null }

    $best = $candidates | Sort-Object { ConvertTo-SortableVersion $_.Tag } -Descending | Select-Object -First 1
    $sha = (gh api "repos/$repo/commits/$($best.Tag)" --jq '.sha' 2>$null)
    if ([string]::IsNullOrWhiteSpace($sha)) { return $null }

    [pscustomobject]@{ Tag = $best.Tag; Sha = $sha; Fallback = $usedFallback }
}

# ---------------------------------------------------------------------------
# 1. discover every action reference in the templates
# ---------------------------------------------------------------------------

if ($SkipMain) { return }

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "gh (GitHub CLI) is required to resolve action releases. Install it and run 'gh auth login'."
}
if (-not (Test-Path $WorkflowsDir)) {
    throw "Workflows directory not found: $WorkflowsDir"
}

$files = @(Get-ChildItem -Path $WorkflowsDir -Recurse -File -Filter '*.tmpl' |
    Sort-Object FullName)
if (-not $files) { $files = @(Get-ChildItem -Path $WorkflowsDir -Recurse -File | Sort-Object FullName) }

$refs = @()   # one row per matching line
foreach ($file in $files) {
    $lineNo = 0
    foreach ($line in [System.IO.File]::ReadAllLines($file.FullName)) {
        $lineNo++
        $m = $usesRegex.Match($line)
        if (-not $m.Success) { continue }
        $repo = Get-ActionRepo $m.Groups['action'].Value
        if (-not $repo) { continue }
        $refs += [pscustomobject]@{
            File    = $file.FullName
            Line    = $lineNo
            Action  = $m.Groups['action'].Value
            Repo    = $repo
            CurRef  = $m.Groups['ref'].Value
            CurVer  = $m.Groups['ver'].Value
        }
    }
}

if (-not $refs) {
    Write-Host "No action references found under $WorkflowsDir." -ForegroundColor Yellow
    return
}

# ---------------------------------------------------------------------------
# 2. resolve the newest vetted release per distinct repo
# ---------------------------------------------------------------------------
Step "Resolving latest vetted releases (>= ${MinimumReleaseAgeDays}d old)..."
$resolved = @{}
foreach ($repo in ($refs.Repo | Sort-Object -Unique)) {
    $r = Resolve-ActionVersion $repo $MinimumReleaseAgeDays
    if (-not $r) { Warn "could not resolve a release for $repo - leaving as-is"; continue }
    if ($r.Fallback) { Warn "$repo has no dated releases - used tag $($r.Tag) without an age check" }
    $resolved[$repo] = $r
}

# ---------------------------------------------------------------------------
# 3. report, and (optionally) rewrite the templates
# ---------------------------------------------------------------------------
$changes = 0
$plan = @{}   # file -> ordered list of {Line, New}
foreach ($ref in $refs) {
    if (-not $resolved.ContainsKey($ref.Repo)) { continue }
    $target = $resolved[$ref.Repo]
    $newVer = $target.Tag
    $newRef = if ($ref.CurRef -match '^[0-9a-f]{40}$') { $target.Sha } else { $ref.CurRef }
    $verChanged = ($ref.CurVer -ne $newVer)
    $refChanged = ($ref.CurRef -ne $newRef)
    if (-not ($verChanged -or $refChanged)) { continue }

    $changes++
    $short = [System.IO.Path]::GetFileName($ref.File)
    Write-Host ("  {0,-22} {1,-28} {2,-9} -> {3}" -f $short, $ref.Action, $ref.CurVer, $newVer)

    if ($Apply) {
        if (-not $plan.ContainsKey($ref.File)) { $plan[$ref.File] = @() }
        $plan[$ref.File] += [pscustomobject]@{ Line = $ref.Line; Ref = $newRef; Ver = $newVer }
    }
}

if ($changes -eq 0) {
    Write-Host "All action pins are already current. Nothing to do." -ForegroundColor Green
    return
}

if (-not $Apply) {
    Write-Host ""
    Write-Host "Report only. Re-run with -Apply to rewrite the templates, then review the diff." -ForegroundColor Yellow
    return
}

foreach ($file in $plan.Keys) {
    # Preserve the file's existing newline style (LF vs CRLF). WriteAllLines would
    # otherwise force the current platform's newline (CRLF on Windows) and churn
    # every line across OSes.
    $raw = [System.IO.File]::ReadAllText($file)
    $nl = if ($raw.Contains("`r`n")) { "`r`n" } else { "`n" }
    $lines = [System.IO.File]::ReadAllLines($file)
    foreach ($edit in $plan[$file]) {
        $idx = $edit.Line - 1
        $m = $usesRegex.Match($lines[$idx])
        if (-not $m.Success) { continue }
        $lines[$idx] = "{0}{1}@{2}{3}{4}" -f `
            $m.Groups['pre'].Value, $m.Groups['action'].Value, $edit.Ref, $m.Groups['mid'].Value, $edit.Ver
    }
    [System.IO.File]::WriteAllText($file, ($lines -join $nl) + $nl)
}

Write-Host ""
Write-Host "Updated $changes reference(s) across $($plan.Keys.Count) file(s). Review the diff before committing." -ForegroundColor Green
