[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [switch] $NoBuild,

    [switch] $MonitorReportOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
$project = Join-Path $PSScriptRoot 'DpiTesting.csproj'
$executable = Join-Path $repoRoot "artifacts\x64\$Configuration\DpiTesting\net10.0-windows10.0.17763.0\win-x64\DpiTesting.exe"
$resultsDirectory = Join-Path $repoRoot 'artifacts\test-results\WinUIDpiManual'
$results = [System.Collections.Generic.List[object]]::new()
$process = $null
$inputIdleTimeoutMilliseconds = 10000
$exitTimeoutMilliseconds = 5000

function Read-ManualResult {
    param(
        [Parameter(Mandatory)]
        [string] $Id,

        [Parameter(Mandatory)]
        [string] $Prompt
    )

    do {
        $answer = (Read-Host "$Prompt [y = pass, n = fail, s = skip]").Trim().ToLowerInvariant()
    } while ($answer -notin @('y', 'n', 's'))

    $status = switch ($answer) {
        'y' { 'Pass' }
        'n' { 'Fail' }
        default { 'Skipped' }
    }

    $notes = Read-Host 'Optional notes'
    return [pscustomobject]@{
        Id = $Id
        Status = $status
        Notes = $notes
    }
}

if (-not ('DpiMonitorProbe' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class DpiMonitorProbe
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        internal int X;
        internal int Y;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(Point point, uint flags);

    [DllImport("shcore.dll")]
    private static extern int GetScaleFactorForMonitor(IntPtr monitor, out int scale);

    public static int GetScalePercent(int x, int y)
    {
        IntPtr monitor = MonitorFromPoint(new Point { X = x, Y = y }, 2);
        int result = GetScaleFactorForMonitor(monitor, out int scale);
        if (result < 0)
        {
            Marshal.ThrowExceptionForHR(result);
        }

        return scale;
    }
}
'@
}

Push-Location $repoRoot
try {
    if (-not $NoBuild) {
        & dotnet build $project --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "DPI sample build failed with exit code $LASTEXITCODE."
        }
    }

    if (-not (Test-Path $executable)) {
        throw "DPI sample executable was not found at '$executable'."
    }

    Add-Type -AssemblyName System.Windows.Forms
    $monitors = [System.Collections.Generic.List[object]]::new()
    foreach ($screen in [System.Windows.Forms.Screen]::AllScreens) {
        $centerX = $screen.Bounds.Left + [int]($screen.Bounds.Width / 2)
        $centerY = $screen.Bounds.Top + [int]($screen.Bounds.Height / 2)
        $monitors.Add([pscustomobject]@{
            DeviceName = $screen.DeviceName
            Primary = $screen.Primary
            ScalePercent = [DpiMonitorProbe]::GetScalePercent($centerX, $centerY)
            Bounds = $screen.Bounds.ToString()
            WorkingArea = $screen.WorkingArea.ToString()
        })
    }

    if ($monitors.Count -lt 2) {
        throw 'This checklist requires at least two monitors.'
    }

    $scaleCount = @($monitors.ScalePercent | Sort-Object -Unique).Count
    if ($scaleCount -lt 2) {
        throw 'This checklist requires monitors configured with at least two different Scale values.'
    }

    Write-Host ''
    Write-Host 'Detected monitors:'
    $monitors | Format-Table -AutoSize
    Write-Host 'Windows Settings must configure at least two monitors with different Scale values.'
    Write-Host 'The app reports native DPI, XamlRoot scale, physical bounds, and ruler consistency.'
    Write-Host ''

    if ($MonitorReportOnly) {
        return
    }

    $process = Start-Process -FilePath $executable -PassThru
    $becameInputIdle = $process.WaitForInputIdle($inputIdleTimeoutMilliseconds)
    if (-not $becameInputIdle) {
        throw 'The DPI sample did not become input-idle within 10 seconds.'
    }

    $results.Add((Read-ManualResult 'initial-monitor' 'With the app fully on the first monitor, do native DPI and XAML scale agree and do both metric lines show MATCH?'))
    $results.Add((Read-ManualResult 'second-monitor' 'After dragging the app fully onto the second monitor, do DPI, XAML scale, pixel bounds, and transition count update without a blank frame?'))
    $results.Add((Read-ManualResult 'logical-rulers' 'On both monitors, do the native and XAML 240 x 120 rulers retain comparable logical size with crisp borders and no clipping?'))
    $results.Add((Read-ManualResult 'resize' 'On the second monitor, do repeated resize, maximize, restore, and snap operations keep host/XAML pixels at MATCH?'))
    $results.Add((Read-ManualResult 'popup' 'On each monitor, does the XAML combo-box popup open at the control, remain sharp, and accept selection?'))
    $results.Add((Read-ManualResult 'virtual-origin' 'If a monitor uses negative virtual-screen coordinates, can the app occupy it without clipping or incorrect bounds?'))
    $results.Add((Read-ManualResult 'transition-stress' 'After moving between monitors at least ten times, is the app responsive with no focus loss, blank island, crash, or growing size drift?'))

    $failed = @($results | Where-Object Status -eq 'Fail').Count
    $passed = @($results | Where-Object Status -eq 'Pass').Count
    $skipped = @($results | Where-Object Status -eq 'Skipped').Count
    $overall = if ($failed -gt 0) { 'Fail' } elseif ($passed -eq 0) { 'Inconclusive' } else { 'Pass' }

    New-Item -ItemType Directory -Force -Path $resultsDirectory | Out-Null
    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $reportPath = Join-Path $resultsDirectory "dpi-manual-$timestamp.json"
    $report = [pscustomobject]@{
        Timestamp = (Get-Date).ToString('o')
        Configuration = $Configuration
        OperatingSystem = [System.Environment]::OSVersion.VersionString
        ProcessId = $process.Id
        Monitors = $monitors
        Overall = $overall
        Passed = $passed
        Failed = $failed
        Skipped = $skipped
        Steps = $results
    }

    $report | ConvertTo-Json -Depth 6 | Set-Content -Path $reportPath -Encoding utf8
    Write-Host ''
    Write-Host "Overall result: $overall ($passed passed, $failed failed, $skipped skipped)"
    Write-Host "Report: $reportPath"
}
finally {
    if ($process -is [System.Diagnostics.Process]) {
        if (-not $process.HasExited) {
            [void]$process.CloseMainWindow()
            $exited = $process.WaitForExit($exitTimeoutMilliseconds)
            if (-not $exited) {
                Write-Warning 'DPI sample did not exit cleanly; terminating it.'
                Stop-Process -Id $process.Id -Force
            }
        }

        $process.Dispose()
    }

    Pop-Location
}
