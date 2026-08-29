// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

public partial struct COLORREF
{
    // COLORREF is 0x00BBGGRR
    /// <summary>
    ///  Bit shift for the red byte.
    /// </summary>
    private const int RedShift = 0;

    /// <summary>
    ///  Bit shift for the green byte.
    /// </summary>
    private const int GreenShift = 8;

    /// <summary>
    ///  Bit shift for the blue byte.
    /// </summary>
    private const int BlueShift = 16;

    /// <summary>
    ///  Gets the red channel byte.
    /// </summary>
    public byte R => (byte)((Value >> RedShift) & 0xFF);

    /// <summary>
    ///  Gets the green channel byte.
    /// </summary>
    public byte G => (byte)((Value >> GreenShift) & 0xFF);

    /// <summary>
    ///  Gets the blue channel byte.
    /// </summary>
    public byte B => (byte)((Value >> BlueShift) & 0xFF);

    /// <summary>
    ///  Converts a managed <see cref="Color"/> to its Win32 <see cref="COLORREF"/> representation.
    /// </summary>
    /// <param name="value">The source color.</param>
    /// <returns>The packed <see cref="COLORREF"/> value.</returns>
    public static explicit operator COLORREF(Color value)
        => new((uint)(value.R << RedShift | value.G << GreenShift | value.B << BlueShift));

    /// <summary>
    ///  Converts a <see cref="COLORREF"/> to a managed <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The packed Win32 color value.</param>
    /// <returns>The corresponding managed color.</returns>
    public static implicit operator Color(COLORREF value) => Color.FromArgb(value.R, value.G, value.B);
}