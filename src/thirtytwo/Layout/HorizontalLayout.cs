// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Arranges elements horizontally by allocating a percentage of the available height to each handler.
/// </summary>
public class HorizontalLayout : ILayoutHandler
{
    private readonly (float Percent, ILayoutHandler Handler)[] _handlers;

    /// <summary>
    ///  Initializes a new instance of the <see cref="HorizontalLayout"/> class.
    /// </summary>
    /// <param name="handlers">
    ///  An array of tuples containing the percentage of height to allocate and the handler to layout in that space.
    ///  The sum of all percentages must equal 1.0.
    /// </param>
    /// <exception cref="ArgumentNullException">The handler array or one of its handlers is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  A percentage is nonfinite or outside 0.0 through 1.0, or the sum does not equal 1.0 within float precision.
    /// </exception>
    public HorizontalLayout(params (float Percent, ILayoutHandler Handler)[] handlers)
    {
        LayoutValidation.ValidateProportionalHandlers(handlers);
        _handlers = [.. handlers];
    }

    /// <summary>
    ///  Lays out the handlers horizontally within the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to layout within.</param>
    public void Layout(Rectangle bounds, float scale)
    {
        int last = _handlers.Length - 1;
        int top = bounds.Top;

        for (int i = 0; i < last; i++)
        {
            int currentHeight = (int)(bounds.Height * _handlers[i].Percent);
            _handlers[i].Handler.Layout(new Rectangle(bounds.X, top, bounds.Width, currentHeight), scale);
            top += currentHeight;
        }

        _handlers[last].Handler.Layout(new Rectangle(bounds.X, top, bounds.Width, bounds.Bottom - top), scale);
    }
}