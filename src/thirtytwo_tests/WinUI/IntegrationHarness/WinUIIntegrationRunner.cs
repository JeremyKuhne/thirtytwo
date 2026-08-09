// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI.IntegrationHarness;

internal sealed class WinUIIntegrationRunner
{
    private const int MaximumWindowHandleCount = 512;
    private const int MaximumStandardErrorLength = 1024 * 1024;
    private static readonly TimeSpan s_cleanupTimeout = TimeSpan.FromSeconds(5);

    private static readonly JsonSerializerOptions s_resultJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _artifactRoot;
    private readonly string? _scenarioExecutableOverride;

    internal WinUIIntegrationRunner()
        : this(Path.Combine(
            GetArtifactsDirectory().FullName,
            "test-results",
            "WinUIIntegrationHarness",
            GetConfigurationName()))
    {
    }

    internal WinUIIntegrationRunner(string artifactRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactRoot);
        _artifactRoot = Path.GetFullPath(artifactRoot);
    }

    internal WinUIIntegrationRunner(string scenarioExecutable, string artifactRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioExecutable);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactRoot);

        _scenarioExecutableOverride = Path.GetFullPath(scenarioExecutable);
        _artifactRoot = Path.GetFullPath(artifactRoot);
    }

    internal async Task<WinUIIntegrationResult> RunAsync(
        WinUIIntegrationScenario scenario,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        string scenarioExecutable = _scenarioExecutableOverride ?? FindScenarioExecutable(scenario);
        if (!File.Exists(scenarioExecutable))
        {
            throw new FileNotFoundException("The WinUI scenario executable was not built.", scenarioExecutable);
        }

        string scenarioName = GetScenarioName(scenario);
        string artifactDirectory = Path.Combine(_artifactRoot, $"{scenarioName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(artifactDirectory);
        string resultPath = Path.Combine(artifactDirectory, "result.json");
        string standardOutputPath = Path.Combine(artifactDirectory, "stdout.log");
        string standardErrorPath = Path.Combine(artifactDirectory, "stderr.log");

        ProcessStartInfo startInfo = new()
        {
            FileName = scenarioExecutable,
            WorkingDirectory = Path.GetDirectoryName(scenarioExecutable)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("--scenario");
        startInfo.ArgumentList.Add(scenarioName);

        using Process process = new() { StartInfo = startInfo };
        Stopwatch stopwatch = Stopwatch.StartNew();
        if (!process.Start())
        {
            throw new InvalidOperationException("ControlHost did not start.");
        }

        int processId = process.Id;
        using CancellationTokenSource streamCancellation = new();
        ScenarioOutputReader outputReader = new(scenarioName, processId);
        BoundedTextReader errorReader = new(MaximumStandardErrorLength);
        Task outputTask = outputReader.ReadAsync(process.StandardOutput, streamCancellation.Token);
        Task errorTask = errorReader.ReadAsync(process.StandardError, streamCancellation.Token);
        Task exitTask = process.WaitForExitAsync(CancellationToken.None);
        Task timeoutTask = Task.Delay(timeout, CancellationToken.None);
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)
            : Task.Delay(Timeout.InfiniteTimeSpan, CancellationToken.None);

        bool timedOut = false;
        IReadOnlyList<long> windowHandles = [];
        UiaSnapshot? uia = null;
        ScreenshotSnapshot? screenshot = null;
        Exception? captureFailure = null;
        List<string> cleanupErrors = [];

        try
        {
            Task first = await Task.WhenAny(exitTask, outputReader.Ready, timeoutTask, cancellationTask).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (first == timeoutTask)
            {
                timedOut = true;
            }
            else if (first == outputReader.Ready)
            {
                WinUIIntegrationEvent ready = await outputReader.Ready.ConfigureAwait(false);
                try
                {
                    WindowHandleValidation.Validate(ready.WindowHandle, processId, ready.ThreadId);
                    windowHandles = CaptureWindowHandles(ready.WindowHandle, processId);
                    switch (scenario)
                    {
                        case WinUIIntegrationScenario.UiaTree:
                        case WinUIIntegrationScenario.HostAccessibility:
                            Task<(UiaSnapshot Uia, ScreenshotSnapshot Screenshot)> captureTask = Task.Run(
                                () => CaptureArtifactsAsync(
                                    ready.WindowHandle,
                                    processId,
                                    artifactDirectory));
                            Task captureCompletion = await Task.WhenAny(
                                captureTask,
                                timeoutTask,
                                cancellationTask).ConfigureAwait(false);
                            if (captureCompletion != captureTask)
                            {
                                ObserveLateFailure(captureTask);
                                cancellationToken.ThrowIfCancellationRequested();
                                timedOut = true;
                                break;
                            }

                            (uia, screenshot) = await captureTask.ConfigureAwait(false);
                            RequestClose(ready.WindowHandle, processId);
                            break;
                        case WinUIIntegrationScenario.RawAirspace:
                        case WinUIIntegrationScenario.RawScrolling:
                        case WinUIIntegrationScenario.HostAirspace:
                        case WinUIIntegrationScenario.HostScrolling:
                            Task<WinUIIntegrationEvent> captureReadyTask = outputReader.CaptureReady;
                            Task captureReadyCompletion = await Task.WhenAny(
                                captureReadyTask,
                                outputReader.ProtocolFailure,
                                exitTask,
                                timeoutTask,
                                cancellationTask).ConfigureAwait(false);
                            cancellationToken.ThrowIfCancellationRequested();
                            if (captureReadyCompletion == timeoutTask)
                            {
                                timedOut = true;
                                break;
                            }

                            if (captureReadyCompletion == exitTask)
                            {
                                WinUIIntegrationEvent? captureFailed = outputReader.Events.LastOrDefault(
                                    entry => entry.Event == "capture-failed");
                                captureFailure = new InvalidOperationException(
                                    captureFailed?.Message
                                        ?? "The capture scenario exited before reporting capture-ready.");
                                break;
                            }

                            if (captureReadyCompletion == outputReader.ProtocolFailure)
                            {
                                captureFailure = new InvalidOperationException(
                                    $"The capture protocol failed: {await outputReader.ProtocolFailure.ConfigureAwait(false)}");
                                break;
                            }

                            WinUIIntegrationEvent captureReady = await captureReadyTask.ConfigureAwait(false);
                            WindowHandleValidation.Validate(captureReady.WindowHandle, processId, captureReady.ThreadId);
                            windowHandles = CaptureWindowHandles(captureReady.WindowHandle, processId);
                            Task<ScreenshotSnapshot> screenshotTask = Task.Run(
                                () => ScreenshotCapture.Capture(
                                    captureReady.WindowHandle,
                                    processId,
                                    Path.Combine(artifactDirectory, "window.png")));
                            Task screenshotCompletion = await Task.WhenAny(
                                screenshotTask,
                                timeoutTask,
                                cancellationTask).ConfigureAwait(false);
                            if (screenshotCompletion != screenshotTask)
                            {
                                ObserveLateFailure(screenshotTask);
                                cancellationToken.ThrowIfCancellationRequested();
                                timedOut = true;
                                break;
                            }

                            screenshot = await screenshotTask.ConfigureAwait(false);
                            RequestClose(captureReady.WindowHandle, processId);
                            break;
                        case WinUIIntegrationScenario.NormalClose:
                            RequestClose(ready.WindowHandle, processId);
                            break;
                        case WinUIIntegrationScenario.Startup:
                        case WinUIIntegrationScenario.ShutdownTimeout:
                        case WinUIIntegrationScenario.EnvironmentOwned:
                        case WinUIIntegrationScenario.EnvironmentBorrowed:
                        case WinUIIntegrationScenario.EnvironmentComposition:
                        case WinUIIntegrationScenario.EnvironmentMultipleLeases:
                        case WinUIIntegrationScenario.EnvironmentCompatibleApplication:
                        case WinUIIntegrationScenario.EnvironmentIncompatibleApplication:
                        case WinUIIntegrationScenario.EnvironmentMtaRejected:
                        case WinUIIntegrationScenario.EnvironmentWrongThreadRejected:
                        case WinUIIntegrationScenario.EnvironmentSecondThreadRejected:
                        case WinUIIntegrationScenario.EnvironmentFinalRelease:
                        case WinUIIntegrationScenario.HostBasic:
                        case WinUIIntegrationScenario.HostColorPicker:
                        case WinUIIntegrationScenario.HostTextEditors:
                        case WinUIIntegrationScenario.HostStress:
                        case WinUIIntegrationScenario.HostMultiple:
                        case WinUIIntegrationScenario.HostLayout:
                        case WinUIIntegrationScenario.HostReparent:
                        case WinUIIntegrationScenario.HostReplacement:
                        case WinUIIntegrationScenario.HostPopupClose:
                        case WinUIIntegrationScenario.HostShutdownCleanup:
                        case WinUIIntegrationScenario.FocusTraversal:
                        case WinUIIntegrationScenario.InputSemantics:
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown WinUI integration scenario '{scenario}'.");
                    }
                }
                catch (Exception exception)
                {
                    captureFailure = exception;
                }

                if (captureFailure is null && !timedOut)
                {
                    Task completion = await Task.WhenAny(exitTask, timeoutTask, cancellationTask).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    timedOut = completion == timeoutTask;
                }
            }
        }
        finally
        {
            if (timedOut || captureFailure is not null || cancellationToken.IsCancellationRequested)
            {
                TryTerminateProcessTree(process, cleanupErrors);
            }

            bool processExited = await WaitForCleanupAsync(
                exitTask,
                s_cleanupTimeout,
                "Process exit",
                cleanupErrors).ConfigureAwait(false);
            if (!processExited)
            {
                TryTerminateProcessTree(process, cleanupErrors);
                await WaitForCleanupAsync(
                    exitTask,
                    s_cleanupTimeout,
                    "Process exit after termination",
                    cleanupErrors).ConfigureAwait(false);
            }

            if (!processExited)
            {
                streamCancellation.Cancel();
            }

            bool outputDrained = await WaitForCleanupAsync(
                outputTask,
                s_cleanupTimeout,
                "Standard output drain",
                cleanupErrors).ConfigureAwait(false);
            bool errorDrained = await WaitForCleanupAsync(
                errorTask,
                s_cleanupTimeout,
                "Standard error drain",
                cleanupErrors).ConfigureAwait(false);
            if (!outputDrained || !errorDrained)
            {
                streamCancellation.Cancel();
                if (!outputDrained)
                {
                    await WaitForCleanupAsync(
                        outputTask,
                        s_cleanupTimeout,
                        "Standard output drain after cancellation",
                        cleanupErrors).ConfigureAwait(false);
                }

                if (!errorDrained)
                {
                    await WaitForCleanupAsync(
                        errorTask,
                        s_cleanupTimeout,
                        "Standard error drain after cancellation",
                        cleanupErrors).ConfigureAwait(false);
                }
            }
        }

        string standardError = errorReader.Text;
        string standardOutput = outputReader.StandardOutput;
        WinUIIntegrationEvent[] eventSnapshot = [.. outputReader.Events];
        List<string> protocolErrorSnapshot = [.. outputReader.ProtocolErrors];
        if (errorReader.Truncated)
        {
            protocolErrorSnapshot.Add(
                $"Standard error exceeded {MaximumStandardErrorLength} retained characters.");
        }

        int? exitCode = GetExitCode(process);
        WinUIIntegrationEvent? readyEvent = eventSnapshot.FirstOrDefault(entry => entry.Event == "ready");
        WinUIIntegrationEvent? lastEvent = eventSnapshot.LastOrDefault();
        if (windowHandles.Count == 0 && readyEvent is not null)
        {
            windowHandles = [readyEvent.WindowHandle];
        }

        string? dumpPath = Directory.EnumerateFiles(artifactDirectory, "*.dmp").FirstOrDefault();
        string? diagnosticMessage = CreateDiagnosticMessage(
            scenarioName,
            processId,
            readyEvent,
            windowHandles,
            lastEvent,
            timeout,
            timedOut,
            exitCode,
            captureFailure,
            protocolErrorSnapshot,
            cleanupErrors,
            dumpPath);

        WinUIIntegrationResult result = new(
            scenarioName,
            processId,
            readyEvent?.ThreadId ?? 0,
            readyEvent?.WindowHandle ?? 0,
            windowHandles,
            exitCode,
            timedOut,
            stopwatch.Elapsed,
            eventSnapshot,
            standardOutput,
            standardError,
            lastEvent?.Event,
            diagnosticMessage,
            artifactDirectory,
            resultPath,
            dumpPath,
            uia,
            screenshot);

        await File.WriteAllTextAsync(standardOutputPath, standardOutput, CancellationToken.None).ConfigureAwait(false);
        await File.WriteAllTextAsync(standardErrorPath, standardError, CancellationToken.None).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            resultPath,
            JsonSerializer.Serialize(result, s_resultJsonOptions),
            CancellationToken.None).ConfigureAwait(false);

        return result;
    }

    private static async Task<(UiaSnapshot Uia, ScreenshotSnapshot Screenshot)> CaptureArtifactsAsync(
        long windowHandle,
        int expectedProcessId,
        string artifactDirectory)
    {
        UiaSnapshot uia = await UiaCapture.CaptureAsync(
            windowHandle,
            expectedProcessId,
            TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        ScreenshotSnapshot screenshot = ScreenshotCapture.Capture(
            windowHandle,
            expectedProcessId,
            Path.Combine(artifactDirectory, "window.png"));
        return (uia, screenshot);
    }

    private static void ObserveLateFailure(Task task)
    {
        _ = task.ContinueWith(
            static completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    private static void TryTerminateProcessTree(Process process, List<string> cleanupErrors)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (Exception exception)
        {
            cleanupErrors.Add($"Process-tree termination failed: {exception}");
        }
    }

    internal static async Task<bool> WaitForCleanupAsync(
        Task task,
        TimeSpan timeout,
        string operation,
        List<string> cleanupErrors)
    {
        try
        {
            await task.WaitAsync(timeout).ConfigureAwait(false);
            return true;
        }
        catch (TimeoutException)
        {
            cleanupErrors.Add($"{operation} did not complete within {timeout}.");
            return false;
        }
        catch (Exception exception)
        {
            cleanupErrors.Add($"{operation} failed: {exception}");
            return false;
        }
    }

    internal static int? GetExitCode(Process process)
    {
        try
        {
            return process.HasExited ? process.ExitCode : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static void RequestClose(long windowHandle, int expectedProcessId)
    {
        HWND window = WindowHandleValidation.Validate(windowHandle, expectedProcessId);
        if (!PInvoke.PostMessage(window, Interop.WM_CLOSE, default, default))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    private static string? CreateDiagnosticMessage(
        string scenario,
        int processId,
        WinUIIntegrationEvent? ready,
        IReadOnlyList<long> windowHandles,
        WinUIIntegrationEvent? last,
        TimeSpan timeout,
        bool timedOut,
        int? exitCode,
        Exception? captureFailure,
        IReadOnlyList<string> protocolErrors,
        IReadOnlyList<string> cleanupErrors,
        string? dumpPath)
    {
        string handles = windowHandles.Count == 0
            ? FormatHandle(ready?.WindowHandle ?? 0)
            : string.Join(", ", windowHandles.Select(FormatHandle));
        string identity = $"process {processId}, thread {ready?.ThreadId ?? 0}, HWNDs [{handles}]";
        string lastEvent = last?.Event ?? "<none>";
        string dump = dumpPath ?? "<none>";
        string cleanup = cleanupErrors.Count == 0
            ? string.Empty
            : $" Cleanup: {string.Join(" | ", cleanupErrors)}";

        if (timedOut)
        {
            return $"Scenario '{scenario}' ({identity}) timed out after {timeout}. Last event: '{lastEvent}'. Dump: '{dump}'.{cleanup}";
        }

        if (captureFailure is not null)
        {
            return $"Scenario '{scenario}' ({identity}) capture failed after event '{lastEvent}': {captureFailure}{cleanup}";
        }

        if (protocolErrors.Count > 0)
        {
            return $"Scenario '{scenario}' ({identity}) emitted invalid JSON: {string.Join(" | ", protocolErrors)}{cleanup}";
        }

        if (cleanupErrors.Count > 0)
        {
            return $"Scenario '{scenario}' ({identity}) cleanup was incomplete after event '{lastEvent}'.{cleanup}";
        }

        if (ready is null)
        {
            return $"Scenario '{scenario}' (process {processId}) exited with code {FormatExitCode(exitCode)} before reporting ready. Last event: '{lastEvent}'.";
        }

        if (exitCode is not 0)
        {
            return $"Scenario '{scenario}' ({identity}) exited with code {FormatExitCode(exitCode)}. Last event: '{lastEvent}'.";
        }

        return null;
    }

    private static string FormatHandle(long windowHandle)
        => $"0x{windowHandle.ToString("x", CultureInfo.InvariantCulture)}";

    private static string FormatExitCode(int? exitCode)
        => exitCode?.ToString(CultureInfo.InvariantCulture) ?? "<unavailable>";

    private static unsafe IReadOnlyList<long> CaptureWindowHandles(long rootWindowHandle, int expectedProcessId)
    {
        List<long> handles = [rootWindowHandle];
        HWND root = WindowHandleValidation.Validate(rootWindowHandle, expectedProcessId);
        root.EnumerateChildWindows(child =>
        {
            if (handles.Count >= MaximumWindowHandleCount)
            {
                return false;
            }

            WindowHandleValidation.Validate((long)child.Value, expectedProcessId);
            handles.Add((long)child.Value);
            return true;
        });

        return handles;
    }

    private static string GetScenarioName(WinUIIntegrationScenario scenario) => scenario switch
    {
        WinUIIntegrationScenario.Startup => "startup",
        WinUIIntegrationScenario.UiaTree => "uia-tree",
        WinUIIntegrationScenario.NormalClose => "normal-close",
        WinUIIntegrationScenario.ShutdownTimeout => "shutdown-timeout",
        WinUIIntegrationScenario.RawAirspace => "airspace",
        WinUIIntegrationScenario.RawScrolling => "scrolling",
        WinUIIntegrationScenario.EnvironmentOwned => "environment-owned",
        WinUIIntegrationScenario.EnvironmentBorrowed => "environment-borrowed",
        WinUIIntegrationScenario.EnvironmentComposition => "environment-composition",
        WinUIIntegrationScenario.EnvironmentMultipleLeases => "environment-multiple-leases",
        WinUIIntegrationScenario.EnvironmentCompatibleApplication => "environment-compatible-application",
        WinUIIntegrationScenario.EnvironmentIncompatibleApplication => "environment-incompatible-application",
        WinUIIntegrationScenario.EnvironmentMtaRejected => "environment-mta-rejected",
        WinUIIntegrationScenario.EnvironmentWrongThreadRejected => "environment-wrong-thread-rejected",
        WinUIIntegrationScenario.EnvironmentSecondThreadRejected => "environment-second-thread-rejected",
        WinUIIntegrationScenario.EnvironmentFinalRelease => "environment-final-release",
        WinUIIntegrationScenario.HostBasic => "host-basic",
        WinUIIntegrationScenario.HostColorPicker => "host-color-picker",
        WinUIIntegrationScenario.HostTextEditors => "host-text-editors",
        WinUIIntegrationScenario.HostStress => "host-stress",
        WinUIIntegrationScenario.HostMultiple => "host-multiple",
        WinUIIntegrationScenario.HostLayout => "host-layout",
        WinUIIntegrationScenario.HostAirspace => "host-airspace",
        WinUIIntegrationScenario.HostScrolling => "host-scrolling",
        WinUIIntegrationScenario.HostAccessibility => "host-accessibility",
        WinUIIntegrationScenario.HostReparent => "host-reparent",
        WinUIIntegrationScenario.HostReplacement => "host-replacement",
        WinUIIntegrationScenario.HostPopupClose => "host-popup-close",
        WinUIIntegrationScenario.HostShutdownCleanup => "host-shutdown-cleanup",
        WinUIIntegrationScenario.FocusTraversal => "focus-traversal",
        WinUIIntegrationScenario.InputSemantics => "input-semantics",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario))
    };

    private static string FindScenarioExecutable(WinUIIntegrationScenario scenario)
        => scenario <= WinUIIntegrationScenario.RawScrolling
            ? FindExecutable("ControlHost", "ControlHost.exe")
            : FindExecutable("IntegrationHost", "IntegrationHost.exe");

    private static string FindExecutable(string projectName, string executableName)
    {
        DirectoryInfo artifacts = GetArtifactsDirectory();
        string outputDirectory = Path.Combine(
            artifacts.FullName,
            "x64",
            GetConfigurationName(),
            projectName);
        if (!Directory.Exists(outputDirectory))
        {
            throw new DirectoryNotFoundException($"Scenario output directory '{outputDirectory}' does not exist.");
        }

        string[] candidates = Directory.GetFiles(
            outputDirectory,
            executableName,
            SearchOption.AllDirectories);
        return candidates.Length switch
        {
            1 => candidates[0],
            0 => throw new FileNotFoundException($"{executableName} was not found under '{outputDirectory}'."),
            _ => throw new InvalidOperationException($"Multiple {executableName} files were found under '{outputDirectory}'.")
        };
    }

    private static DirectoryInfo GetArtifactsDirectory()
        => FindArtifactsDirectory(new(AppContext.BaseDirectory))
            ?? FindArtifactsDirectory(new(Environment.CurrentDirectory))
            ?? throw new DirectoryNotFoundException("Could not locate the repository artifacts directory.");

    private static string GetConfigurationName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    private static DirectoryInfo? FindArtifactsDirectory(DirectoryInfo directory)
    {
        for (DirectoryInfo? current = directory; current is not null; current = current.Parent)
        {
            if (current.Name.Equals("artifacts", StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            string artifactsPath = Path.Combine(current.FullName, "artifacts");
            if (File.Exists(Path.Combine(current.FullName, "thirtytwo.slnx"))
                && Directory.Exists(artifactsPath))
            {
                return new(artifactsPath);
            }
        }

        return null;
    }
}
