// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Layout provider factory.
/// </summary>
public static class Layout
{
    /// <inheritdoc cref="FixedPercentLayout(ILayoutHandler, float, float, VerticalAlignment, HorizontalAlignment)"/>
    public static ILayoutHandler FixedPercent(
        float widthPercent,
        float heightPercent,
        ILayoutHandler handler,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
        => new FixedPercentLayout(handler, heightPercent, widthPercent, verticalAlignment, horizontalAlignment);

    /// <inheritdoc cref="FixedPercentLayout(ILayoutHandler, float, float, VerticalAlignment, HorizontalAlignment)"/>
    /// <param name="percent">The percentage of available width and height to use.</param>
    public static ILayoutHandler FixedPercent(
        float percent,
        ILayoutHandler handler,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
        => new FixedPercentLayout(handler, percent, percent, verticalAlignment, horizontalAlignment);

    /// <inheritdoc cref="FixedSizeLayout(ILayoutHandler, Size, VerticalAlignment, HorizontalAlignment)"/>
    public static ILayoutHandler FixedSize(
        Size size,
        ILayoutHandler handler,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center)
        => new FixedSizeLayout(handler, size, verticalAlignment, horizontalAlignment);

    /// <inheritdoc cref="RowsLayout"/>
    /// <inheritdoc cref="RowsLayout(ValueTuple{float, ILayoutHandler}[])"/>
    public static ILayoutHandler Rows(
        params (float Percent, ILayoutHandler Handler)[] handlers)
        => new RowsLayout(handlers);

    /// <inheritdoc cref="ColumnsLayout"/>
    /// <inheritdoc cref="ColumnsLayout(ValueTuple{float, ILayoutHandler}[])"/>
    public static ILayoutHandler Columns(
        params (float Percent, ILayoutHandler Handler)[] handlers)
        => new ColumnsLayout(handlers);

    /// <inheritdoc cref="PaddedLayout(Padding, ILayoutHandler)"/>
    public static ILayoutHandler Margin(
        Padding margin,
        ILayoutHandler handler)
        => new PaddedLayout(margin, handler);

    /// <inheritdoc cref="FillLayout(ILayoutHandler)"/>
    public static ILayoutHandler Fill(ILayoutHandler handler) => new FillLayout(handler);

    /// <inheritdoc cref="EmptyLayout"/>
    public static ILayoutHandler Empty => EmptyLayout.Instance;
}