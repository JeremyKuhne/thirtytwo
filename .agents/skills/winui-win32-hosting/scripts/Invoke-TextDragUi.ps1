[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [ValidateSet('win-x64', 'win-arm64')]
    [string] $RuntimeIdentifier = 'win-x64',

    [string] $ArtifactDirectory = (Join-Path ([IO.Path]::GetTempPath()) "winui-text-drag-$([Guid]::NewGuid().ToString('N'))")
)

$ErrorActionPreference = 'Stop'
$skillRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$sourceAssetRoot = Join-Path $skillRoot 'assets'
$targetFramework = 'net10.0-windows10.0.17763.0'

if (-not $IsWindows) {
    throw 'WinUI text drag automation requires Windows.'
}

if ($RuntimeIdentifier -eq 'win-arm64' -and
    [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne
        [Runtime.InteropServices.Architecture]::Arm64) {
    throw 'Running win-arm64 UI automation requires an ARM64 PowerShell process.'
}

New-Item -ItemType Directory -Force -Path $ArtifactDirectory | Out-Null
$assetRoot = Join-Path `
    ([IO.Path]::GetTempPath()) `
    "winui-text-drag-workspace-$([Guid]::NewGuid().ToString('N'))"

$assetFiles = @(
    'Directory.Build.props'
    'minimal-host\app.manifest'
    'minimal-host\MinimalWinUIHost.csproj'
    'minimal-host\NativeMethods.json'
    'minimal-host\NativeMethods.txt'
    'minimal-host\Program.cs'
    'minimal-host\TextDragContent.cs'
    'minimal-host\WindowsAppSdkInterop.cs'
    'minimal-host\XamlApplication.cs'
    'direct-editor-drag-repro\DirectEditorDragRepro.csproj'
    'direct-editor-drag-repro\Program.cs'
    'direct-editor-drag-repro\ReproApplication.cs'
    'text-drag-content\DirectEditorDragContent.cs'
)

foreach ($relativePath in $assetFiles) {
    $sourcePath = Join-Path $sourceAssetRoot $relativePath
    $destinationPath = Join-Path $assetRoot $relativePath
    New-Item -ItemType Directory -Force -Path (Split-Path $destinationPath) | Out-Null
    Copy-Item -LiteralPath $sourcePath -Destination $destinationPath
}

$minimalHostProject = Join-Path $assetRoot 'minimal-host\MinimalWinUIHost.csproj'
$windowReproProject = Join-Path $assetRoot 'direct-editor-drag-repro\DirectEditorDragRepro.csproj'

foreach ($project in $minimalHostProject, $windowReproProject) {
    $buildOutput = & dotnet build $project `
        --configuration $Configuration `
        --runtime $RuntimeIdentifier `
        --nologo 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Building '$project' failed.$([Environment]::NewLine)$($buildOutput -join [Environment]::NewLine)"
    }
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

if (-not ('TextDragUiNativeV2' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class TextDragUiNativeV2
{
    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint firstThread, uint secondThread, bool attach);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr window);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("user32.dll")]
    private static extern IntPtr SetActiveWindow(IntPtr window);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr window);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    public static void ActivateWindow(IntPtr window)
    {
        uint processId;
        uint windowThread = GetWindowThreadProcessId(window, out processId);
        uint currentThread = GetCurrentThreadId();
        bool attached = currentThread != windowThread && AttachThreadInput(currentThread, windowThread, true);
        try
        {
            BringWindowToTop(window);
            SetForegroundWindow(window);
            SetActiveWindow(window);
            SetFocus(window);
        }
        finally
        {
            if (attached)
            {
                AttachThreadInput(currentThread, windowThread, false);
            }
        }
    }
}
'@
}

$mouseMove = 0x0001
$leftButtonDown = 0x0002
$leftButtonUp = 0x0004
$script:mouseButtonDown = $false

function Start-UiProcess {
    param(
        [Parameter(Mandatory)]
        [string] $Executable,

        [string[]] $ArgumentList = @()
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new($Executable)
    $startInfo.UseShellExecute = $true
    foreach ($argument in $ArgumentList) {
        $startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw "Failed to start '$Executable'."
    }

    try {
        $process.WaitForInputIdle(10000) | Out-Null
    }
    catch {
    }

    $ready = [Threading.SpinWait]::SpinUntil(
        [Func[bool]] {
            $process.Refresh()
            return $process.HasExited -or $process.MainWindowHandle -ne 0
        },
        10000)
    if (-not $ready -or $process.HasExited -or $process.MainWindowHandle -eq 0) {
        throw "'$Executable' exited or did not create a top-level window."
    }

    return $process
}

function Stop-UiProcess {
    param([Diagnostics.Process] $Process)

    if ($script:mouseButtonDown) {
        [TextDragUiNativeV2]::mouse_event($leftButtonUp, 0, 0, 0, [UIntPtr]::Zero)
        $script:mouseButtonDown = $false
    }

    if ($null -eq $Process -or $Process.HasExited) {
        return
    }

    if ($Process.CloseMainWindow() -and $Process.WaitForExit(5000)) {
        return
    }

    $Process.Kill($true)
    if (-not $Process.WaitForExit(5000)) {
        throw "Process $($Process.Id) did not terminate."
    }
}

function Get-AutomationRoot {
    param([Diagnostics.Process] $Process)

    return [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$Process.MainWindowHandle)
}

function Get-AutomationElement {
    param(
        [System.Windows.Automation.AutomationElement] $Root,
        [string] $AutomationId
    )

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
        $AutomationId)
    $element = $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $element) {
        throw "Automation element '$AutomationId' was not found."
    }

    return $element
}

function Get-ElementCenter {
    param([System.Windows.Automation.AutomationElement] $Element)

    $bounds = $Element.Current.BoundingRectangle
    if ([double]::IsInfinity($bounds.X) -or [double]::IsInfinity($bounds.Y)) {
        throw "Automation element '$($Element.Current.AutomationId)' is offscreen."
    }

    return [pscustomobject]@{
        X = [int]($bounds.X + [Math]::Min(150, $bounds.Width / 3))
        Y = [int]($bounds.Y + ($bounds.Height / 2))
    }
}

function Get-TextPattern {
    param([System.Windows.Automation.AutomationElement] $Element)

    $patternObject = $null
    if (-not $Element.TryGetCurrentPattern(
        [System.Windows.Automation.TextPattern]::Pattern,
        [ref] $patternObject)) {
        throw "'$($Element.Current.AutomationId)' does not expose TextPattern."
    }

    return [System.Windows.Automation.TextPattern] $patternObject
}

function Get-ValuePattern {
    param([System.Windows.Automation.AutomationElement] $Element)

    $patternObject = $null
    if (-not $Element.TryGetCurrentPattern(
        [System.Windows.Automation.ValuePattern]::Pattern,
        [ref] $patternObject)) {
        throw "'$($Element.Current.AutomationId)' does not expose ValuePattern."
    }

    return [System.Windows.Automation.ValuePattern] $patternObject
}

function Select-AllText {
    param([System.Windows.Automation.AutomationElement] $Element)

    $Element.SetFocus()
    $textPattern = Get-TextPattern $Element
    $textPattern.DocumentRange.Select()
    $selection = $textPattern.GetSelection()
    return $selection[0].GetText(-1)
}

function Collapse-TextSelection {
    param([System.Windows.Automation.AutomationElement] $Element)

    $Element.SetFocus()
    $textPattern = Get-TextPattern $Element
    $range = $textPattern.DocumentRange
    $range.MoveEndpointByRange(
        [System.Windows.Automation.Text.TextPatternRangeEndpoint]::End,
        $range,
        [System.Windows.Automation.Text.TextPatternRangeEndpoint]::Start)
    $range.Select()
}

function Get-SelectedText {
    param([System.Windows.Automation.AutomationElement] $Element)

    $selection = (Get-TextPattern $Element).GetSelection()
    return $selection[0].GetText(-1)
}

function Wait-ForElementName {
    param(
        [System.Windows.Automation.AutomationElement] $Element,
        [string] $Pattern,
        [int] $TimeoutMilliseconds = 5000,
        [switch] $AllowTimeout
    )

    if ($Element.Current.Name -match $Pattern) {
        return $true
    }

    $matched = [Threading.SpinWait]::SpinUntil(
        [Func[bool]] { return $Element.Current.Name -match $Pattern },
        $TimeoutMilliseconds)
    if ($matched) {
        return $true
    }

    if ($AllowTimeout) {
        return $false
    }

    throw "Timed out waiting for '$Pattern'; current value is '$($Element.Current.Name)'."
}

function Begin-Drag {
    param(
        [Diagnostics.Process] $Process,
        [System.Windows.Automation.AutomationElement] $Source,
        [switch] $UseTextContentArea
    )

    $start = Get-ElementCenter $Source
    if ($UseTextContentArea) {
        $bounds = $Source.Current.BoundingRectangle
        $start.Y = [int]($bounds.Bottom - 15)
    }

    [TextDragUiNativeV2]::ActivateWindow([IntPtr]$Process.MainWindowHandle)
    try {
        $Process.WaitForInputIdle(5000) | Out-Null
    }
    catch {
    }

    [TextDragUiNativeV2]::SetCursorPos($start.X, $start.Y) | Out-Null
    [TextDragUiNativeV2]::mouse_event($leftButtonDown, 0, 0, 0, [UIntPtr]::Zero)
    $script:mouseButtonDown = $true
    foreach ($unusedStep in 1..5) {
        [TextDragUiNativeV2]::mouse_event($mouseMove, 5, 3, 0, [UIntPtr]::Zero)
    }
}

function Begin-DragUntilStatus {
    param(
        [Diagnostics.Process] $Process,
        [System.Windows.Automation.AutomationElement] $Source,
        [System.Windows.Automation.AutomationElement] $Status,
        [string] $Pattern
    )

    foreach ($sourceAttempt in 1..3) {
        Begin-Drag $Process $Source
        if (Wait-ForElementName $Status $Pattern -TimeoutMilliseconds 1000 -AllowTimeout) {
            return
        }

        Release-Drag
        try {
            $Process.WaitForInputIdle(5000) | Out-Null
        }
        catch {
        }
    }

    throw "Source did not start the drag after three movement attempts; status is '$($Status.Current.Name)'."
}

function Move-DragTo {
    param([System.Windows.Automation.AutomationElement] $Target)

    $destination = Get-ElementCenter $Target
    $current = [System.Windows.Forms.Cursor]::Position
    foreach ($step in 1..8) {
        $x = [int]($current.X + (($destination.X - $current.X) * $step / 8))
        $y = [int]($current.Y + (($destination.Y - $current.Y) * $step / 8))
        [TextDragUiNativeV2]::SetCursorPos($x, $y) | Out-Null
    }
}

function Release-Drag {
    if ($script:mouseButtonDown) {
        [TextDragUiNativeV2]::mouse_event($leftButtonUp, 0, 0, 0, [UIntPtr]::Zero)
        $script:mouseButtonDown = $false
    }
}

function Invoke-SuccessfulDrag {
    param(
        [Diagnostics.Process] $Process,
        [System.Windows.Automation.AutomationElement] $Source,
        [System.Windows.Automation.AutomationElement] $Target,
        [System.Windows.Automation.AutomationElement] $Status,
        [string] $StartPattern,
        [string] $AcceptancePattern,
        [string] $CompletionPattern,
        [string] $ExpectedText
    )

    (Get-ValuePattern $Target).SetValue('')
    Begin-DragUntilStatus $Process $Source $Status $StartPattern
    try {
        $Process.WaitForInputIdle(5000) | Out-Null
    }
    catch {
    }

    $accepted = $false
    foreach ($targetAttempt in 1..3) {
        Move-DragTo $Target
        foreach ($unusedMove in 1..3) {
            [TextDragUiNativeV2]::mouse_event($mouseMove, 1, 1, 0, [UIntPtr]::Zero)
        }

        $accepted = Wait-ForElementName `
            $Status `
            $AcceptancePattern `
            -TimeoutMilliseconds 1000 `
            -AllowTimeout
        if ($accepted) {
            break
        }
    }

    if (-not $accepted) {
        throw "Target did not accept the active drag after three movement attempts."
    }

    Release-Drag
    Wait-ForElementName $Status $CompletionPattern | Out-Null
    $targetText = (Get-ValuePattern $Target).Current.Value
    if ($targetText -cne $ExpectedText) {
        throw "Expected target text '$ExpectedText', found '$targetText'."
    }

    return [pscustomobject]@{
        DragStarting = $true
        DropCompleted = $true
        TargetText = $targetText
        Status = $Status.Current.Name
    }
}

function Invoke-DirectHostMatrix {
    param(
        [Diagnostics.Process] $Process,
        [string] $HostName
    )

    $root = Get-AutomationRoot $Process
    $status = Get-AutomationElement $root 'DirectStatus'
    $target = Get-AutomationElement $root 'DirectDropTarget'
    $results = [Collections.Generic.List[object]]::new()

    $islandFocusAnchor = Get-AutomationElement $root 'DirectTextBoxSource'
    $islandFocusAnchor.SetFocus()
    try {
        $Process.WaitForInputIdle(5000) | Out-Null
    }
    catch {
    }

    $controlSource = Get-AutomationElement $root 'DirectTextBlockSource'
    $controlText = $controlSource.Current.Name
    $controlPoint = Get-ElementCenter $controlSource
    [TextDragUiNativeV2]::ActivateWindow([IntPtr]$Process.MainWindowHandle)
    [TextDragUiNativeV2]::SetCursorPos($controlPoint.X, $controlPoint.Y) | Out-Null
    [TextDragUiNativeV2]::mouse_event($leftButtonDown, 0, 0, 0, [UIntPtr]::Zero)
    [TextDragUiNativeV2]::mouse_event($leftButtonUp, 0, 0, 0, [UIntPtr]::Zero)
    try {
        $Process.WaitForInputIdle(5000) | Out-Null
    }
    catch {
    }

    $controlResult = Invoke-SuccessfulDrag `
        $Process `
        $controlSource `
        $target `
        $status `
        'TextBlock raised DragStarting' `
        'target accepted Copy' `
        'TextBlock completed with Copy' `
        $controlText
    $results.Add([pscustomobject]@{
        Host = $HostName
        Source = 'TextBlock'
        Expected = 'Copy'
        Passed = $true
        SelectionBefore = $null
        SelectionAfter = $null
        TargetText = $controlResult.TargetText
        Status = $controlResult.Status
    })

    foreach ($sourceCase in @(
        @{ AutomationId = 'DirectTextBoxSource'; Name = 'TextBox' },
        @{ AutomationId = 'DirectRichEditSource'; Name = 'RichEditBox' }
    )) {
        (Get-ValuePattern $target).SetValue('')
        $source = Get-AutomationElement $root $sourceCase.AutomationId
        $selectionBefore = Select-AllText $source
        Begin-Drag $Process $source -UseTextContentArea
        $dragStarting = Wait-ForElementName `
            $status `
            "$($sourceCase.Name) raised DragStarting" `
            -TimeoutMilliseconds 1000 `
            -AllowTimeout
        Release-Drag
        $selectionAfter = Get-SelectedText $source
        $targetText = (Get-ValuePattern $target).Current.Value
        $passed = -not $dragStarting `
            -and $targetText.Length -eq 0 `
            -and $selectionAfter -cne $selectionBefore
        if (-not $passed) {
            throw "$HostName $($sourceCase.Name) direct-source result differed: " `
                + "DragStarting=$dragStarting; Status='$($status.Current.Name)'; " `
                + "SelectionBefore='$selectionBefore'; SelectionAfter='$selectionAfter'; " `
                + "Target='$targetText'."
        }

        $results.Add([pscustomobject]@{
            Host = $HostName
            Source = $sourceCase.Name
            Expected = 'No DragStarting; editor selection gesture'
            Passed = $passed
            SelectionBefore = $selectionBefore
            SelectionAfter = $selectionAfter
            TargetText = $targetText
            Status = $status.Current.Name
        })
    }

    return $results
}

