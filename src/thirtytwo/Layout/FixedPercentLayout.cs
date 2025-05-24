// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses a fixed percent of the available space, aligning within as specified.
/// </summary>
/// <param name="handler">The handler to layout within the specified space.</param>
/// <param name="heightPercent">The percentage of available height to use.</param>
/// <param name="widthPercent">The percentage of available width to use.</param>
/// <param name="verticalAlignment">The vertical alignment within the bounds.</param>
/// <param name="horizontalAlignment">The horizontal alignment within the bounds.</param>
public class FixedPercentLayout(
    ILayoutHandler handler,
    float heightPercent,
    float widthPercent,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    public void Layout(Rectangle bounds)
    {
        Size size = new((int)(bounds.Width * widthPercent), (int)(bounds.Height * heightPercent));

        int x = horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => bounds.Right - size.Width,
            HorizontalAlignment.Center => bounds.X + ((bounds.Width - size.Width) / 2),
            _ => bounds.Left,
        };

        int y = verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => bounds.Bottom - size.Height,
            VerticalAlignment.Center => bounds.Y + ((bounds.Height - size.Height) / 2),
            _ => bounds.Top,
        };

        handler.Layout(new Rectangle(new Point(x, y), size));
    }
}