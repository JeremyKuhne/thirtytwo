// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostControlIntegrationTests
{
    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostBasic_AttachesContentAndDisposesWithParent()
    {
        WinUIIntegrationResult result = AssertScenario(WinUIIntegrationScenario.HostBasic);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "host-attached",
            "site-bridge-owned-by-winui",
            "host-content-created",
            "host-wrong-thread-rejected",
            "ready",
            "host-parent-destroyed",
            "environment-stopped",
            "scenario-completed");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostColorPicker_ProjectsColorAndEvent()
    {
        WinUIIntegrationResult result = AssertScenario(WinUIIntegrationScenario.HostColorPicker);

        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "color-picker-projected",
            "ready",
            "environment-stopped",
            "scenario-completed");
    }

    [TestMethod]
    [Timeout(180_000)]
    public void RunAsync_HostStress_CleansUpOneThousandHostsAndConstructorFailures()
    {
        WinUIIntegrationResult result = AssertScenario(
            WinUIIntegrationScenario.HostStress,
            TimeSpan.FromSeconds(150));

        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "host-constructor-failure-cleaned",
            "host-stress-completed",
            "ready",
            "environment-stopped",
            "scenario-completed");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostMultiple_DisposesIndependentHostsInDifferentOrders()
        => AssertScenario(WinUIIntegrationScenario.HostMultiple)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "multiple-host-disposal-completed",
                "ready",
                "scenario-completed");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostLayout_TracksSizeVisibilityResizeAndDpi()
        => AssertScenario(WinUIIntegrationScenario.HostLayout)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "ready",
                "host-zero-size",
                "host-visibility-synchronized",
                "host-resize-storm-completed",
                "host-dpi-resynchronized",
                "scenario-completed");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostReparent_ReattachesSourceAndPreservesContent()
        => AssertScenario(WinUIIntegrationScenario.HostReparent)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "destroyed-reparent-target-rejected",
                "host-reparented",
                "ready",
                "scenario-completed");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostReplacement_CreatesFreshSourceAndContent()
        => AssertScenario(WinUIIntegrationScenario.HostReplacement)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "host-replacement-created",
                "ready",
                "scenario-completed");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostPopupClose_DisposesWhilePopupIsOpen()
        => AssertScenario(WinUIIntegrationScenario.HostPopupClose)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "ready",
                "host-popup-open",
                "popup-parent-destroyed",
                "environment-stopped",
                "scenario-completed");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostShutdownCleanup_ReleasesLeakedHostOnOwnerThread()
        => AssertScenario(WinUIIntegrationScenario.HostShutdownCleanup)
            .Events.Select(entry => entry.Event).Should().ContainInOrder(
                "ready",
                "host-left-for-shutdown",
                "host-shutdown-cleaned",
                "environment-stopped",
                "scenario-completed");

    private static WinUIIntegrationResult AssertScenario(
        WinUIIntegrationScenario scenario,
        TimeSpan? timeout = null)
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(scenario, timeout ?? TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.TimedOut.Should().BeFalse();
        result.ExitCode.Should().Be(0);
        result.StandardError.Should().BeEmpty();
        AssertProcessExited(result.ProcessId);
        return result;
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