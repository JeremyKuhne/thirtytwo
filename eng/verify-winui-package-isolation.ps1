param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$artifacts = Join-Path $repoRoot 'artifacts/package-tests'
$packages = Join-Path $artifacts 'packages'
$restoreRoot = Join-Path $artifacts 'restore'
$nugetConfig = Join-Path $artifacts 'NuGet.Config'
$version = '0.0.0-local'

Remove-Item $artifacts -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $packages, $restoreRoot | Out-Null

$writerSettings = [System.Xml.XmlWriterSettings]::new()
$writerSettings.Indent = $true
$writer = [System.Xml.XmlWriter]::Create($nugetConfig, $writerSettings)
try {
    $writer.WriteStartDocument()
    $writer.WriteStartElement('configuration')
    $writer.WriteStartElement('packageSources')
    $writer.WriteStartElement('clear')
    $writer.WriteEndElement()
    foreach ($source in @(
        @{ Key = 'local'; Value = $packages },
        @{ Key = 'nuget'; Value = 'https://api.nuget.org/v3/index.json' },
        @{ Key = 'winsdk'; Value = 'https://pkgs.dev.azure.com/azure-public/winsdk/_packaging/CI/nuget/v3/index.json' }
    )) {
        $writer.WriteStartElement('add')
        $writer.WriteAttributeString('key', $source.Key)
        $writer.WriteAttributeString('value', $source.Value)
        $writer.WriteEndElement()
    }

    $writer.WriteEndElement()
    $writer.WriteEndElement()
    $writer.WriteEndDocument()
}
finally {
    $writer.Dispose()
}

dotnet pack (Join-Path $repoRoot 'src/thirtytwo/thirtytwo.csproj') `
    --configuration $Configuration `
    --output $packages `
    -p:Version=$version
if ($LASTEXITCODE -ne 0) { throw 'Packing thirtytwo failed.' }

dotnet pack (Join-Path $repoRoot 'src/thirtytwo.winui/thirtytwo.winui.csproj') `
    --configuration $Configuration `
    --output $packages `
    -p:Version=$version
if ($LASTEXITCODE -ne 0) { throw 'Packing thirtytwo.winui failed.' }

function Restore-And-ReadAssets([string] $ProjectName) {
    $projectDirectory = Join-Path $repoRoot "src/package-tests/$ProjectName"
    $project = Join-Path $projectDirectory "$ProjectName.csproj"
    $packageRoot = Join-Path $restoreRoot $ProjectName
    $intermediateRoot = Join-Path $artifacts "obj/$ProjectName/"
    $outputRoot = Join-Path $artifacts "bin/$ProjectName/"

    dotnet restore $project `
        --packages $packageRoot `
        --configfile $nugetConfig `
        -p:BaseIntermediateOutputPath=$intermediateRoot `
        -p:BaseOutputPath=$outputRoot `
        --force-evaluate | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Restoring $ProjectName failed." }

    dotnet build $project `
        --configuration $Configuration `
        --no-restore `
        -p:BaseIntermediateOutputPath=$intermediateRoot `
        -p:BaseOutputPath=$outputRoot | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Building $ProjectName failed." }

    return Get-Content (Join-Path $intermediateRoot 'project.assets.json') -Raw | ConvertFrom-Json
}

$coreAssets = Restore-And-ReadAssets 'CoreOnlyConsumer'
$coreLibraries = @($coreAssets.libraries.PSObject.Properties.Name)
if ($coreLibraries -match '^Microsoft\.WindowsAppSDK(?:\.|/)') {
    throw 'The core-only consumer restored a Microsoft.WindowsAppSDK package.'
}

if ($coreLibraries -match '^thirtytwo\.winui/') {
    throw 'The core-only consumer restored thirtytwo.winui.'
}

$winuiAssets = Restore-And-ReadAssets 'WinUIConsumer'
$winuiLibraries = @($winuiAssets.libraries.PSObject.Properties.Name)
if ($winuiLibraries -notcontains "thirtytwo/$version") {
    throw 'The WinUI consumer did not restore the local thirtytwo package.'
}

if ($winuiLibraries -notcontains "thirtytwo.winui/$version") {
    throw 'The WinUI consumer did not restore the local thirtytwo.winui package.'
}

$windowsAppSdk = @($winuiLibraries | Where-Object { $_ -like 'Microsoft.WindowsAppSDK/*' })
if ($windowsAppSdk.Count -ne 1 -or $windowsAppSdk[0] -ne 'Microsoft.WindowsAppSDK/2.3.1') {
    throw "Expected Microsoft.WindowsAppSDK/2.3.1, found '$($windowsAppSdk -join ', ')'."
}

$deploymentProperties = dotnet msbuild `
    (Join-Path $repoRoot 'src/thirtytwo.winui/thirtytwo.winui.csproj') `
    -getProperty:WindowsPackageType `
    -getProperty:WindowsAppSDKSelfContained `
    -getProperty:SelfContained `
    -getProperty:RuntimeIdentifier | ConvertFrom-Json

foreach ($propertyName in @('WindowsPackageType', 'WindowsAppSDKSelfContained', 'SelfContained', 'RuntimeIdentifier')) {
    if ($deploymentProperties.Properties.$propertyName) {
        throw "thirtytwo.winui forces $propertyName='$($deploymentProperties.Properties.$propertyName)'."
    }
}

Write-Host 'WinUI package isolation checks passed.'