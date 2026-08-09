// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class WinUIIntegrationHarnessTests
{
    // First Windows App SDK activation can be materially slower on a cold hosted runner than on a warm local machine.
    private static readonly TimeSpan s_startupTimeout = TimeSpan.FromSeconds(30);

    [TestMethod]
    [Timeout(45_000)]
    public void RunAsync_Startup_ReturnsStructuredResultAndExits()
    {
        WinUIIntegrationResult result = CreateRunner()
            .RunAsync(WinUIIntegrationScenario.Startup, s_startupTimeout)
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.TimedOut.Should().BeFalse();
        result.ExitCode.Should().Be(0);
        result.ProcessId.Should().BePositive();
        result.MainThreadId.Should().BePositive();
        result.WindowHandle.Should().NotBe(0);
        result.WindowHandles.Should().Contain(result.WindowHandle);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "process-started",
            "ready",
            "close-received",
            "window-destroyed",
            "dispatcher-queue-shutdown-completed",
            "scenario-completed");
        result.StandardOutput.Should().Contain("\"event\":\"ready\"");
        result.StandardError.Should().BeEmpty();
        File.Exists(result.ResultPath).Should().BeTrue();
        AssertProcessExited(result.ProcessId);
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_UiaTree_CapturesColorPickerAndNonblankScreenshot()
    {
        WinUIIntegrationResult result = CreateRunner()
            .RunAsync(WinUIIntegrationScenario.UiaTree, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.ExitCode.Should().Be(0);
        result.Uia.Should().NotBeNull();
        result.Uia!.RootWindowHandle.Should().Be(result.WindowHandle);
        result.Uia.Elements.Should().HaveCountGreaterThan(10);
        result.Uia.Elements.Select(element => element.ControlType).Should().Contain("ControlType.Slider");
        result.Uia.Elements.Select(element => element.ControlType).Should().Contain("ControlType.ComboBox");
        result.Uia.Elements.Select(element => element.ControlType).Should().Contain("ControlType.Edit");
        result.Screenshot.Should().NotBeNull();
        result.Screenshot!.Width.Should().BePositive();
        result.Screenshot.Height.Should().BePositive();
        result.Screenshot.SampledColorCount.Should().BeGreaterThan(4);
        new FileInfo(result.Screenshot.Path).Length.Should().BeGreaterThan(1_000);
        AssertProcessExited(result.ProcessId);
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_NormalClose_DisposesBeforeDispatcherShutdown()
    {
        WinUIIntegrationResult result = CreateRunner()
            .RunAsync(WinUIIntegrationScenario.NormalClose, TimeSpan.FromSeconds(15))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.ExitCode.Should().Be(0);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "close-received",
            "island-disposed",
            "window-destroyed",
            "dispatcher-queue-shutdown-started",
            "dispatcher-queue-shutdown-completed");
        AssertProcessExited(result.ProcessId);
    }

    [TestMethod]
    [Timeout(20_000)]
    public void RunAsync_ShutdownTimeout_KillsProcessTreeAndReportsDiagnostics()
    {
        WinUIIntegrationResult result = CreateRunner()
            .RunAsync(WinUIIntegrationScenario.ShutdownTimeout, TimeSpan.FromSeconds(5))
            .GetAwaiter()
            .GetResult();

        result.TimedOut.Should().BeTrue();
        result.ProcessId.Should().BePositive();
        result.MainThreadId.Should().BePositive();
        result.WindowHandle.Should().NotBe(0);
        result.LastEvent.Should().Be("ready");
        result.DiagnosticMessage.Should().Contain("shutdown-timeout");
        result.DiagnosticMessage.Should().Contain($"process {result.ProcessId}");
        result.DiagnosticMessage.Should().Contain($"thread {result.MainThreadId}");
        result.DiagnosticMessage.Should().Contain("HWNDs [0x");
        result.DiagnosticMessage.Should().Contain("Last event: 'ready'");
        result.DiagnosticMessage.Should().Contain("Dump: '<none>'");
        AssertProcessExited(result.ProcessId);
    }

    [TestMethod]
    [Timeout(120_000)]
    public void RunAsync_RepeatedStartup_LeavesNoChildProcesses()
    {
        WinUIIntegrationRunner runner = CreateRunner();

        for (int iteration = 0; iteration < 3; iteration++)
        {
            WinUIIntegrationResult result = runner
                .RunAsync(WinUIIntegrationScenario.Startup, s_startupTimeout)
                .GetAwaiter()
                .GetResult();

            result.DiagnosticMessage.Should().BeNull();
            result.ExitCode.Should().Be(0);
            AssertProcessExited(result.ProcessId);
        }
    }

    private WinUIIntegrationRunner CreateRunner()
        => new();

    private static void AssertProcessExited(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            process.HasExited.Should().BeTrue($"ControlHost process {processId} should have exited");
        }
        catch (ArgumentException)
        {
        }
    }
}
