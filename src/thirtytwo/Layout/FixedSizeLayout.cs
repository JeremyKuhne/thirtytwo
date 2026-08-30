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
/// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
/// <exception cref="ArgumentOutOfRangeException">
///  A size dimension is negative, or an alignment value is undefined.
/// </exception>
public class FixedSizeLayout(
    ILayoutHandler handler,
    Size size,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    private readonly ILayoutHandler _handler = LayoutValidation.ValidateHandler(handler);
    private readonly Size _size = LayoutValidation.ValidateFixedSize(size);
    private readonly VerticalAlignment _verticalAlignment =
        LayoutValidation.ValidateAlignment(verticalAlignment, nameof(verticalAlignment));
    private readonly HorizontalAlignment _horizontalAlignment =
        LayoutValidation.ValidateAlignment(horizontalAlignment, nameof(horizontalAlignment));

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="scale"/> is not finite or positive.</exception>
    /// <exception cref="OverflowException">The scaled child bounds cannot be represented by integers.</exception>
    public void Layout(Rectangle bounds, float scale)
    {
        LayoutValidation.ValidateScale(scale);
        Size scaledSize = new(
            LayoutValidation.MultiplyAndRound(_size.Width, scale),
            LayoutValidation.MultiplyAndRound(_size.Height, scale));

        int x = _horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => checked(bounds.X + bounds.Width - scaledSize.Width),
            HorizontalAlignment.Center => checked(bounds.X + ((bounds.Width - scaledSize.Width) / 2)),
            _ => throw new InvalidOperationException("The horizontal alignment was not validated."),
        };

        int y = _verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => checked(bounds.Y + bounds.Height - scaledSize.Height),
            VerticalAlignment.Center => checked(bounds.Y + ((bounds.Height - scaledSize.Height) / 2)),
            _ => throw new InvalidOperationException("The vertical alignment was not validated."),
        };

        _handler.Layout(new Rectangle(new Point(x, y), scaledSize), scale);
    }
}