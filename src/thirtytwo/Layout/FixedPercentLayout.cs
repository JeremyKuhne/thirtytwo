// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Uses fixed width and height percentages of the available bounds, then aligns the result.
/// </summary>
/// <remarks>
///  <para>
///   The computed size truncates <c>bounds.Width * widthPercent</c> and
///   <c>bounds.Height * heightPercent</c> toward zero. Percentages are finite and nonnegative but are not clamped to
///   <c>1.0</c>; values above <c>1.0</c> deliberately produce rectangles larger than the input bounds.
///  </para>
/// </remarks>
/// <param name="handler">The child handler that receives the computed and aligned rectangle.</param>
/// <param name="heightPercent">The nonnegative factor applied to the available height.</param>
/// <param name="widthPercent">The nonnegative factor applied to the available width.</param>
/// <param name="verticalAlignment">
///  The vertical placement of the computed rectangle inside the available bounds.
/// </param>
/// <param name="horizontalAlignment">
///  The horizontal placement of the computed rectangle inside the available bounds.
/// </param>
/// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
/// <exception cref="ArgumentOutOfRangeException">
///  A percentage is negative or nonfinite, or an alignment value is undefined.
/// </exception>
public class FixedPercentLayout(
    ILayoutHandler handler,
    float heightPercent,
    float widthPercent,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    private readonly ILayoutHandler _handler = LayoutValidation.ValidateHandler(handler);
    private readonly float _heightPercent =
        LayoutValidation.ValidateFixedPercent(heightPercent, nameof(heightPercent));
    private readonly float _widthPercent =
        LayoutValidation.ValidateFixedPercent(widthPercent, nameof(widthPercent));
    private readonly VerticalAlignment _verticalAlignment =
        LayoutValidation.ValidateAlignment(verticalAlignment, nameof(verticalAlignment));
    private readonly HorizontalAlignment _horizontalAlignment =
        LayoutValidation.ValidateAlignment(horizontalAlignment, nameof(horizontalAlignment));

    /// <inheritdoc/>
    /// <exception cref="OverflowException">The computed child bounds cannot be represented by integers.</exception>
    public void Layout(Rectangle bounds, float scale)
    {
        Size size = new(
            LayoutValidation.MultiplyAndTruncate(bounds.Width, _widthPercent),
            LayoutValidation.MultiplyAndTruncate(bounds.Height, _heightPercent));

        int x = _horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => checked(bounds.X + bounds.Width - size.Width),
            HorizontalAlignment.Center => checked(bounds.X + ((bounds.Width - size.Width) / 2)),
            _ => throw new InvalidOperationException("The horizontal alignment was not validated."),
        };

        int y = _verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => checked(bounds.Y + bounds.Height - size.Height),
            VerticalAlignment.Center => checked(bounds.Y + ((bounds.Height - size.Height) / 2)),
            _ => throw new InvalidOperationException("The vertical alignment was not validated."),
        };

        _handler.Layout(new Rectangle(new Point(x, y), size), scale);
    }
}