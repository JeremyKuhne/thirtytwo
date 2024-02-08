// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Brush : Resource, IPointer<ID2D1Brush>
{
    public unsafe new ID2D1Brush* Pointer => (ID2D1Brush*)base.Pointer;

    public Brush(ID2D1Brush* brush) : base((ID2D1Resource*)brush)
    {
    }

    public static implicit operator ID2D1Brush*(Brush brush) => brush.Pointer;
}