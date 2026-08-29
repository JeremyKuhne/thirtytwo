// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Variant;

namespace Windows.Win32.UI.Accessibility;

public partial struct UiaRect
{
    /// <summary>
    ///  Converts a <see cref="Rectangle"/> into a <see cref="UiaRect"/>.
    /// </summary>
    /// <param name="value">The rectangle to convert.</param>
    /// <returns>A <see cref="UiaRect"/> with matching origin and size values.</returns>
    /// <remarks>
    ///  The conversion copies integer <see cref="Rectangle"/> members directly into
    ///  the floating-point fields of <see cref="UiaRect"/>.
    /// </remarks>
    public static explicit operator UiaRect(Rectangle value) => new()
    {
        height = value.Height,
        width = value.Width,
        left = value.X,
        top = value.Y
    };

    /// <summary>
    ///  Converts a <see cref="UiaRect"/> into a <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="value">The rectangle to convert.</param>
    /// <returns>A <see cref="Rectangle"/> built from rounded <see cref="UiaRect"/> values.</returns>
    /// <remarks>
    ///  Each coordinate and dimension is rounded with <see cref="Math.Round(double)"/>
    ///  before being cast to <see cref="int"/>.
    /// </remarks>
    public static explicit operator Rectangle(UiaRect value) => new(
        (int)Math.Round(value.left),
        (int)Math.Round(value.top),
        (int)Math.Round(value.width),
        (int)Math.Round(value.height));

    /// <summary>
    ///  Converts the current rectangle into a <see cref="VARIANT"/> containing four doubles.
    /// </summary>
    /// <returns>A <see cref="VARIANT"/> initialized from the in-memory <see cref="UiaRect"/> data.</returns>
    /// <remarks>
    ///  This method passes the struct as a pointer to four contiguous <see cref="double"/> values
    ///  to <see cref="PInvoke.InitVariantFromDoubleArray(double*,uint,Windows.Win32.System.Variant.VARIANT*)"/>.
    ///  Failure HRESULT values are converted to exceptions via <c>ThrowOnFailure()</c>.
    /// </remarks>
    public unsafe VARIANT ToVARIANT()
    {
        VARIANT variant = default;
        fixed (UiaRect* u = &this)
        {
            PInvoke.InitVariantFromDoubleArray((double*)u, 4, &variant).ThrowOnFailure();
        }

        return variant;
    }
}