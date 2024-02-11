// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Image : Resource, IPointer<ID2D1Image>
{
    public unsafe new ID2D1Image* Pointer => (ID2D1Image*)base.Pointer;

    public Image(ID2D1Image* image) : base((ID2D1Resource*)image)
    {
    }

    public static implicit operator ID2D1Image*(Image image) => image.Pointer;
}