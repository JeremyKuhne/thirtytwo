// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Places child handlers in vertical columns by splitting the available width proportionally.
/// </summary>
/// <remarks>
///  <para>
///   Every child receives the full input height. Each handler except the last receives
///   <c>(int)(bounds.Width * percent)</c> pixels, and the last handler receives the remaining width so total
///   coverage matches the original bounds.
///  </para>
/// </remarks>
public class VerticalLayout : ILayoutHandler
{
    private readonly (float Percent, ILayoutHandler Handler)[] _handlers;

    /// <summary>
    ///  Initializes a new instance of the <see cref="VerticalLayout"/> class.
    /// </summary>
    /// <param name="handlers">
    ///  The proportional child definitions. Each tuple contains a width percentage and the handler for that segment.
    ///  Percentages must be finite, each percentage must be between <c>0.0</c> and <c>1.0</c>, and the total must
    ///  equal <c>1.0</c> within tolerance.
    /// </param>
    /// <exception cref="ArgumentNullException">The handler array or one of its handlers is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  A percentage is nonfinite or outside 0.0 through 1.0, or the sum does not equal 1.0 within float precision.
    /// </exception>
    public VerticalLayout(params (float Percent, ILayoutHandler Handler)[] handlers)
    {
        LayoutValidation.ValidateProportionalHandlers(handlers);
        _handlers = [.. handlers];
    }

    /// <inheritdoc/>
    /// <exception cref="OverflowException">The computed child bounds cannot be represented by integers.</exception>
    public void Layout(Rectangle bounds, float scale)
    {
        int last = _handlers.Length - 1;
        int left = bounds.Left;
        int right = checked(bounds.X + bounds.Width);

        for (int i = 0; i < last; i++)
        {
            int currentWidth = LayoutValidation.MultiplyAndTruncate(bounds.Width, _handlers[i].Percent);
            _handlers[i].Handler.Layout(new Rectangle(left, bounds.Y, currentWidth, bounds.Height), scale);
            left = checked(left + currentWidth);
        }

        _handlers[last].Handler.Layout(new Rectangle(left, bounds.Y, checked(right - left), bounds.Height), scale);
    }
}