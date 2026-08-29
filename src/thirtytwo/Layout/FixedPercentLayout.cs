// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses fixed width and height percentages of the available bounds, then aligns the result.
/// </summary>
/// <remarks>
///  <para>
///   The computed size is <c>(int)(bounds.Width * widthPercent)</c> by
///   <c>(int)(bounds.Height * heightPercent)</c>. No clamping or validation is performed; values outside the
///   <c>0.0</c> through <c>1.0</c> range can produce rectangles larger than, or offset outside, the input bounds.
///  </para>
/// </remarks>
/// <param name="handler">The child handler that receives the computed and aligned rectangle.</param>
/// <param name="heightPercent">The fraction of the available height to use when computing the child height.</param>
/// <param name="widthPercent">The fraction of the available width to use when computing the child width.</param>
/// <param name="verticalAlignment">
///  The vertical placement of the computed rectangle inside the available bounds.
/// </param>
/// <param name="horizontalAlignment">
///  The horizontal placement of the computed rectangle inside the available bounds.
/// </param>
public class FixedPercentLayout(
    ILayoutHandler handler,
    float heightPercent,
    float widthPercent,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    /// <inheritdoc/>
    public void Layout(Rectangle bounds, float scale)
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

        handler.Layout(new Rectangle(new Point(x, y), size), scale);
    }
}