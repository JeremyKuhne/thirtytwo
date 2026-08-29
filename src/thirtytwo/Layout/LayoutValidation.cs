// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Provides shared validation for proportional layout definitions.
/// </summary>
internal static class LayoutValidation
{
    /// <summary>
    ///  The maximum allowed absolute difference between the total percentage and 1.0.
    /// </summary>
    private const double PercentageTolerance = 0.00001;

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