// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  A 32-bit ARGB color value used by GDI+.
/// </summary>
/// <remarks>
///  <para>
///   The channel bytes are laid out in memory as blue, green, red, and alpha.
///  </para>
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ARGB
{
    /// <summary>
    ///  The blue color channel.
    /// </summary>
    [FieldOffset(0)]
    public readonly byte B;

    /// <summary>
    ///  The green color channel.
    /// </summary>
    [FieldOffset(1)]
    public readonly byte G;

    /// <summary>
    ///  The red color channel.
    /// </summary>
    [FieldOffset(2)]
    public readonly byte R;

    /// <summary>
    ///  The alpha channel.
    /// </summary>
    [FieldOffset(3)]
    public readonly byte A;

    /// <summary>
    ///  The packed 32-bit ARGB value.
    /// </summary>
    [FieldOffset(0)]
    public readonly uint Value;

    /// <summary>
    ///  Initializes a color from a packed 32-bit ARGB value.
    /// </summary>
    /// <param name="value">The packed ARGB value.</param>
    public ARGB(uint value)
    {
        Unsafe.SkipInit(out this);
        Value = value;
    }

    /// <summary>
    ///  Initializes an opaque color from red, green, and blue channels.
    /// </summary>
    /// <param name="red">The red channel value.</param>
    /// <param name="green">The green channel value.</param>
    /// <param name="blue">The blue channel value.</param>
    public ARGB(byte red, byte green, byte blue)
        : this(255, red, green, blue)
    {
    }

    /// <summary>
    ///  Initializes a color from alpha, red, green, and blue channels.
    /// </summary>
    /// <param name="alpha">The alpha channel value.</param>
    /// <param name="red">The red channel value.</param>
    /// <param name="green">The green channel value.</param>
    /// <param name="blue">The blue channel value.</param>
    [SkipLocalsInit]
    public ARGB(byte alpha, byte red, byte green, byte blue)
    {
        Unsafe.SkipInit(out this);
        A = alpha;
        R = red;
        G = green;
        B = blue;
    }

    /// <summary>
    ///  Converts a Win32 color to an opaque <see cref="ARGB"/> value.
    /// </summary>
    /// <param name="color">The Win32 color value.</param>
    /// <returns>An opaque <see cref="ARGB"/> value.</returns>
    public static implicit operator ARGB(COLORREF color) => new(color.R, color.G, color.B);

    /// <summary>
    ///  Converts an <see cref="ARGB"/> value to a Win32 color.
    /// </summary>
    /// <param name="color">The ARGB value to convert.</param>
    /// <returns>The corresponding Win32 color value.</returns>
    public static explicit operator COLORREF(ARGB color) => (COLORREF)((Color)color);

    /// <summary>
    ///  Converts a <see cref="Color"/> to an <see cref="ARGB"/> value.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The corresponding <see cref="ARGB"/> value.</returns>
    public static implicit operator ARGB(Color color) => new(color.A, color.R, color.G, color.B);

    /// <summary>
    ///  Converts an <see cref="ARGB"/> value to a <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The ARGB value to convert.</param>
    /// <returns>The corresponding <see cref="Color"/>.</returns>
    public static implicit operator Color(ARGB color) => Color.FromArgb((int)color.Value);

    /// <summary>
    ///  Converts an <see cref="ARGB"/> value to its packed 32-bit representation.
    /// </summary>
    /// <param name="color">The ARGB value to convert.</param>
    /// <returns>The packed 32-bit ARGB value.</returns>
    public static implicit operator uint(ARGB color) => color.Value;
}