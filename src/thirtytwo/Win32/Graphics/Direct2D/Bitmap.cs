// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Bitmap : Image, IPointer<ID2D1Bitmap>
{
    public unsafe new ID2D1Bitmap* Pointer => (ID2D1Bitmap*)base.Pointer;

    public Bitmap(ID2D1Bitmap* bitmap) : base((ID2D1Image*)bitmap)
    {
    }

    public static implicit operator ID2D1Bitmap*(Bitmap bitmap) => bitmap.Pointer;
}