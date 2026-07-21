<#
.SYNOPSIS
    Scaffolds a new .NET repository built to the engineering-baseline standard.

.DESCRIPTION
    Creates the local tree for a CLI tool, class library, or multi-target library,
    including centralized build configuration, test project, CI workflows,
    governance files, and agent-enablement stubs.

    The static file bodies live as real files under this script's `template/`
    folder and are rendered with simple {{TOKEN}} substitution. This script is the
    orchestration layer: it runs `dotnet new` for the project skeletons, renders
    the template folder, and applies the project-file hardening that `dotnet new`
    does not (Central Package Management fix-ups, packaging metadata, Source Link).

    Stops at the remote boundary: does not create the GitHub repository, configure
    branch protection, or publish any package. Those steps are printed at the end
    as a checklist for explicit approval.

.PARAMETER Root
    The directory to scaffold into. Created if it does not exist. Must be empty.

.PARAMETER Name
    The repository and solution name (for example "widgettool" or "MyLib").

.PARAMETER Archetype
    The kind of project to create:
      tool          - A dotnet tool (PackAsTool). Produces a console entry point.
      library       - A single-TFM NuGet library.
      multi-target  - A library targeting a modern TFM and a legacy TFM side by side.

.PARAMETER PackageId
    The NuGet package id (for example "WidgetTool" or "Contoso.MyLib").

.PARAMETER Description
    One-line package description, used in csproj and README.

.PARAMETER Owner
    The GitHub owner (user or org) for repository URLs and author metadata.

.PARAMETER ToolCommandName
    For archetype=tool: the CLI command name. Defaults to -Name in lowercase.

.PARAMETER Framework
    The primary (modern) target framework moniker. Default: net10.0.

.PARAMETER FrameworkLegacy
    For archetype=multi-target: the legacy TFM (for example "net481").

.PARAMETER License
    SPDX license identifier. Default: MIT. Only the MIT body is bundled; other
    licenses render every reference but skip the LICENSE file with a warning.

.PARAMETER TestRunner
    The test framework, both run on Microsoft Testing Platform. Accepted:
    mstest (default) or xunit (uses xunit.v3).

.PARAMETER Skills
    The agent skills to vendor into .agents/skills from the commons. Default: the
    universal starting tier (manage-skills, agent-files-review, create-pr,
    address-pr-feedback, security-review). Pass an empty array to skip vendoring.

.PARAMETER SkillsRef
    The release tag or commit SHA to pin vendored skills to. Default: the latest
    published release of -SkillsRepo, resolved at run time via gh.

.PARAMETER SkillsRepo
    The OWNER/REPO of the skills commons to vendor from. Default:
    JeremyKuhne/agent-skills.

.EXAMPLE
    .\New-DotnetRepo.ps1 -Root C:\src\widgettool -Name widgettool `
        -Archetype tool -PackageId WidgetTool -ToolCommandName widgettool `
        -Description "A sample command-line tool." -Owner JeremyKuhne
#>

#Requires -Version 7.2
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Root,
    [Parameter(Mandatory)] [string] $Name,
    [Parameter(Mandatory)] [ValidateSet('tool', 'library', 'multi-target')] [string] $Archetype,
    [Parameter(Mandatory)] [string] $PackageId,
    [Parameter(Mandatory)] [string] $Description,
    [Parameter(Mandatory)] [string] $Owner,
    [string] $ToolCommandName,
    [string] $Framework = 'net10.0',
    [string] $FrameworkLegacy,
    [string] $License = 'MIT',
    [ValidateSet('mstest', 'xunit')] [string] $TestRunner = 'mstest',
    [string[]] $Skills = @(
        'manage-skills',
        'agent-files-review',
        'create-pr',
        'address-pr-feedback',
        'security-review'),
    [string] $SkillsRef,
    [string] $SkillsRepo = 'JeremyKuhne/agent-skills'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# helpers
# ---------------------------------------------------------------------------

function Step ([string] $msg) { Write-Host "  $msg" -ForegroundColor Cyan }
function Done ([string] $msg) { Write-Host "  OK  $msg" -ForegroundColor Green }
function Warn ([string] $msg) { Write-Host "  WARN $msg" -ForegroundColor Yellow }

function New-Dir ([string] $path) {
    if ($path -and -not (Test-Path $path)) { New-Item -ItemType Directory -Path $path -Force | Out-Null }
}

