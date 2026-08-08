// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Validates shared layout-construction invariants.</summary>
internal static class LayoutValidation
{
    private const double PercentageTolerance = 0.00001;

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