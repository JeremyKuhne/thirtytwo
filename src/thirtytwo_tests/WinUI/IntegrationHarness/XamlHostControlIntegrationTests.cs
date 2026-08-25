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
    [Timeout(30_000)]
    public void RunAsync_HostDropTarget_ConfiguresRoutedDropAndDisposesWithParent()
    {
        WinUIIntegrationResult result = AssertScenario(WinUIIntegrationScenario.HostDropTarget);

        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "winui-empty-text-drop-caret-bounded",
            "winui-drop-routing-configured",
            "winui-drop-caret-canceled-for-reparent",
            "winui-drop-routing-retained-after-reparent",
            "drop-target-host-destroyed",
            "environment-stopped",
            "scenario-completed");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostNativeTextDrag_MovesSelectionIntoBothWinUIEditors()
    {
        WinUIIntegrationResult result = AssertScenario(WinUIIntegrationScenario.HostNativeTextDrag);

        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "native-text-drag-controls-created",
            "native-text-drag-targets-loaded",
            "native-text-drag-textbox-source-ready",
            "native-text-drag-textbox-source-positioned",
            "native-text-drag-textbox-source-left-down",
            "native-text-drag-textbox-source-drag-move",
            "native-text-drag-textbox-target-drag-enter",
            "native-text-drag-textbox-mouse-left-up-injecting",
            "native-text-drag-textbox-mouse-left-up-injected",
            "native-text-drag-textbox-target-drop",
            "native-text-drag-textbox-target-committed",
            "native-text-drag-textbox-source-deleted",
            "native-text-drag-textbox-move-verified",
            "native-text-drag-rich-edit-box-source-ready",
            "native-text-drag-rich-edit-box-source-positioned",
            "native-text-drag-rich-edit-box-source-left-down",
            "native-text-drag-rich-edit-box-source-drag-move",
            "native-text-drag-rich-edit-box-target-drag-enter",
            "native-text-drag-rich-edit-box-mouse-left-up-injecting",
            "native-text-drag-rich-edit-box-mouse-left-up-injected",
            "native-text-drag-rich-edit-box-target-drop",
            "native-text-drag-rich-edit-box-target-committed",
            "native-text-drag-rich-edit-box-source-deleted",
            "native-text-drag-rich-edit-box-move-verified",
            "native-text-drag-completed",
            "environment-stopped",
            "scenario-completed");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostTextEditors_ProjectAndDisposeWithParent()
    {
        WinUIIntegrationResult result = AssertScenario(WinUIIntegrationScenario.HostTextEditors);

        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "text-editors-projected",
            "ready",
            "text-editor-events-projected",
            "text-editor-hosts-destroyed",
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
