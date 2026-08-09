// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostAirspaceIntegrationTests
{
    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_RawAirspace_CapturesZOrderAndClippingEvidence()
        => RunAndAssertAirspace(
            WinUIIntegrationScenario.RawAirspace,
            DrawingColor.White);

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostAirspace_CapturesZOrderAndClippingEvidence()
        => RunAndAssertAirspace(
            WinUIIntegrationScenario.HostAirspace,
            Windows.Application.CurrentColorState.Palette.WindowBackground);

    private static void RunAndAssertAirspace(
        WinUIIntegrationScenario scenario,
        DrawingColor clippedHiddenColor)
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(scenario, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        result.DiagnosticMessage.Should().BeNull();
        result.ExitCode.Should().Be(0);
        result.Screenshot.Should().NotBeNull();
        result.Screenshot!.SampledColorCount.Should().BeGreaterThan(8);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "airspace-zorder-verified",
            "capture-ready",
            "airspace-disposed",
            "scenario-completed");

        WinUIIntegrationEvent captureReady = result.Events.Single(entry => entry.Event == "capture-ready");
        Dictionary<string, AirspaceCaptureSample>? samples = JsonSerializer.Deserialize<Dictionary<string, AirspaceCaptureSample>>(
            captureReady.Message ?? throw new InvalidOperationException("The capture-ready event did not include sample points."));
        samples.Should().NotBeNull();
        samples!.Keys.Should().BeEquivalentTo(
            "nativeAbove",
            "xamlAbove",
            "nativeUnderExposed",
            "xamlUnderExposed",
            "clippedVisible",
            "clippedHidden");

        Dictionary<string, DrawingColor> expectedColors = new()
        {
            ["nativeAbove"] = DrawingColor.Magenta,
            ["xamlAbove"] = DrawingColor.FromArgb(255, 255, 140, 0),
            ["nativeUnderExposed"] = DrawingColor.SeaGreen,
            ["xamlUnderExposed"] = DrawingColor.RoyalBlue,
            ["clippedVisible"] = DrawingColor.Purple,
            ["clippedHidden"] = clippedHiddenColor
        };

        using DrawingBitmap screenshot = new(result.Screenshot.Path);
        foreach ((string name, AirspaceCaptureSample sample) in samples)
        {
            sample.X.Should().BeInRange(0, screenshot.Width - 1, $"sample '{name}' should be inside the screenshot");
            sample.Y.Should().BeInRange(0, screenshot.Height - 1, $"sample '{name}' should be inside the screenshot");
            DrawingColor actual = screenshot.GetPixel(sample.X, sample.Y);
            AssertColorNear(actual, expectedColors[name], name);
        }
    }

    private static void AssertColorNear(DrawingColor actual, DrawingColor expected, string sampleName)
    {
        const int ChannelTolerance = 12;
        Math.Abs(actual.R - expected.R).Should().BeLessThanOrEqualTo(
            ChannelTolerance,
            $"sample '{sampleName}' red channel should match {expected}");
        Math.Abs(actual.G - expected.G).Should().BeLessThanOrEqualTo(
            ChannelTolerance,
            $"sample '{sampleName}' green channel should match {expected}");
        Math.Abs(actual.B - expected.B).Should().BeLessThanOrEqualTo(
            ChannelTolerance,
            $"sample '{sampleName}' blue channel should match {expected}");
    }
}
