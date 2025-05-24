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
    public void Layout(Rectangle bounds, float scale)
    {
        ApplyLeftAndRightPadding(ref bounds, margin.Left, margin.Right, scale);
        ApplyTopAndBottomPadding(ref bounds, margin.Top, margin.Bottom, scale);

        handler.Layout(bounds, scale);

        static void ApplyLeftAndRightPadding(ref Rectangle bounds, int leftPadding, int rightPadding, float scale)
        {
            if (bounds.Width <= 0)
            {
                // No width to work with, nothing to do.
                return;
            }

            int left = (int)MathF.Round(leftPadding * scale);
            int right = (int)MathF.Round(rightPadding * scale);

            int marginWidth = left + right;
            int remainingWidth = bounds.Width - marginWidth;
            if (remainingWidth < 0)
            {
                if (left > 1 || right > 1)
                {
                    // Not enough space to grant full margins, try again at half scale.
                    ApplyLeftAndRightPadding(ref bounds, left, right, .5f);
                }
            }
            else
            {
                bounds.X += left;
                bounds.Width -= marginWidth;
            }
        }

        static void ApplyTopAndBottomPadding(ref Rectangle bounds, int topPadding, int bottomPadding, float scale)
        {
            if (bounds.Height <= 0)
            {
                // No height to work with, nothing to do.
                return;
            }

            int top = (int)MathF.Round(topPadding * scale);
            int bottom = (int)MathF.Round(bottomPadding * scale);
            int marginHeight = top + bottom;
            int remainingHeight = bounds.Height - marginHeight;

            if (remainingHeight < 0)
            {
                if (top > 1 || bottom > 1)
                {
                    // Not enough space to grant full margins, try again at half scale.
                    ApplyTopAndBottomPadding(ref bounds, top, bottom, .5f);
                }
            }
            else
            {
                bounds.Y += top;
                bounds.Height -= marginHeight;
            }
        }
    }
}