function Invoke-CanonicalMatrix {
    param([Diagnostics.Process] $Process)

    $root = Get-AutomationRoot $Process
    $status = Get-AutomationElement $root 'CanonicalStatus'
    $target = Get-AutomationElement $root 'CanonicalDropTarget'
    $results = [Collections.Generic.List[object]]::new()

    foreach ($sourceCase in @(
        @{
            EditorId = 'CanonicalTextBoxSource'
            HandleId = 'CanonicalTextBoxHandle'
            Name = 'TextBox handle'
        },
        @{
            EditorId = 'CanonicalRichEditSource'
            HandleId = 'CanonicalRichEditHandle'
            Name = 'RichEditBox handle'
        }
    )) {
        $editor = Get-AutomationElement $root $sourceCase.EditorId
        $handle = Get-AutomationElement $root $sourceCase.HandleId
        $selectedText = Select-AllText $editor
        $result = Invoke-SuccessfulDrag `
            $Process `
            $handle `
            $target `
            $status `
            'Dragging \d+ selected characters' `
            'Target accepted Copy' `
            'Drag completed with Copy' `
            $selectedText
        $results.Add([pscustomobject]@{
            Host = 'DesktopWindowXamlSource'
            Source = $sourceCase.Name
            Expected = 'Copy'
            Passed = $true
            SelectionBefore = $selectedText
            SelectionAfter = Get-SelectedText $editor
            TargetText = $result.TargetText
            Status = $result.Status
        })
    }

    $textBox = Get-AutomationElement $root 'CanonicalTextBoxSource'
    $textBoxHandle = Get-AutomationElement $root 'CanonicalTextBoxHandle'
    Collapse-TextSelection $textBox
    (Get-ValuePattern $target).SetValue('sentinel')
    Begin-DragUntilStatus $Process $textBoxHandle $status 'Select text before starting the drag'
    Release-Drag
    $targetText = (Get-ValuePattern $target).Current.Value
    if ($targetText -cne 'sentinel') {
        throw "Empty-selection cancellation changed the target to '$targetText'."
    }

    $results.Add([pscustomobject]@{
        Host = 'DesktopWindowXamlSource'
        Source = 'TextBox handle with empty selection'
        Expected = 'Canceled'
        Passed = $true
        SelectionBefore = ''
        SelectionAfter = Get-SelectedText $textBox
        TargetText = $targetText
        Status = $status.Current.Name
    })

    return $results
}

