// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows;

/// <summary>
///  Provides shared validation and checked conversion for layout definitions.
/// </summary>
internal static class LayoutValidation
{
    /// <summary>
    ///  The maximum allowed absolute difference between the total percentage and 1.0.
    /// </summary>
    private const double PercentageTolerance = 0.00001;

    /// <summary>
    ///  Ensures that a child layout handler is present.
    /// </summary>
    /// <param name="handler">The handler to validate.</param>
    /// <param name="parameterName">The caller expression to use if validation fails.</param>
    /// <returns>The validated handler.</returns>
    internal static ILayoutHandler ValidateHandler(
        ILayoutHandler handler,
        [CallerArgumentExpression(nameof(handler))] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(handler, parameterName);
        return handler;
    }

    /// <summary>
    ///  Ensures that a horizontal alignment value is defined.
    /// </summary>
    /// <param name="alignment">The alignment to validate.</param>
    /// <param name="parameterName">The public parameter name to use if validation fails.</param>
    /// <returns>The validated alignment.</returns>
    internal static HorizontalAlignment ValidateAlignment(HorizontalAlignment alignment, string parameterName)
        => Enum.IsDefined(alignment)
            ? alignment
            : throw new ArgumentOutOfRangeException(parameterName, alignment, "The alignment value is not defined.");

    /// <summary>
    ///  Ensures that a vertical alignment value is defined.
    /// </summary>
    /// <param name="alignment">The alignment to validate.</param>
    /// <param name="parameterName">The public parameter name to use if validation fails.</param>
    /// <returns>The validated alignment.</returns>
    internal static VerticalAlignment ValidateAlignment(VerticalAlignment alignment, string parameterName)
        => Enum.IsDefined(alignment)
            ? alignment
            : throw new ArgumentOutOfRangeException(parameterName, alignment, "The alignment value is not defined.");

    /// <summary>
    ///  Ensures that a fixed-size percentage is finite and nonnegative.
    /// </summary>
    /// <param name="percent">The percentage to validate.</param>
    /// <param name="parameterName">The public parameter name to use if validation fails.</param>
    /// <returns>The validated percentage.</returns>
    internal static float ValidateFixedPercent(float percent, string parameterName)
        => float.IsFinite(percent) && percent >= 0
            ? percent
            : throw new ArgumentOutOfRangeException(
                parameterName,
                percent,
                "The percentage must be finite and nonnegative.");

    /// <summary>
    ///  Ensures that a fixed logical size has nonnegative dimensions.
    /// </summary>
    /// <param name="size">The size to validate.</param>
    /// <returns>The validated size.</returns>
    internal static Size ValidateFixedSize(Size size)
        => size.Width >= 0 && size.Height >= 0
            ? size
            : throw new ArgumentOutOfRangeException(nameof(size), size, "Width and height must be nonnegative.");

    /// <summary>
    ///  Ensures that a layout scale is finite and greater than zero.
    /// </summary>
    /// <param name="scale">The scale to validate.</param>
    internal static void ValidateScale(float scale)
    {
        if (!float.IsFinite(scale) || scale <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                "The scale must be finite and greater than zero.");
        }
    }

    /// <summary>
    ///  Multiplies an integer dimension by a factor and rounds to the nearest integer without silent overflow.
    /// </summary>
    /// <param name="value">The integer dimension to multiply.</param>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>The rounded integer result.</returns>
    internal static int MultiplyAndRound(int value, float factor)
        => factor == 1.0f ? value : checked((int)MathF.Round(value * factor));

    /// <summary>
    ///  Multiplies an integer dimension by a factor and truncates toward zero without silent overflow.
    /// </summary>
    /// <param name="value">The integer dimension to multiply.</param>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>The truncated integer result.</returns>
    internal static int MultiplyAndTruncate(int value, float factor)
        => factor == 1.0f ? value : checked((int)(value * factor));

    /// <summary>
    ///  Validates proportional child definitions used by split-layout containers.
    /// </summary>
    /// <param name="handlers">The handlers to validate.</param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="handlers"/> is <see langword="null"/>, or one of the handler entries is
    ///  <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  A percentage is not finite, a percentage falls outside <c>0.0</c> through <c>1.0</c>, or the total
    ///  percentage does not equal <c>1.0</c> within tolerance.
    /// </exception>
    internal static void ValidateProportionalHandlers((float Percent, ILayoutHandler Handler)[] handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        double totalPercent = 0;
        foreach ((float percent, ILayoutHandler handler) in handlers)
        {
            if (!float.IsFinite(percent) || percent < 0 || percent > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(handlers),
                    percent,
                    "Each percentage must be finite and between 0.0 and 1.0.");
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handlers), "A layout handler cannot be null.");
            }

            totalPercent += percent;
        }

        if (Math.Abs(totalPercent - 1.0) > PercentageTolerance)
        {
            throw new ArgumentOutOfRangeException(nameof(handlers), "Total percentage must be 1.0.");
        }
    }
}