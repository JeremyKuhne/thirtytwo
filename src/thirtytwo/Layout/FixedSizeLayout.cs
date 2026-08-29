// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses a fixed logical size, scales it by the current layout scale, and aligns it in the available bounds.
/// </summary>
/// <remarks>
///  <para>
///   Width and height are scaled independently with <see cref="MathF.Round(float)"/> and converted to integers
///   before alignment. The aligned rectangle is then forwarded to the child handler.
///  </para>
/// </remarks>
/// <param name="handler">The child handler that receives the computed and aligned rectangle.</param>
/// <param name="size">The unscaled logical size to apply before alignment.</param>
/// <param name="verticalAlignment">The vertical placement of the scaled rectangle inside the available bounds.</param>
/// <param name="horizontalAlignment">
///  The horizontal placement of the scaled rectangle inside the available bounds.
/// </param>
public class FixedSizeLayout(
    ILayoutHandler handler,
    Size size,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    /// <inheritdoc/>
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