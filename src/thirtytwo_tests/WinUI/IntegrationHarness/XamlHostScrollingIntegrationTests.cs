// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
[DoNotParallelize]
public class XamlHostScrollingIntegrationTests
{
    private static readonly ScrollingObservation[] s_expectedObservations =
    [
        new(80, 70, 0, 0, 180, 120, 320, 200, 0, 0, 320, 200, true, true),
        new(80, 70, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true),
        new(140, 110, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true)
    ];

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_RawScrolling_CapturesTranslatedAncestorEvidence()
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(WinUIIntegrationScenario.RawScrolling, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        AssertScenario(result, DrawingColor.White);
    }

    [TestMethod]
    [Timeout(30_000)]
    public void RunAsync_HostScrolling_CapturesTranslatedAncestorEvidence()
    {
        WinUIIntegrationResult result = new WinUIIntegrationRunner()
            .RunAsync(WinUIIntegrationScenario.HostScrolling, TimeSpan.FromSeconds(20))
            .GetAwaiter()
            .GetResult();

        AssertScenario(result, Windows.Application.CurrentColorState.Palette.WindowBackground);
    }

    private static void AssertScenario(WinUIIntegrationResult result, DrawingColor clippedColor)
    {
        result.DiagnosticMessage.Should().BeNull();
        result.ExitCode.Should().Be(0);
        result.Screenshot.Should().NotBeNull();
        result.Screenshot!.SampledColorCount.Should().BeGreaterThan(5);
        result.Events.Select(entry => entry.Event).Should().ContainInOrder(
            "ready",
            "scroll-initial-verified",
            "scroll-content-translated",
            "scroll-viewport-moved",
            "scroll-bounds-synchronized",
            "capture-ready",
            "scrolling-disposed",
            "scenario-completed");

        string[] observationEvents =
        [
            "scroll-initial-verified",
            "scroll-content-translated",
            "scroll-viewport-moved"
        ];
        ScrollingObservation[] observations = observationEvents
            .Select(eventName => Deserialize<ScrollingObservation>(result, eventName))
            .ToArray();
        observations.Should().Equal(s_expectedObservations);

        Dictionary<string, ScrollingCaptureSample> samples =
            Deserialize<Dictionary<string, ScrollingCaptureSample>>(result, "capture-ready");
        samples.Keys.Should().BeEquivalentTo(
            "hostVisible",
            "hostClippedLeft",
            "hostClippedTop",
            "contentExposed",
            "focusTarget");

        Dictionary<string, DrawingColor> expectedColors = new()
        {
            ["hostVisible"] = DrawingColor.RoyalBlue,
            ["hostClippedLeft"] = clippedColor,
            ["hostClippedTop"] = clippedColor,
            ["contentExposed"] = DrawingColor.FromArgb(255, 24, 24, 24),
            ["focusTarget"] = DrawingColor.SeaGreen
        };

        using DrawingBitmap screenshot = new(result.Screenshot.Path);
        foreach ((string name, ScrollingCaptureSample sample) in samples)
        {
            sample.X.Should().BeInRange(0, screenshot.Width - 1, $"sample '{name}' should be inside the screenshot");
            sample.Y.Should().BeInRange(0, screenshot.Height - 1, $"sample '{name}' should be inside the screenshot");
            AssertColorNear(screenshot.GetPixel(sample.X, sample.Y), expectedColors[name], name);
        }
    }

    private static T Deserialize<T>(WinUIIntegrationResult result, string eventName)
    {
        WinUIIntegrationEvent scenarioEvent = result.Events.Single(entry => entry.Event == eventName);
        return JsonSerializer.Deserialize<T>(
            scenarioEvent.Message ?? throw new InvalidOperationException($"The {eventName} event had no payload."))
            ?? throw new InvalidOperationException($"The {eventName} payload was null.");
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