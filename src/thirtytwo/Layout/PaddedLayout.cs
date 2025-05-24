// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Applies padding (margins) to the layout area before delegating layout to the specified handler.
/// </summary>
/// <param name="margin">The padding to apply on each side of the layout bounds.</param>
/// <param name="handler">The layout handler to which the padded bounds are passed.</param>
public class PaddedLayout(
    Padding margin,
    ILayoutHandler handler) : ILayoutHandler
{
    /// <summary>
    ///  Lays out the handler within the specified bounds, applying the configured padding.
    /// </summary>
    /// <param name="bounds">The bounds within which to layout, before padding is applied.</param>
    public void Layout(Rectangle bounds)
    {
        int widthMargin = margin.Left + margin.Right;
        int remainingWidth = bounds.Width - widthMargin;
        if (remainingWidth < 0)
        {
            // Not enough space to grant full margins
            // TODO: Complicated logic here (evenly distribute what we have available in pixels?)
        }
        else
        {
            bounds.X += margin.Left;
            bounds.Width -= margin.Left + margin.Right;
        }

        int heightMargin = margin.Top + margin.Bottom;
        int remainingHeight = bounds.Height - heightMargin;
        if (remainingHeight < 0)
        {
            // Not enough space to grant full margins
            // TODO: Complicated logic here (evenly distribute what we have available in pixels?)
        }
        else
        {
            bounds.Y += margin.Top;
            bounds.Height -= margin.Top + margin.Bottom;
        }

        handler.Layout(bounds);
    }
}