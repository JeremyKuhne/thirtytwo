// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostEnvironmentIntegrationTests
{
    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentOwned_OwnsAndStopsQueue()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentOwned,
            "queue-owned",
            "application-owned",
            "environment-acquired",
            "environment-stopped",
            "application-retained",
            "owned-queue-stopped");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentBorrowed_RetainsExternalQueue()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentBorrowed,
            "external-queue-created",
            "queue-borrowed",
            "application-owned",
            "borrowed-queue-retained");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentComposition_ComposesProvidersResourcesAndThemes()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentComposition,
            "metadata-composed",
            "metadata-collision-reported",
            "resource-registration-rolled-back",
            "resources-composed",
            "resource-collision-reported",
            "theme-dictionaries-preserved");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentMultipleLeases_ReferenceCountsEnvironment()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentMultipleLeases,
            "lease-count-two",
            "lease-count-one",
            "environment-stopped");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentCompatibleApplication_BorrowsApplication()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentCompatibleApplication,
            "external-application-created",
            "application-borrowed",
            "borrowed-queue-retained");

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentIncompatibleApplication_RejectsApplication()
    {
        WinUIIntegrationResult result = AssertScenario(
            WinUIIntegrationScenario.EnvironmentIncompatibleApplication,
            "external-application-created",
            "incompatible-application-rejected",
            "borrowed-queue-retained");

        FindEvent(result, "incompatible-application-rejected").Message.Should().Contain("does not implement");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentMtaRejected_ReportsThreadFailure()
    {
        WinUIIntegrationResult result = AssertScenario(
            WinUIIntegrationScenario.EnvironmentMtaRejected,
            "mta-rejected");

        FindEvent(result, "mta-rejected").Message.Should().Contain("requires an STA thread");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentWrongThreadRejected_RejectsDisposal()
    {
        WinUIIntegrationResult result = AssertScenario(
            WinUIIntegrationScenario.EnvironmentWrongThreadRejected,
            "wrong-thread-rejected");

        FindEvent(result, "wrong-thread-rejected").Message.Should().Contain("Expected managed thread");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentSecondThreadRejected_RejectsSecondXamlThread()
    {
        WinUIIntegrationResult result = AssertScenario(
            WinUIIntegrationScenario.EnvironmentSecondThreadRejected,
            "second-thread-rejected");

        FindEvent(result, "second-thread-rejected").Message.Should().Contain("designated to managed thread");
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_EnvironmentFinalRelease_ReleasesEnvironmentAndRetainsApplication()
        => AssertScenario(
            WinUIIntegrationScenario.EnvironmentFinalRelease,
            "double-dispose-idempotent",
            "final-lease-released",
            "environment-reacquired",
            "application-retained",
            "owned-queue-stopped");

    private static WinUIIntegrationResult AssertScenario(
        WinUIIntegrationScenario scenario,
        params string[] expectedEvents)
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(scenario, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.TimedOut.Should().BeFalse();
        result.ExitCode.Should().Be(0);
        result.ProcessId.Should().BePositive();
        result.MainThreadId.Should().BePositive();
        result.WindowHandle.Should().NotBe(0);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(expectedEvents);
        result.Events.Select(entry => entry.Event).Should().Contain("product-diagnostics-observed");
        result.Events.Select(entry => entry.Event).Should().EndWith("scenario-completed");
        result.StandardError.Should().BeEmpty();
        File.Exists(result.ResultPath).Should().BeTrue();
        AssertProcessExited(result.ProcessId);
        return result;
    }

    private static WinUIIntegrationEvent FindEvent(WinUIIntegrationResult result, string eventName)
        => result.Events.Should().ContainSingle(entry => entry.Event == eventName).Subject;

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