function Invoke-Dotnet {
    $output = & dotnet @args 2>&1
    if ($LASTEXITCODE -ne 0) { throw "dotnet $args failed (exit $LASTEXITCODE):`n$output" }
}

# Render every file under $TemplateRoot into $DestRoot, stripping the trailing
# .tmpl and replacing {{TOKEN}} placeholders. {{TOKEN}} never collides with
# GitHub Actions ${{ ... }} expressions because only the literal token keys below
# are replaced.
function Expand-Template {
    param(
        [string] $TemplateRoot,
        [string] $DestRoot,
        [hashtable] $Tokens,
        [string[]] $SkipLeaf = @()
    )
    Get-ChildItem -LiteralPath $TemplateRoot -Recurse -File -Force | ForEach-Object {
        $rel = $_.FullName.Substring($TemplateRoot.Length).TrimStart([char]'\', [char]'/')
        if ($rel.EndsWith('.tmpl')) { $rel = $rel.Substring(0, $rel.Length - 5) }
        if ($SkipLeaf -contains (Split-Path $rel -Leaf)) { return }
        $text = Get-Content -LiteralPath $_.FullName -Raw
        foreach ($k in $Tokens.Keys) { $text = $text.Replace("{{$k}}", [string]$Tokens[$k]) }
        $dest = Join-Path $DestRoot $rel
        New-Dir (Split-Path $dest -Parent)
        Set-Content -LiteralPath $dest -Value $text -Encoding utf8NoBOM -NoNewline
    }
}

# ---------------------------------------------------------------------------
# locate the bundled template content folder (travels next to this script)
# ---------------------------------------------------------------------------

$TemplateRoot = Join-Path $PSScriptRoot 'template'
if (-not (Test-Path $TemplateRoot)) { throw "Template folder not found next to the script: $TemplateRoot" }

# ---------------------------------------------------------------------------
# derived values and template tokens
# ---------------------------------------------------------------------------

if ($Archetype -eq 'multi-target' -and -not $FrameworkLegacy) {
    throw "Archetype 'multi-target' requires -FrameworkLegacy (for example -FrameworkLegacy net472)."
}

$ToolCommandName = if ($ToolCommandName) { $ToolCommandName } else { $Name.ToLowerInvariant() }
$RepoUrl       = "https://github.com/$Owner/$Name"
$Year          = (Get-Date).Year
$IsMultiTarget = $Archetype -eq 'multi-target'
$IsTool        = $Archetype -eq 'tool'
$MainTfm       = $Framework
# Title-case only an all-lowercase name; preserve any name the user already cased
# (ToTitleCase would downcase internal capitals, for example MyLib -> Mylib).
$NameTitle     = if ($Name -cmatch '[A-Z]') { $Name } else { (Get-Culture).TextInfo.ToTitleCase($Name) }
$SdkVersion    = (& dotnet --version 2>&1).Trim()

# The generated global.json pins this exact SDK. If the host SDK is a prerelease,
# allowPrerelease must be true or the pinned version cannot resolve - keep them in
# sync rather than emitting a contradictory, unbuildable global.json.
$SdkIsPrerelease = $SdkVersion -match '-'
$AllowPrerelease = if ($SdkIsPrerelease) { 'true' } else { 'false' }
if ($SdkIsPrerelease) { Warn "host SDK $SdkVersion is a prerelease; global.json will set allowPrerelease=true" }

$LegacyTfmLine     = if ($IsMultiTarget) { "`n    <LegacyTfm>$FrameworkLegacy</LegacyTfm>" } else { '' }
$FrameworksDisplay = if ($IsMultiTarget) { "$Framework, $FrameworkLegacy" } else { $Framework }
$CiRunner          = if ($IsMultiTarget) { 'windows-latest' } else { 'ubuntu-latest' }

$installLines = if ($IsTool) {
    @('```shell', "dotnet tool install --global $PackageId", '```')
} else {
    @('```shell', "dotnet add package $PackageId", '```')
}
$InstallCommand = $installLines -join "`n"

# Pinned, vetted package versions come from the manifest - never 'latest'.
# Refresh deliberately with scripts/Update-ScaffoldVersions.ps1.
$versionsPath = Join-Path $PSScriptRoot 'versions.json'
if (-not (Test-Path $versionsPath)) { throw "Version manifest not found: $versionsPath" }
$pin = (Get-Content $versionsPath -Raw | ConvertFrom-Json).packages

$coreIds = @('MinVer', 'Microsoft.SourceLink.GitHub')
$testIds = switch ($TestRunner) {
    'mstest' { @('MSTest') }
    'xunit'  { @('xunit.v3', 'xunit.runner.visualstudio') }
}
$PackageVersionsXml = (@($coreIds; $testIds) | ForEach-Object {
    $v = $pin.$_
    if (-not $v) { throw "versions.json has no pinned version for '$_'. Run Update-ScaffoldVersions.ps1." }
    "    <PackageVersion Include=`"$_`" Version=`"$v`" />"
}) -join "`n"

$tokens = @{
    SDK_VERSION        = $SdkVersion
    ALLOW_PRERELEASE   = $AllowPrerelease
    FRAMEWORK          = $Framework
    LEGACY_TFM_LINE    = $LegacyTfmLine
    FRAMEWORKS_DISPLAY = $FrameworksDisplay
    OWNER              = $Owner
    YEAR               = "$Year"
    LICENSE            = $License
    REPO_URL           = $RepoUrl
    NAME               = $Name
    NAME_TITLE         = $NameTitle
    DESCRIPTION        = $Description
    PACKAGE_ID         = $PackageId
    ARCHETYPE          = $Archetype
    CI_RUNNER           = $CiRunner
    INSTALL_COMMAND    = $InstallCommand
    PACKAGE_VERSIONS   = $PackageVersionsXml
}

# License header for generated source. No per-file year - that is maintenance
# churn with no legal benefit; "and contributors" credits everyone without a roster.
$headerLines = @(
    "Copyright (c) $Owner and contributors."
    "SPDX-License-Identifier: $License"
    "See LICENSE in the project root for license information."
)
$fileHeaderComment = (($headerLines | ForEach-Object { "// $_" }) -join "`n") + "`n`n"

# ---------------------------------------------------------------------------
# 0. prepare root
# ---------------------------------------------------------------------------

Write-Host "`nScaffolding '$Name' ($Archetype) -> $Root`n" -ForegroundColor White

if (Test-Path -LiteralPath $Root) {
    $existing = @(Get-ChildItem -LiteralPath $Root -Force -ErrorAction SilentlyContinue)
    if ($existing.Count -gt 0) {
        throw "Root '$Root' is not empty ($($existing.Count) items). Remove it or choose a different path."
    }
} else {
    New-Item -ItemType Directory -Path $Root -Force | Out-Null
}

Push-Location -LiteralPath $Root
try {

# ---------------------------------------------------------------------------
# 1. project skeleton via dotnet new
# ---------------------------------------------------------------------------
Step 'solution'
Invoke-Dotnet new sln -n $Name --output . --format slnx

Step 'main project'
$srcDir = "src/$Name"
if ($IsTool) {
    Invoke-Dotnet new console -n $Name -o $srcDir --framework $MainTfm --no-restore
} else {
    Invoke-Dotnet new classlib -n $Name -o $srcDir --framework $MainTfm --no-restore
}
Invoke-Dotnet sln "$Name.slnx" add $srcDir --in-root

Step 'test project'
$testDir = "tests/$Name.tests"
$testProjectName = "$Name.Tests"
if ($TestRunner -eq 'mstest') {
    # No --coverage-tool: the hardening step below wipes the template's package
    # references, so the coverage package it would add is removed anyway.
    Invoke-Dotnet new mstest --test-runner Microsoft.Testing.Platform -n $testProjectName -o $testDir --framework $MainTfm --no-restore
} else {
    # The SDK ships no xunit v3 template; generate the v2 template for its file
    # structure and convert it to xunit.v3 + MTP during hardening below.
    Invoke-Dotnet new xunit -n $testProjectName -o $testDir --framework $MainTfm --no-restore
}
Invoke-Dotnet sln "$Name.slnx" add $testDir --in-root
Done 'project skeleton'

# Point every project's TargetFramework at the central $(MainTfm) property so
# Directory.Build.props is the single source of truth.
foreach ($csproj in Get-ChildItem -Recurse -Filter '*.csproj') {
    $doc = [xml](Get-Content $csproj.FullName -Raw)
    foreach ($node in $doc.SelectNodes('//*[local-name()="TargetFramework"]')) { $node.InnerText = '$(MainTfm)' }
    $doc.Save($csproj.FullName)
}

# ---------------------------------------------------------------------------
# 2. render the bundled template content folder
# ---------------------------------------------------------------------------
Step 'render template files'
$skip = @()
if ($License -ne 'MIT') { $skip += 'LICENSE' }
Expand-Template -TemplateRoot $TemplateRoot -DestRoot (Get-Location).Path -Tokens $tokens -SkipLeaf $skip
if ($License -ne 'MIT') {
    # Only the MIT body ships with the scaffold. Write a TODO placeholder so the
    # source headers and the README "See LICENSE" reference still resolve to a file.
    $licensePlaceholder = @(
        "Copyright (c) $Year $Owner and contributors."
        ''
        "SPDX-License-Identifier: $License"
        ''
        "TODO: Replace this placeholder with the full $License license text before"
        "publishing. The scaffold only bundles the MIT body; the canonical text is at"
        "https://spdx.org/licenses/$License.html."
    )
    Set-Content -LiteralPath 'LICENSE' -Value (($licensePlaceholder -join "`n") + "`n") -Encoding utf8NoBOM -NoNewline
    Warn "License '$License': wrote a TODO placeholder LICENSE - replace it with the full $License text before publishing."
}
Done 'template files rendered'

# ---------------------------------------------------------------------------
# 3. foundation: .editorconfig + .gitignore via dotnet new (good defaults)
# ---------------------------------------------------------------------------
Step 'foundation (.editorconfig, .gitignore)'
Invoke-Dotnet new gitignore --output .
Invoke-Dotnet new editorconfig --output .

# Enforce a license header on C# source (IDE0073). The file_header_template uses
# a literal \n as its line separator; warnings-as-errors then fails the build on
# any file missing the header.
$headerTemplate = $headerLines -join '\n'
Add-Content -LiteralPath '.editorconfig' -Value "`n[*.cs]`nfile_header_template = $headerTemplate`ndotnet_diagnostic.IDE0073.severity = warning`n"

# Keep the top-level artifacts output tree untracked - but only if the gitignore
# template does not already cover it. Matched per line (Get-Content strips line
# endings) so a CRLF gitignore does not defeat the check and duplicate the entry.
$ignoreLines = if (Test-Path -LiteralPath '.gitignore') { Get-Content -LiteralPath '.gitignore' } else { @() }
if ($ignoreLines -notcontains 'artifacts/') {
    Add-Content -LiteralPath '.gitignore' -Value "`n# Build outputs`nartifacts/`n"
}
Done 'foundation'

# ---------------------------------------------------------------------------
# 4. harden the main project file (packaging metadata, Source Link, MinVer)
# ---------------------------------------------------------------------------
Step "harden $srcDir/$Name.csproj"
$mainCsproj = "$srcDir/$Name.csproj"
$xml = [xml](Get-Content $mainCsproj -Raw)

$pg = $xml.CreateElement('PropertyGroup')
$pg.SetAttribute('Label', 'Package identity')
$props = [ordered]@{
    PackageId         = $PackageId
    Description       = $Description
    PackageTags       = if ($IsTool) { 'dotnet;dotnet-tool;cli' } else { 'dotnet;library' }
    PackageReadmeFile = 'README.md'
}
if ($IsTool) {
    $props['PackAsTool']      = 'true'
    $props['ToolCommandName'] = $ToolCommandName
}
foreach ($kv in $props.GetEnumerator()) {
    $el = $xml.CreateElement($kv.Key); $el.InnerText = $kv.Value; $pg.AppendChild($el) | Out-Null
}
# Keep the shippable project AOT/trim clean on its modern target: enables the
# trim, AOT, and single-file analyzers so the code composes into trimmed/AOT apps
# and avoids runtime reflection, even when AOT is never published. (Net-only, so
# it is scoped to the modern TFM - the analyzers do not apply to .NET Framework.)
$aot = $xml.CreateElement('IsAotCompatible')
$aot.SetAttribute('Condition', "'`$(TargetFramework)' == '`$(MainTfm)'")
$aot.InnerText = 'true'
$pg.AppendChild($aot) | Out-Null
$xml.Project.AppendChild($pg) | Out-Null

# README packed onto the gallery page.
$ig = $xml.CreateElement('ItemGroup')
$none = $xml.CreateElement('None')
$none.SetAttribute('Include', '$(MSBuildThisFileDirectory)../../README.md')
$none.SetAttribute('Pack', 'true')
$none.SetAttribute('PackagePath', '\')
$none.SetAttribute('Visible', 'false')
$ig.AppendChild($none) | Out-Null
$xml.Project.AppendChild($ig) | Out-Null

# MinVer + Source Link (versions come from Directory.Packages.props).
$ig2 = $xml.CreateElement('ItemGroup')
foreach ($ref in @(
    @{ Include = 'MinVer'; PrivateAssets = 'all' },
    @{ Include = 'Microsoft.SourceLink.GitHub'; PrivateAssets = 'All'; IncludeAssets = 'runtime; build; native; contentfiles; analyzers; buildtransitive' }
)) {
    $pr = $xml.CreateElement('PackageReference')
    $pr.SetAttribute('Include', $ref.Include)
    $pr.SetAttribute('PrivateAssets', $ref.PrivateAssets)
    if ($ref.ContainsKey('IncludeAssets')) { $pr.SetAttribute('IncludeAssets', $ref.IncludeAssets) }
    $ig2.AppendChild($pr) | Out-Null
}
$xml.Project.AppendChild($ig2) | Out-Null

if ($IsMultiTarget) {
    $tfmNode = $xml.SelectSingleNode('//*[local-name()="TargetFramework"]')
    if ($tfmNode) {
        $tfs = $xml.CreateElement('TargetFrameworks')
        $tfs.InnerText = '$(MainTfm);$(LegacyTfm)'
        $tfmNode.ParentNode.ReplaceChild($tfs, $tfmNode) | Out-Null
    }
}

$xml.Save((Resolve-Path $mainCsproj))
Done "$Name.csproj hardened"

if ($IsTool) {
    # Replace the console template's top-level statements with an explicit Program
    # class and Main method - a clearer, more discoverable entry point for a CLI.
    $rootNs = $Name -replace '[^\w.]', '_'
    if ($rootNs -match '^[0-9]') { $rootNs = "_$rootNs" }
    $programLines = @(
        "namespace $rootNs;"
        ''
        'internal sealed class Program'
        '{'
        '    private static void Main(string[] args)'
        '    {'
        '        Console.WriteLine("Hello, World!");'
        '    }'
        '}'
    )
    Set-Content -LiteralPath "$srcDir/Program.cs" -Value (($programLines -join "`n") + "`n") -Encoding utf8NoBOM -NoNewline
}
else {
    # Document the classlib placeholder so a library builds under
    # GenerateDocumentationFile + TreatWarningsAsErrors (CS1591).
    $classFile = "$srcDir/Class1.cs"
    if (Test-Path $classFile) {
        $doc = "/// <summary>`n/// Placeholder type - replace with the library's public API.`n/// </summary>`npublic class Class1"
        (Get-Content $classFile -Raw) -replace 'public class Class1', $doc |
            Set-Content -LiteralPath $classFile -Encoding utf8NoBOM -NoNewline
    }
}

# ---------------------------------------------------------------------------
# 5. harden the test project file (CPM fix-up, doc-gen off, project reference)
# ---------------------------------------------------------------------------
Step "harden $testDir/$testProjectName.csproj"
$testCsproj = "$testDir/$testProjectName.csproj"
$xml = [xml](Get-Content $testCsproj -Raw)
$proj = $xml.Project

# Drop the template's PackageReferences (packages/versions vary) and any now-empty
# ItemGroup, then add the MTP package set from the manifest (versionless; CPM pins).
foreach ($pr in @($xml.SelectNodes('//*[local-name()="PackageReference"]'))) {
    $grp = $pr.ParentNode; $grp.RemoveChild($pr) | Out-Null
    if (-not $grp.HasChildNodes) { $grp.ParentNode.RemoveChild($grp) | Out-Null }
}
$pkgGroup = $xml.CreateElement('ItemGroup')
foreach ($pkgId in $testIds) {
    $pr = $xml.CreateElement('PackageReference'); $pr.SetAttribute('Include', $pkgId)
    $pkgGroup.AppendChild($pr) | Out-Null
}
$proj.AppendChild($pkgGroup) | Out-Null

# Idempotently set the MTP + test-project properties (the mstest template already
# sets some of these, so update in place rather than duplicating). Keep XML-doc
# generation enabled because IDE0005 requires it when code style runs at build;
# suppress only CS1591 for the test surface.
$firstPg = $xml.SelectSingleNode('//*[local-name()="PropertyGroup"]')
if (-not $firstPg) { $firstPg = $xml.CreateElement('PropertyGroup'); $proj.InsertBefore($firstPg, $proj.FirstChild) | Out-Null }
$wanted = [ordered]@{ OutputType = 'Exe'; IsPackable = 'false'; GenerateDocumentationFile = 'true'; NoWarn = '$(NoWarn);CS1591' }
if ($TestRunner -eq 'mstest') { $wanted['EnableMSTestRunner'] = 'true' }
else { $wanted['UseMicrosoftTestingPlatformRunner'] = 'true' }
foreach ($kv in $wanted.GetEnumerator()) {
    $existing = $xml.SelectSingleNode("//*[local-name()='$($kv.Key)']")
    if ($existing) { $existing.InnerText = $kv.Value }
    else { $el = $xml.CreateElement($kv.Key); $el.InnerText = $kv.Value; $firstPg.AppendChild($el) | Out-Null }
}

# Reference the project under test.
$ig = $xml.CreateElement('ItemGroup')
$ref = $xml.CreateElement('ProjectReference'); $ref.SetAttribute('Include', "../../$srcDir/$Name.csproj")
$ig.AppendChild($ref) | Out-Null
$proj.AppendChild($ig) | Out-Null
$xml.Save((Resolve-Path $testCsproj))

# Run at the fullest level of concurrency by default.
if ($TestRunner -eq 'mstest') {
    # The MTP MSTest template emits MSTestSettings.cs with method-level parallelism;
    # overwrite it to make the worker count explicit (0 = all processors). Fully
    # qualify the attribute because MSTest is already imported implicitly and an
    # explicit using fails the generated repo's IDE0005 build gate.
    Set-Content -LiteralPath "$testDir/MSTestSettings.cs" -Encoding utf8NoBOM -NoNewline -Value @'
// Fullest MSTest parallelization: every test method may run concurrently across
// all available processors.
[assembly: Microsoft.VisualStudio.TestTools.UnitTesting.Parallelize(
    Workers = 0,
    Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.MethodLevel)]
'@
} else {
    Set-Content -LiteralPath "$testDir/Parallelism.cs" -Encoding utf8NoBOM -NoNewline -Value @'
// xunit.v3 parallelizes test collections by default; lift the thread cap so every
// available processor is used (the fullest level of concurrency).
[assembly: Xunit.CollectionBehavior(MaxParallelThreads = -1)]
'@
}
Done "$testProjectName.csproj hardened"

# ---------------------------------------------------------------------------
# 5b. prepend the license header to generated source (enforced by IDE0073)
# ---------------------------------------------------------------------------
Step 'file headers'
Get-ChildItem -Path 'src', 'tests' -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -notmatch '[\\/](obj|bin|artifacts)[\\/]' } |
    ForEach-Object {
        $body = Get-Content -LiteralPath $_.FullName -Raw
        if ($body -notmatch '(?m)^// Copyright \(c\)') {
            Set-Content -LiteralPath $_.FullName -Value ($fileHeaderComment + $body) -Encoding utf8NoBOM -NoNewline
        }
    }
Done 'file headers'

# ---------------------------------------------------------------------------
# 6. AGENTS.md -> .github/copilot-instructions.md mirror
# ---------------------------------------------------------------------------
# The mirror logic lives in the generated repo's tools/Sync-AgentInstructions.ps1
# so the same code regenerates the mirror here and verifies it in CI
# (.github/workflows/agent-files.yml).
Step 'agent mirror'
& (Join-Path $PWD 'tools/Sync-AgentInstructions.ps1')
Done 'agent mirror'

# ---------------------------------------------------------------------------
# 6b. vendor the starting agent-skill tier (domain 9)
# ---------------------------------------------------------------------------
# Pinned, provenance-stamped copies of the universal skill cores from the commons.
# A greenfield repo has nothing to reconcile, so this reduces to "vendor the tier".
# Graceful: install with `gh skill` when available, otherwise record the exact
# pinned commands in the final summary so the scaffold never hard-fails offline.
$skillSummary = ''
if ($Skills -and $Skills.Count -gt 0) {
    Step 'vendor starting skills'
    $skillsDir = '.agents/skills'
    New-Dir $skillsDir
    $destAbs = Join-Path (Get-Location).Path $skillsDir

    $gh = Get-Command gh -ErrorAction SilentlyContinue
    $ref = $SkillsRef
    if ($gh -and -not $ref) {
        $ref = (& gh release view --repo $SkillsRepo --json tagName --jq '.tagName' 2>$null)
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($ref)) { $ref = $null }
    }
    $ghSkill = $false
    if ($gh) { & gh skill --help *> $null; $ghSkill = ($LASTEXITCODE -eq 0) }

    $vendored = @()
    $pending = @()
    if ($ghSkill -and $ref) {
        foreach ($s in $Skills) {
            & gh skill install $SkillsRepo $s --pin $ref --dir $destAbs --agent github-copilot --force *> $null
            if ($LASTEXITCODE -eq 0) { $vendored += $s; Done "vendored $s ($ref)" }
            else { $pending += $s; Warn "could not vendor $s - recorded as a manual step" }
        }
    }
    else {
        $pending = @($Skills)
        $reason = if (-not $gh) { 'gh not found' } elseif (-not $ghSkill) { 'gh skill unavailable' } else { 'could not resolve a release ref' }
        Warn "skill vendoring deferred ($reason) - commands are in the final summary"
    }

    # A short catalog documents the intended set and how to manage it even when the
    # install was deferred; gh skill writes per-skill provenance into frontmatter.
    $pin = if ($ref) { $ref } else { '<latest release>' }
    $readmePath = Join-Path $skillsDir 'README.md'
    if (-not (Test-Path $readmePath)) {
        $lines = @(
            '# Vendored agent skills'
            ''
            "Pinned, provenance-stamped copies of shared skill cores from $SkillsRepo,"
            'each with a thin repo-specific overlay. Manage them with the manage-skills'
            'skill (find / build / update); never edit a vendored core silently - record'
            'every change as upstreamed, moved to the overlay, or a tracked divergence.'
            ''
            '| Skill | Pin |'
            '| ----- | --- |'
        ) + ($Skills | ForEach-Object { "| $_ | $pin |" })
        Set-Content -LiteralPath $readmePath -Value (($lines -join "`n") + "`n") -Encoding utf8NoBOM -NoNewline
    }

    if ($vendored.Count -gt 0) {
        $skillSummary += "`n  Review and commit the vendored skills under .agents/skills (pinned $ref):`n" +
            (($vendored | ForEach-Object { "    - $_" }) -join "`n") + "`n"
    }
    if ($pending.Count -gt 0) {
        $pinCmd = if ($ref) { $ref } else { 'vX.Y.Z' }
        $cmds = ($pending | ForEach-Object { "    gh skill install $SkillsRepo $_ --pin $pinCmd --agent github-copilot" }) -join "`n"
        $skillSummary += "`n  Vendor the starting skill tier (needs gh skill), then commit .agents/skills:`n$cmds`n"
    }
    Done 'starting skills'
}

# ---------------------------------------------------------------------------
# 7. final restore (packages now centrally pinned)
# ---------------------------------------------------------------------------
Step 'dotnet restore'
Invoke-Dotnet restore
Done 'restore'

# ---------------------------------------------------------------------------
# done - print the remote boundary checklist
# ---------------------------------------------------------------------------

Write-Host @"

Local scaffold complete. Before starting remote setup, verify:
  dotnet build -c Release
  dotnet test  -c Release
$skillSummary
Then, with explicit approval for each step, run:

  REMOTE SETUP CHECKLIST
  ----------------------
  1. Replace <SHA> placeholders in .github/workflows/*.yml with full commit SHAs
     (use Dependabot or https://app.stepsecurity.io to resolve them).

  2. Create the repository (choose visibility - default to private):
       gh repo create $Owner/$Name --private --source . --remote origin
     (use --public instead to publish it openly)

  3. git init && git add -A && git commit -m "Initial scaffold"
     git branch -M main
     git push -u origin main

  4. For a private repository, enable GitHub Code Security before the first pull
     request. If it is unavailable, replace CodeQL with a supported scanner.

  5. Branch protection: require the build status check, branches up to date or a
     merge queue, and either CodeQL code-scanning results at documented
     thresholds or the replacement scanner's status check. Require pull requests
     with no bypass path; block force-push and deletion. If a direct or bypass
     push is required, restore default-branch push validation in ci.yml,
     codeql.yml or its replacement, and agent-files.yml first.

  6. Enable secret scanning and push protection (GitHub repo Settings > Security).

  7. Register trusted-publishing policy on nuget.org before the first publish:
     Owner: $Owner  Repo: $Name  Workflow: publish.yml
     https://www.nuget.org/account/transform/trusted-publishers

"@ -ForegroundColor White

} finally {
    Pop-Location
}
