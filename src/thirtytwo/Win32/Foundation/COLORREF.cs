// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

public partial struct COLORREF
{
    // COLORREF is 0x00BBGGRR
    private const int RedShift = 0;
    private const int GreenShift = 8;
    private const int BlueShift = 16;

    public byte R => (byte)((Value >> RedShift) & 0xFF);
    public byte G => (byte)((Value >> GreenShift) & 0xFF);
    public byte B => (byte)((Value >> BlueShift) & 0xFF);

    public static explicit operator COLORREF(Color value)
        => new((uint)(value.R << RedShift | value.G << GreenShift | value.B << BlueShift));

    public static implicit operator Color(COLORREF value) => Color.FromArgb(value.R, value.G, value.B);
}