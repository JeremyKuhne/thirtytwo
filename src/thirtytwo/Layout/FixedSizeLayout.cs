// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses a fixed size for the layout, aligning within as specified.
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
        int x = horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => bounds.Right - size.Width,
            HorizontalAlignment.Center => (bounds.Width - size.Width) / 2,
            _ => bounds.Left,
        };

        int y = verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => bounds.Bottom - size.Height,
            VerticalAlignment.Center => (bounds.Height - size.Height) / 2,
            _ => bounds.Top,
        };

        handler.Layout(new Rectangle(new Point(x, y), size), scale);
    }
}