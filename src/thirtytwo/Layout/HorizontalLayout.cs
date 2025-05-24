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
    /// <exception cref="ArgumentOutOfRangeException">
    ///  Thrown when the sum of all percentages does not equal 1.0.
    /// </exception>
    public HorizontalLayout(params (float Percent, ILayoutHandler Handler)[] handlers)
    {
        float totalPercent = 0f;
        foreach (var (percent, handler) in handlers)
        {
            totalPercent += percent;
        }

        if (totalPercent != 1.0f)
            throw new ArgumentOutOfRangeException(nameof(handlers), $"Total percentage must be 1.0f.");

        _handlers = handlers;
    }

    /// <summary>
    ///  Lays out the handlers horizontally within the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to layout within.</param>
    public void Layout(Rectangle bounds)
    {
        int last = _handlers.Length - 1;
        int top = bounds.Top;

        for (int i = 0; i < last; i++)
        {
            int currentHeight = (int)(bounds.Height * _handlers[i].Percent);
            _handlers[i].Handler.Layout(new Rectangle(bounds.X, top, bounds.Width, currentHeight));
            top += currentHeight;
        }

        _handlers[last].Handler.Layout(new Rectangle(bounds.X, top, bounds.Width, bounds.Height - top));
    }
}