// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostAccessibilityIntegrationTests
{
    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostAccessibility_CapturesHierarchyPatternsFocusAndThemes()
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(WinUIIntegrationScenario.HostAccessibility, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.ExitCode.Should().Be(0);
        result.Uia.Should().NotBeNull();
        UiaAccessibilityAssertions.Assert(result.Uia!, result.WindowHandle);
        result.Screenshot.Should().NotBeNull();
        result.Screenshot!.SampledColorCount.Should().BeGreaterThan(4);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "host-accessibility-created",
            "theme-light-applied",
            "theme-dark-applied",
            "theme-system-applied",
            "accessibility-ready",
            "accessibility-disposed",
            "environment-stopped",
            "scenario-completed");
    }
}