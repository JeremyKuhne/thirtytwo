// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

public unsafe class Bitmap : BitmapSource, IPointer<IWICBitmap>
{
    public unsafe new IWICBitmap* Pointer => (IWICBitmap*)base.Pointer;

    public Bitmap(IWICBitmap* bitmap) : base((IWICBitmapSource*)bitmap)
    {
    }

    public static implicit operator IWICBitmap*(Bitmap bitmap) => bitmap.Pointer;
}