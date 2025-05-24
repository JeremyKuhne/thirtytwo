// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public class LastLayoutHandler : ILayoutHandler
{
    public Rectangle LastBounds { get; private set; }
    public void Layout(Rectangle bounds) => LastBounds = bounds;
}

public class LayoutTests
{
    [Fact]
    public void FillLayout_ForwardsBounds()
    {
        LastLayoutHandler handler = new();
        FillLayout layout = new(handler);
        Rectangle bounds = new(1, 2, 3, 4);
        layout.Layout(bounds);
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
        layout.Layout(bounds);
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
        layout.Layout(bounds);
        handler.LastBounds.Should().Be(new Rectangle(150, 70, 50, 30));
    }

    [Fact]
    public void PaddedLayout_AppliesPadding()
    {
        LastLayoutHandler handler = new();
        PaddedLayout layout = new((10, 20, 30, 40), handler);
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds);
        handler.LastBounds.Should().Be(new Rectangle(10, 20, 60, 140));
    }

    [Fact]
    public void HorizontalLayout_DistributesHeight()
    {
        LastLayoutHandler handler1 = new();
        LastLayoutHandler handler2 = new();
        HorizontalLayout layout = new((0.3f, handler1), (0.7f, handler2));
        Rectangle bounds = new(0, 0, 100, 200);
        layout.Layout(bounds);
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
        layout.Layout(bounds);
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
        layout.Layout(bounds);
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
}
