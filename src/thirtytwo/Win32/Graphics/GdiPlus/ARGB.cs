// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.Graphics.GdiPlus;

[StructLayout(LayoutKind.Explicit)]
public readonly struct ARGB
{
    [FieldOffset(0)]
    public readonly byte B;
    [FieldOffset(1)]
    public readonly byte G;
    [FieldOffset(2)]
    public readonly byte R;
    [FieldOffset(3)]
    public readonly byte A;

    [FieldOffset(0)]
    public readonly uint Value;

    public ARGB(uint value)
    {
        Unsafe.SkipInit(out this);
        Value = value;
    }

    public ARGB(byte red, byte green, byte blue)
        : this(255, red, green, blue)
    {
    }

    [SkipLocalsInit]
    public ARGB(byte alpha, byte red, byte green, byte blue)
    {
        Unsafe.SkipInit(out this);
        A = alpha;
        R = red;
        G = green;
        B = blue;
    }

    public static implicit operator ARGB(COLORREF color) => new(color.R, color.G, color.B);
    public static explicit operator COLORREF(ARGB color) => (COLORREF)((Color)color);
    public static implicit operator ARGB(Color color) => new(color.A, color.R, color.G, color.B);
    public static implicit operator Color(ARGB color) => Color.FromArgb((int)color.Value);
    public static implicit operator uint(ARGB color) => color.Value;
}