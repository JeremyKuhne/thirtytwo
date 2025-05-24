// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public class LayoutTests
{
    [Fact]
    public void FillLayout_ForwardsBounds()
    {
        LastLayoutHandler handler = new();
        FillLayout layout = new(handler);
        Rectangle bounds = new(1, 2, 3, 4);
        layout.Layout(bounds, 1.0f);
        handler.LastBounds.Should().Be(bounds);
    }

    [Fact]
    public void FixedPercentLayout_PositionsAndSizesCorrectly()
    {
        LastLayoutHandler handler = new();
        FixedPercentLayout layout = new(
            handler,
            heightPercent: 0.5f,
            widthPercent: 0.3f,
            verticalAlignment: VerticalAlignment.Bottom,
            horizontalAlignment: HorizontalAlignment.Left);
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds, 1.0f);
        handler.LastBounds.Should().Be(new Rectangle(0, 100, 30, 100));
    }

    [Fact]
    public void FixedSizeLayout_PositionsCorrectly()
    {
        LastLayoutHandler handler = new();
        FixedSizeLayout layout = new(
            handler,
            new Size(50, 30),
            VerticalAlignment.Bottom,
            HorizontalAlignment.Right);
        Rectangle bounds = new(0, 0, 200, 100);
        layout.Layout(bounds, 1.0f);
        handler.LastBounds.Should().Be(new Rectangle(150, 70, 50, 30));
    }

    [Fact]
    public void PaddedLayout_AppliesPadding()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 20, 30, 40), handler);
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds, 1.0f);
        handler.LastBounds.Should().Be(new Rectangle(10, 20, 60, 140));
    }

    [Fact]
    public void HorizontalLayout_DistributesHeight()
    {
        LastLayoutHandler handler1 = new();
        LastLayoutHandler handler2 = new();
        HorizontalLayout layout = new((0.3f, handler1), (0.7f, handler2));
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds, 1.0f);
        handler1.LastBounds.Should().Be(new Rectangle(0, 0, 100, 60));
        handler2.LastBounds.Should().Be(new Rectangle(0, 60, 100, 140));
    }

    [Fact]
    public void VerticalLayout_DistributesWidth()
    {
        LastLayoutHandler handler1 = new();
        LastLayoutHandler handler2 = new();
        VerticalLayout layout = new((0.4f, handler1), (0.6f, handler2));
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds, 1.0f);
        handler1.LastBounds.Should().Be(new Rectangle(0, 0, 40, 200));
        handler2.LastBounds.Should().Be(new Rectangle(40, 0, 60, 200));
    }

    [Fact]
    public void HorizontalLayout_InvalidPercentages_Throws()
    {
        LastLayoutHandler handler = new();
        FluentActions.Invoking(() => new HorizontalLayout((0.2f, handler), (0.2f, handler)))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void VerticalLayout_InvalidPercentages_Throws()
    {
        LastLayoutHandler handler = new();
        FluentActions.Invoking(() => new VerticalLayout((0.5f, handler), (0.6f, handler)))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReplaceableLayout_UpdatesNewHandler()
    {
        LastLayoutHandler handler1 = new();
        LastLayoutHandler handler2 = new();
        ReplaceableLayout layout = new(handler1);
        Rectangle bounds = new(0, 0, 50, 50);
        layout.Layout(bounds, 1.0f);
        handler1.LastBounds.Should().Be(bounds);
        layout.Handler = handler2;
        handler2.LastBounds.Should().Be(bounds);
    }

    [Fact]
    public void ReplaceableLayout_SetNullHandler_Throws()
    {
        LastLayoutHandler handler = new();
        ReplaceableLayout layout = new(handler);
        FluentActions.Invoking(() => layout.Handler = null!)
            .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void PaddedLayout_ScalesCorrectly_Scale125()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);
        Rectangle bounds = new(0, 0, 100, 100);
        layout.Layout(bounds, 1.25f);

        // With scale 1.25, all paddings should be 12.5 rounded to 12
        handler.LastBounds.Should().Be(new Rectangle(12, 12, 76, 76));
        handler.LastScale.Should().Be(1.25f);
    }

    [Fact]
    public void PaddedLayout_ScalesCorrectly_Scale150()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);
        Rectangle bounds = new(0, 0, 100, 100);
        layout.Layout(bounds, 1.5f);

        // With scale 1.5, all paddings should be 15
        handler.LastBounds.Should().Be(new Rectangle(15, 15, 70, 70));
        handler.LastScale.Should().Be(1.5f);
    }

    [Fact]
    public void PaddedLayout_ScalesCorrectly_Scale175()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);
        Rectangle bounds = new(0, 0, 100, 100);
        layout.Layout(bounds, 1.75f);

        // With scale 1.75, all paddings should be 17.5 rounded to 18
        handler.LastBounds.Should().Be(new Rectangle(18, 18, 64, 64));
        handler.LastScale.Should().Be(1.75f);
    }

    [Fact]
    public void PaddedLayout_ScalesCorrectly_Scale200()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);
        Rectangle bounds = new(0, 0, 100, 100);
        layout.Layout(bounds, 2.0f);

        // With scale 2.0, all paddings should be 20
        handler.LastBounds.Should().Be(new Rectangle(20, 20, 60, 60));
        handler.LastScale.Should().Be(2.0f);
    }

    [Fact]
    public void PaddedLayout_InsufficientWidth_ScalesDown()
    {
        LastLayoutHandler handler = new();

        // Large horizontal margins that would exceed available width
        PaddedLayout layout = new((30, 10, 30, 10), handler);
        Rectangle bounds = new(0, 0, 50, 100);
        layout.Layout(bounds, 1.0f);

        // Should scale down to half scale (30 * 0.5 = 15 on each side)
        handler.LastBounds.Should().Be(new Rectangle(15, 10, 20, 80));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_InsufficientHeight_ScalesDown()
    {
        LastLayoutHandler handler = new();

        // Large vertical margins that would exceed available height
        PaddedLayout layout = new((10, 30, 10, 30), handler);
        Rectangle bounds = new(0, 0, 100, 50);
        layout.Layout(bounds, 1.0f);

        // Should scale down to half scale (30 * 0.5 = 15 on each side)
        handler.LastBounds.Should().Be(new Rectangle(10, 15, 80, 20));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_ExtremelySmallBounds_WidthProvidesSomePadding()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);

        // Width is only 5 pixels
        Rectangle bounds = new(0, 0, 5, 100);
        layout.Layout(bounds, 1.0f);

        // Should eat 2 pixels from each side
        handler.LastBounds.Should().Be(new Rectangle(2, 10, 1, 80));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_ExtremelySmallBounds_HeightProvidesSomePadding()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);

        // Height is only 5 pixels
        Rectangle bounds = new(0, 0, 100, 5);
        layout.Layout(bounds, 1.0f);

        // Should grab 2 pixels from top and bottom
        handler.LastBounds.Should().Be(new Rectangle(10, 2, 80, 1));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_VeryHighScale_HandlesTightBounds()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((5, 5, 5, 5), handler);
        Rectangle bounds = new(0, 0, 60, 60);
        layout.Layout(bounds, 5.0f);

        // With scale 5.0, each padding would be 25, which should fit.
        handler.LastBounds.Should().Be(new Rectangle(25, 25, 10, 10));
        handler.LastScale.Should().Be(5.0f);
    }

    [Fact]
    public void PaddedLayout_ZeroWidthBounds_PreservesPosition()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);

        // Width is 0 pixels
        Rectangle bounds = new(5, 5, 0, 50);
        layout.Layout(bounds, 1.0f);

        // Should maintain X position but have 0 width
        handler.LastBounds.Should().Be(new Rectangle(5, 15, 0, 30));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_ZeroHeightBounds_PreservesPosition()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 10, 10, 10), handler);

        // Height is 0 pixels
        Rectangle bounds = new(5, 5, 50, 0);
        layout.Layout(bounds, 1.0f);

        // Should maintain Y position but have 0 height
        handler.LastBounds.Should().Be(new Rectangle(15, 5, 30, 0));
        handler.LastScale.Should().Be(1.0f);
    }

    [Fact]
    public void PaddedLayout_DeepRecursion()
    {
        LastLayoutHandler handler = new();

        // Create extremely large padding values
        PaddedLayout layout = new((int.MaxValue / 4, 0, int.MaxValue / 4, 0), handler);

        // Tiny bounds that can't possibly accommodate the padding
        Rectangle bounds = new(0, 0, 10, 100);

        // This will cause infinite recursion in ApplyLeftAndRightPadding
        // because even at half scale repeatedly, the padding will still be too large
        layout.Layout(bounds, 1.0f);
    }


    public class LastLayoutHandler : ILayoutHandler
    {
        public Rectangle LastBounds { get; private set; }
        public float LastScale { get; private set; }

        public void Layout(Rectangle bounds, float scale)
        {
            LastBounds = bounds;
            LastScale = scale;
        }
    }
}
