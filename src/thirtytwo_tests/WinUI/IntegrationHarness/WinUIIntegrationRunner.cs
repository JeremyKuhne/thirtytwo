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

    private static readonly JsonSerializerOptions s_resultJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _artifactRoot;
    private readonly string _controlHostExecutable;

    internal WinUIIntegrationRunner()
        : this(Path.Combine(
            GetArtifactsDirectory().FullName,
            "test-results",
            "WinUIIntegrationHarness",
            GetConfigurationName()))
    {
    }

    internal WinUIIntegrationRunner(string artifactRoot)
        : this(FindControlHostExecutable(), artifactRoot)
    {
    }

    internal WinUIIntegrationRunner(string controlHostExecutable, string artifactRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(controlHostExecutable);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactRoot);

        _controlHostExecutable = Path.GetFullPath(controlHostExecutable);
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

        if (!File.Exists(_controlHostExecutable))
        {
            throw new FileNotFoundException("The ControlHost scenario executable was not built.", _controlHostExecutable);
        }

        string scenarioName = GetScenarioName(scenario);
        string artifactDirectory = Path.Combine(_artifactRoot, $"{scenarioName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(artifactDirectory);
        string resultPath = Path.Combine(artifactDirectory, "result.json");
        string standardOutputPath = Path.Combine(artifactDirectory, "stdout.log");
        string standardErrorPath = Path.Combine(artifactDirectory, "stderr.log");

        ProcessStartInfo startInfo = new()
        {
            FileName = _controlHostExecutable,
            WorkingDirectory = Path.GetDirectoryName(_controlHostExecutable)!,
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
        ScenarioOutputReader outputReader = new(scenarioName, processId);
        Task outputTask = outputReader.ReadAsync(process.StandardOutput);
        Task<(string Text, bool Truncated)> errorTask =
            BoundedTextReader.ReadAsync(process.StandardError, MaximumStandardErrorLength);
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
                WindowHandleValidation.Validate(ready.WindowHandle, processId, ready.ThreadId);
                windowHandles = CaptureWindowHandles(ready.WindowHandle, processId);
                try
                {
                    switch (scenario)
                    {
                        case WinUIIntegrationScenario.UiaTree:
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
                        case WinUIIntegrationScenario.NormalClose:
                            RequestClose(ready.WindowHandle, processId);
                            break;
                        case WinUIIntegrationScenario.Startup:
                        case WinUIIntegrationScenario.ShutdownTimeout:
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
                await TerminateProcessTreeAsync(process).ConfigureAwait(false);
            }

            await exitTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await outputTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }

        (string standardError, bool standardErrorTruncated) = await errorTask.ConfigureAwait(false);
        string standardOutput = outputReader.StandardOutput;
        WinUIIntegrationEvent[] eventSnapshot = [.. outputReader.Events];
        List<string> protocolErrorSnapshot = [.. outputReader.ProtocolErrors];
        if (standardErrorTruncated)
        {
            protocolErrorSnapshot.Add(
                $"Standard error exceeded {MaximumStandardErrorLength} retained characters.");
        }

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
            process.ExitCode,
            captureFailure,
            protocolErrorSnapshot,
            dumpPath);

        WinUIIntegrationResult result = new(
            scenarioName,
            processId,
            readyEvent?.ThreadId ?? 0,
            readyEvent?.WindowHandle ?? 0,
            windowHandles,
            process.ExitCode,
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

    private static async Task TerminateProcessTreeAsync(Process process)
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

        await process.WaitForExitAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
    }

    private static unsafe void RequestClose(long windowHandle, int expectedProcessId)
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
        int exitCode,
        Exception? captureFailure,
        IReadOnlyList<string> protocolErrors,
        string? dumpPath)
    {
        string handles = windowHandles.Count == 0
            ? FormatHandle(ready?.WindowHandle ?? 0)
            : string.Join(", ", windowHandles.Select(FormatHandle));
        string identity = $"process {processId}, thread {ready?.ThreadId ?? 0}, HWNDs [{handles}]";
        string lastEvent = last?.Event ?? "<none>";
        string dump = dumpPath ?? "<none>";

        if (timedOut)
        {
            return $"Scenario '{scenario}' ({identity}) timed out after {timeout}. Last event: '{lastEvent}'. Dump: '{dump}'.";
        }

        if (captureFailure is not null)
        {
            return $"Scenario '{scenario}' ({identity}) capture failed after event '{lastEvent}': {captureFailure}";
        }

        if (protocolErrors.Count > 0)
        {
            return $"Scenario '{scenario}' ({identity}) emitted invalid JSON: {string.Join(" | ", protocolErrors)}";
        }

        if (ready is null)
        {
            return $"Scenario '{scenario}' (process {processId}) exited with code {exitCode} before reporting ready. Last event: '{lastEvent}'.";
        }

        if (exitCode != 0)
        {
            return $"Scenario '{scenario}' ({identity}) exited with code {exitCode}. Last event: '{lastEvent}'.";
        }

        return null;
    }

    private static string FormatHandle(long windowHandle)
        => $"0x{windowHandle.ToString("x", CultureInfo.InvariantCulture)}";

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
        _ => throw new ArgumentOutOfRangeException(nameof(scenario))
    };

    private static string FindControlHostExecutable()
    {
        DirectoryInfo artifacts = GetArtifactsDirectory();
        string controlHostDirectory = Path.Combine(
            artifacts.FullName,
            "x64",
            GetConfigurationName(),
            "ControlHost");
        if (!Directory.Exists(controlHostDirectory))
        {
            throw new DirectoryNotFoundException($"ControlHost output directory '{controlHostDirectory}' does not exist.");
        }

        string[] candidates = Directory.GetFiles(
            controlHostDirectory,
            "ControlHost.exe",
            SearchOption.AllDirectories);
        return candidates.Length switch
        {
            1 => candidates[0],
            0 => throw new FileNotFoundException($"ControlHost.exe was not found under '{controlHostDirectory}'."),
            _ => throw new InvalidOperationException($"Multiple ControlHost executables were found under '{controlHostDirectory}'.")
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
