// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

public unsafe class BitmapFrameDecode : BitmapSource, IPointer<IWICBitmapFrameDecode>
{
    public new IWICBitmapFrameDecode* Pointer => (IWICBitmapFrameDecode*)base.Pointer;

    public BitmapFrameDecode(IWICBitmapFrameDecode* pointer) : base((IWICBitmapSource*)pointer) { }

    public static implicit operator IWICBitmapFrameDecode*(BitmapFrameDecode d) => d.Pointer;
}