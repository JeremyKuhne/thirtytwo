// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

public unsafe class BitmapSource : DirectDrawBase<IWICBitmapSource>
{
    public BitmapSource(IWICBitmapSource* pointer) : base(pointer) { }

    public static implicit operator IWICBitmapSource*(BitmapSource d) => d.Pointer;
}