$minimalHostExecutable = Join-Path $assetRoot "minimal-host\bin\$Configuration\$targetFramework\$RuntimeIdentifier\MinimalWinUIHost.exe"
$windowReproExecutable = Join-Path $assetRoot "direct-editor-drag-repro\bin\$Configuration\$targetFramework\$RuntimeIdentifier\DirectEditorDragRepro.exe"
$allResults = [Collections.Generic.List[object]]::new()

$windowProcess = $null
try {
    $windowProcess = Start-UiProcess $windowReproExecutable
    foreach ($result in Invoke-DirectHostMatrix $windowProcess 'WinUI Window') {
        $allResults.Add($result)
    }
}
finally {
    Stop-UiProcess $windowProcess
}

$islandDiagnosticProcess = $null
try {
    $islandDiagnosticProcess = Start-UiProcess $minimalHostExecutable @('--direct-editor-repro')
    foreach ($result in Invoke-DirectHostMatrix $islandDiagnosticProcess 'DesktopWindowXamlSource') {
        $allResults.Add($result)
    }
}
finally {
    Stop-UiProcess $islandDiagnosticProcess
}

$canonicalProcess = $null
try {
    $canonicalProcess = Start-UiProcess $minimalHostExecutable
    foreach ($result in Invoke-CanonicalMatrix $canonicalProcess) {
        $allResults.Add($result)
    }
}
finally {
    Stop-UiProcess $canonicalProcess
}

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
$report = [ordered]@{
    TimestampUtc = [DateTimeOffset]::UtcNow
    WindowsAppSdk = '2.3.1'
    OSVersion = [Environment]::OSVersion.Version.ToString()
    OSArchitecture = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
    ProcessArchitecture = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    Framework = [Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
    Elevated = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    Configuration = $Configuration
    RuntimeIdentifier = $RuntimeIdentifier
    Cases = $allResults
}

$resultPath = Join-Path $ArtifactDirectory 'result.json'
$report | ConvertTo-Json -Depth 6 | Set-Content $resultPath -Encoding utf8NoBOM
Remove-Item $assetRoot -Recurse -Force
$resultPath