// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostFocusIntegrationTests
{
    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_FocusTraversal_VisitsNativeAndXamlStopsInBothDirections()
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(WinUIIntegrationScenario.FocusTraversal, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.TimedOut.Should().BeFalse();
        result.ExitCode.Should().Be(0);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "focus-native-before",
            "focus-xaml-first",
            "focus-xaml-second",
            "focus-native-after",
            "focus-forward-wrapped",
            "focus-backward-wrapped",
            "focus-backward-xaml-second",
            "focus-backward-xaml-first",
            "focus-hidden-host-skipped",
            "focus-disabled-host-skipped",
            "focus-reactivation-stable",
            "focus-traversal-completed",
            "environment-stopped",
            "scenario-completed");
        result.StandardError.Should().BeEmpty();
        AssertProcessExited(result.ProcessId);
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_InputSemantics_HandlesKeysOnceAndRetainsXamlFocus()
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(WinUIIntegrationScenario.InputSemantics, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.TimedOut.Should().BeFalse();
        result.ExitCode.Should().Be(0);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "input-space-single-activation",
            "input-enter-single-delivery",
            "input-accelerator-single-invocation",
            "input-arrow-remained-in-xaml",
            "input-popup-closed-focus-retained",
            "input-text-page-ready",
            "environment-stopped",
            "scenario-completed");
        result.StandardError.Should().BeEmpty();
        AssertProcessExited(result.ProcessId);
    }

    private static void AssertProcessExited(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            process.HasExited.Should().BeTrue($"IntegrationHost process {processId} should have exited");
        }
        catch (ArgumentException)
        {
        }
    }
}