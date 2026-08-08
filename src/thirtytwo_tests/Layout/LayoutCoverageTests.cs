// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

[TestClass]
public class LayoutCoverageTests
{
    [TestMethod]
    public void FixedPercentLayout_AllAlignments_UseNonzeroBoundsOrigin()
    {
        Rectangle bounds = new(10, 20, 100, 60);
        (VerticalAlignment Vertical, HorizontalAlignment Horizontal, Point Expected)[] cases =
        [
            (VerticalAlignment.Top, HorizontalAlignment.Left, new(10, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Center, new(40, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Right, new(70, 20)),
            (VerticalAlignment.Center, HorizontalAlignment.Left, new(10, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Center, new(40, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Right, new(70, 35)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Left, new(10, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Center, new(40, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Right, new(70, 50))
        ];

        foreach ((VerticalAlignment vertical, HorizontalAlignment horizontal, Point expected) in cases)
        {
            RecordingLayoutHandler handler = new();
            FixedPercentLayout layout = new(
                handler,
                heightPercent: 0.5f,
                widthPercent: 0.4f,
                vertical,
                horizontal);

            layout.Layout(bounds, 1.25f);

            handler.LastBounds.Should().Be(new Rectangle(expected, new Size(40, 30)));
            handler.LastScale.Should().Be(1.25f);
        }
    }

    [TestMethod]
    public void FixedPercent_UniformPercent_AppliesBothDimensions()
    {
        RecordingLayoutHandler handler = new();
        ILayoutHandler layout = Layout.FixedPercent(0.5f, handler);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(35, 35, 50, 30));
    }

    [TestMethod]
    public void FixedPercentLayout_InvalidAlignments_DefaultToTopLeft()
    {
        RecordingLayoutHandler handler = new();
        FixedPercentLayout layout = new(
            handler,
            heightPercent: 0.5f,
            widthPercent: 0.4f,
            (VerticalAlignment)int.MaxValue,
            (HorizontalAlignment)int.MaxValue);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(10, 20, 40, 30));
    }

    [TestMethod]
    public void FixedSizeLayout_AllAlignments_UseNonzeroBoundsOrigin()
    {
        Rectangle bounds = new(10, 20, 100, 60);
        (VerticalAlignment Vertical, HorizontalAlignment Horizontal, Point Expected)[] cases =
        [
            (VerticalAlignment.Top, HorizontalAlignment.Left, new(10, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Center, new(40, 20)),
            (VerticalAlignment.Top, HorizontalAlignment.Right, new(70, 20)),
            (VerticalAlignment.Center, HorizontalAlignment.Left, new(10, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Center, new(40, 35)),
            (VerticalAlignment.Center, HorizontalAlignment.Right, new(70, 35)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Left, new(10, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Center, new(40, 50)),
            (VerticalAlignment.Bottom, HorizontalAlignment.Right, new(70, 50))
        ];

        foreach ((VerticalAlignment vertical, HorizontalAlignment horizontal, Point expected) in cases)
        {
            RecordingLayoutHandler handler = new();
            FixedSizeLayout layout = new(handler, new Size(40, 30), vertical, horizontal);

            layout.Layout(bounds, 1.0f);

            handler.LastBounds.Should().Be(new Rectangle(expected, new Size(40, 30)));
        }
    }

    [TestMethod]
    public void FixedSizeLayout_FractionalScale_RoundsDimensions()
    {
        RecordingLayoutHandler handler = new();
        FixedSizeLayout layout = new(handler, new Size(3, 5));

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.5f);

        handler.LastBounds.Should().Be(new Rectangle(58, 46, 4, 8));
        handler.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void FixedSize_ForwardsSizeAndAlignments()
    {
        RecordingLayoutHandler handler = new();
        ILayoutHandler layout = Layout.FixedSize(
            new Size(40, 30),
            handler,
            VerticalAlignment.Bottom,
            HorizontalAlignment.Right);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(70, 50, 40, 30));
    }

    [TestMethod]
    public void FixedSizeLayout_InvalidAlignments_DefaultToTopLeft()
    {
        RecordingLayoutHandler handler = new();
        FixedSizeLayout layout = new(
            handler,
            new Size(40, 30),
            (VerticalAlignment)int.MaxValue,
            (HorizontalAlignment)int.MaxValue);

        layout.Layout(new Rectangle(10, 20, 100, 60), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(10, 20, 40, 30));
    }

    [TestMethod]
    public void HorizontalLayout_ThreeChildren_AssignsRoundingRemainderToLast()
    {
        RecordingLayoutHandler first = new();
        RecordingLayoutHandler second = new();
        RecordingLayoutHandler third = new();
        HorizontalLayout layout = new((0.333f, first), (0.333f, second), (0.334f, third));

        layout.Layout(new Rectangle(10, 20, 101, 101), 1.5f);

        first.LastBounds.Should().Be(new Rectangle(10, 20, 101, 33));
        second.LastBounds.Should().Be(new Rectangle(10, 53, 101, 33));
        third.LastBounds.Should().Be(new Rectangle(10, 86, 101, 35));
        first.LastScale.Should().Be(1.5f);
        second.LastScale.Should().Be(1.5f);
        third.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void VerticalLayout_ThreeChildren_AssignsRoundingRemainderToLast()
    {
        RecordingLayoutHandler first = new();
        RecordingLayoutHandler second = new();
        RecordingLayoutHandler third = new();
        VerticalLayout layout = new((0.333f, first), (0.333f, second), (0.334f, third));

        layout.Layout(new Rectangle(10, 20, 101, 101), 1.5f);

        first.LastBounds.Should().Be(new Rectangle(10, 20, 33, 101));
        second.LastBounds.Should().Be(new Rectangle(43, 20, 33, 101));
        third.LastBounds.Should().Be(new Rectangle(76, 20, 35, 101));
        first.LastScale.Should().Be(1.5f);
        second.LastScale.Should().Be(1.5f);
        third.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_SingleChild_ReceivesAllBounds()
    {
        Rectangle bounds = new(10, 20, 101, 61);
        RecordingLayoutHandler horizontalHandler = new();
        RecordingLayoutHandler verticalHandler = new();

        new HorizontalLayout((1.0f, horizontalHandler)).Layout(bounds, 1.25f);
        new VerticalLayout((1.0f, verticalHandler)).Layout(bounds, 1.25f);

        horizontalHandler.LastBounds.Should().Be(bounds);
        verticalHandler.LastBounds.Should().Be(bounds);
        horizontalHandler.LastScale.Should().Be(1.25f);
        verticalHandler.LastScale.Should().Be(1.25f);
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_CommonDecimalPercentages_AreAccepted()
    {
        RecordingLayoutHandler handler = new();
        (float Percent, ILayoutHandler Handler)[] handlers =
        [
            (0.07f, handler),
            (0.42f, handler),
            (0.32f, handler),
            (0.12f, handler),
            (0.07f, handler)
        ];

        Action createHorizontal = () => _ = new HorizontalLayout(handlers);
        Action createVertical = () => _ = new VerticalLayout(handlers);

        createHorizontal.Should().NotThrow();
        createVertical.Should().NotThrow();
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_NegativeOrOversizedIndividualPercentage_Throws()
    {
        RecordingLayoutHandler handler = new();

        Action createHorizontal = () => _ = new HorizontalLayout((-0.5f, handler), (1.5f, handler));
        Action createVertical = () => _ = new VerticalLayout((1.5f, handler), (-0.5f, handler));

        createHorizontal.Should().Throw<ArgumentOutOfRangeException>();
        createVertical.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_NullHandler_Throws()
    {
        Action createHorizontal = () => _ = new HorizontalLayout((1.0f, null!));
        Action createVertical = () => _ = new VerticalLayout((1.0f, null!));

        createHorizontal.Should().Throw<ArgumentNullException>();
        createVertical.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_NonfinitePercentage_Throws()
    {
        RecordingLayoutHandler handler = new();

        Action createHorizontal = () => _ = new HorizontalLayout((float.NaN, handler), (1.0f, handler));
        Action createVertical = () => _ = new VerticalLayout((float.PositiveInfinity, handler), (0.0f, handler));

        createHorizontal.Should().Throw<ArgumentOutOfRangeException>();
        createVertical.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void HorizontalAndVerticalLayout_CopyHandlerDefinitions()
    {
        RecordingLayoutHandler original = new();
        RecordingLayoutHandler replacement = new();
        (float Percent, ILayoutHandler Handler)[] horizontalDefinitions = [(1.0f, original)];
        (float Percent, ILayoutHandler Handler)[] verticalDefinitions = [(1.0f, original)];
        HorizontalLayout horizontal = new(horizontalDefinitions);
        VerticalLayout vertical = new(verticalDefinitions);
        horizontalDefinitions[0] = (0.0f, replacement);
        verticalDefinitions[0] = (0.0f, replacement);
        Rectangle bounds = new(10, 20, 100, 60);

        horizontal.Layout(bounds, 1.0f);
        vertical.Layout(bounds, 1.0f);

        original.CallCount.Should().Be(2);
        replacement.CallCount.Should().Be(0);
    }

    [TestMethod]
    public void HorizontalAndVertical_CreateExpectedLayouts()
    {
        RecordingLayoutHandler handler = new();

        Layout.Horizontal((1.0f, handler)).Should().BeOfType<HorizontalLayout>();
        Layout.Vertical((1.0f, handler)).Should().BeOfType<VerticalLayout>();
    }

    [TestMethod]
    public void PaddedLayout_AsymmetricPadding_UsesNonzeroOriginAndForwardsScale()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((3, 5, 7, 11), handler);

        layout.Layout(new Rectangle(10, 20, 100, 80), 2.0f);

        handler.LastBounds.Should().Be(new Rectangle(16, 30, 80, 48));
        handler.LastScale.Should().Be(2.0f);
    }

    [TestMethod]
    public void PaddedLayout_MaximumPadding_DoesNotOverflow()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((int.MaxValue, 0, int.MaxValue, 0), handler);

        layout.Layout(new Rectangle(10, 20, 10, 100), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(14, 20, 2, 100));
    }

    [TestMethod]
    public void PaddedLayout_TightAsymmetricPadding_ScalesTrailingEdges()
    {
        RecordingLayoutHandler handler = new();
        PaddedLayout layout = new((1, 1, 10, 10), handler);

        layout.Layout(new Rectangle(10, 20, 5, 5), 1.0f);

        handler.LastBounds.Should().Be(new Rectangle(10, 20, 0, 0));
    }

    [TestMethod]
    public void Padding_ImplicitConversions_SetAllFields()
    {
        Padding uniform = 7;
        Padding asymmetric = (1, 2, 3, 4);

        (uniform.Left, uniform.Top, uniform.Right, uniform.Bottom).Should().Be((7, 7, 7, 7));
        (asymmetric.Left, asymmetric.Top, asymmetric.Right, asymmetric.Bottom).Should().Be((1, 2, 3, 4));
    }

    [TestMethod]
    public void PaddingF_ImplicitConversions_SetAllFields()
    {
        PaddingF uniform = 1.5f;
        PaddingF asymmetric = (1.0f, 2.0f, 3.0f, 4.0f);

        (uniform.Left, uniform.Top, uniform.Right, uniform.Bottom).Should().Be((1.5f, 1.5f, 1.5f, 1.5f));
        (asymmetric.Left, asymmetric.Top, asymmetric.Right, asymmetric.Bottom).Should().Be((1.0f, 2.0f, 3.0f, 4.0f));
    }

    [TestMethod]
    public void FillAndMargin_ForwardBoundsAndScale()
    {
        Rectangle bounds = new(10, 20, 100, 80);
        RecordingLayoutHandler fillHandler = new();
        RecordingLayoutHandler marginHandler = new();

        Layout.Fill(fillHandler).Layout(bounds, 1.5f);
        Layout.Margin((1, 2, 3, 4), marginHandler).Layout(bounds, 1.5f);

        fillHandler.LastBounds.Should().Be(bounds);
        fillHandler.LastScale.Should().Be(1.5f);
        marginHandler.LastBounds.Should().Be(new Rectangle(12, 23, 94, 71));
        marginHandler.LastScale.Should().Be(1.5f);
    }

    [TestMethod]
    public void Empty_ReturnsSingletonThatAcceptsLayout()
    {
        Layout.Empty.Should().BeSameAs(EmptyLayout.Instance);

        Action layout = () => Layout.Empty.Layout(new Rectangle(10, 20, 100, 80), 1.5f);

        layout.Should().NotThrow();
    }

    [TestMethod]
    public void ReplaceableLayout_SetBeforeFirstLayout_UsesDefaults()
    {
        RecordingLayoutHandler initial = new();
        RecordingLayoutHandler replacement = new();
        ReplaceableLayout layout = new(initial);

        layout.Handler = replacement;

        replacement.LastBounds.Should().Be(Rectangle.Empty);
        replacement.LastScale.Should().Be(1.0f);
        replacement.CallCount.Should().Be(1);
        initial.CallCount.Should().Be(0);
    }

    [TestMethod]
    public void ReplaceableLayout_ForwardsLatestBoundsAndScaleToReplacement()
    {
        RecordingLayoutHandler initial = new();
        RecordingLayoutHandler replacement = new();
        ReplaceableLayout layout = new(initial);
        Rectangle bounds = new(10, 20, 100, 80);

        layout.Layout(bounds, 1.75f);
        layout.Handler = replacement;

        initial.LastBounds.Should().Be(bounds);
        initial.LastScale.Should().Be(1.75f);
        replacement.LastBounds.Should().Be(bounds);
        replacement.LastScale.Should().Be(1.75f);
    }

    [STATestMethod]
    public void LayoutBinder_WindowPositionChanged_LaysOutClientBounds()
    {
        using Window window = new(new Rectangle(10, 20, 200, 100));
        RecordingLayoutHandler handler = new();
        LayoutBinder binder = new(window, handler);

        window.MoveWindow(new Rectangle(20, 30, 300, 150), repaint: false);

        handler.CallCount.Should().BeGreaterThan(0);
        handler.LastBounds.Should().Be(window.GetClientRectangle());
        handler.LastScale.Should().Be(window.GetScale());
        GC.KeepAlive(binder);
    }

    private sealed class RecordingLayoutHandler : ILayoutHandler
    {
        public int CallCount { get; private set; }
        public Rectangle LastBounds { get; private set; }
        public float LastScale { get; private set; }

        public void Layout(Rectangle bounds, float scale)
        {
            CallCount++;
            LastBounds = bounds;
            LastScale = scale;
        }
    }
}
