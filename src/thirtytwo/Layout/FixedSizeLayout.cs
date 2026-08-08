// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses a fixed logical size, scaled by the layout scale, and aligns it within the available bounds.
/// </summary>
/// <param name="handler">The handler to layout within the specified space.</param>
/// <param name="size">The fixed size to use.</param>
/// <param name="verticalAlignment">The vertical alignment within the bounds.</param>
/// <param name="horizontalAlignment">The horizontal alignment within the bounds.</param>
public class FixedSizeLayout(
    ILayoutHandler handler,
    Size size,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    public void Layout(Rectangle bounds, float scale)
    {
        Size scaledSize = new(
            (int)MathF.Round(size.Width * scale),
            (int)MathF.Round(size.Height * scale));

        int x = horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => bounds.Right - scaledSize.Width,
            HorizontalAlignment.Center => bounds.X + ((bounds.Width - scaledSize.Width) / 2),
            _ => bounds.Left,
        };

        int y = verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => bounds.Bottom - scaledSize.Height,
            VerticalAlignment.Center => bounds.Y + ((bounds.Height - scaledSize.Height) / 2),
            _ => bounds.Top,
        };

        handler.Layout(new Rectangle(new Point(x, y), scaledSize), scale);
    }
}