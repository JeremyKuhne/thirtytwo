// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Applies scaled padding to incoming bounds before delegating layout to a child handler.
/// </summary>
/// <remarks>
///  <para>
///   Horizontal and vertical margins are resolved independently. Each side is first scaled with
///   <see cref="MathF.Round(float)"/>. When the two margins on an axis exceed the available extent, their rounded
///   integer values are repeatedly halved and rounded again. The first pair whose sum fits is applied; this policy
///   therefore shrinks in powers of two rather than selecting the largest exact fit.
///  </para>
///  <para>
///   Halving stops when both margins on the axis are one pixel or less. If that pair still does not fit, the axis is
///   forwarded unchanged. An axis whose input extent is zero or negative is also unchanged. Negative margins are
///   supported and expand the forwarded bounds when their combined value fits.
///  </para>
/// </remarks>
/// <param name="margin">The logical padding to apply on each edge.</param>
/// <param name="handler">The child handler that receives the padded bounds.</param>
/// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
public class PaddedLayout(
    Padding margin,
    ILayoutHandler handler) : ILayoutHandler
{
    private readonly ILayoutHandler _handler = LayoutValidation.ValidateHandler(handler);

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="scale"/> is not finite or positive.</exception>
    /// <exception cref="OverflowException">
    ///  The scaled padding or resulting bounds cannot be represented by integers.
    /// </exception>
    public void Layout(Rectangle bounds, float scale)
    {
        LayoutValidation.ValidateScale(scale);
        ApplyLeftAndRightPadding(ref bounds, margin.Left, margin.Right, scale);
        ApplyTopAndBottomPadding(ref bounds, margin.Top, margin.Bottom, scale);

        _handler.Layout(bounds, scale);

        static void ApplyLeftAndRightPadding(ref Rectangle bounds, int leftPadding, int rightPadding, float scale)
        {
            if (bounds.Width <= 0)
            {
                // No width to work with, nothing to do.
                return;
            }

            int left = LayoutValidation.MultiplyAndRound(leftPadding, scale);
            int right = LayoutValidation.MultiplyAndRound(rightPadding, scale);

            long marginWidth = (long)left + right;
            long remainingWidth = bounds.Width - marginWidth;
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
                bounds.X = checked(bounds.X + left);
                bounds.Width = checked(bounds.Width - checked((int)marginWidth));
            }
        }

        static void ApplyTopAndBottomPadding(ref Rectangle bounds, int topPadding, int bottomPadding, float scale)
        {
            if (bounds.Height <= 0)
            {
                // No height to work with, nothing to do.
                return;
            }

            int top = LayoutValidation.MultiplyAndRound(topPadding, scale);
            int bottom = LayoutValidation.MultiplyAndRound(bottomPadding, scale);
            long marginHeight = (long)top + bottom;
            long remainingHeight = bounds.Height - marginHeight;

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
                bounds.Y = checked(bounds.Y + top);
                bounds.Height = checked(bounds.Height - checked((int)marginHeight));
            }
        }
    }
}