// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class SolidBrush : Brush, IPointer<GpSolidFill>
{
    public new GpSolidFill* Pointer => (GpSolidFill*)base.Pointer;

    public SolidBrush(ARGB color) : base(CreateBrush(color))
    {
    }

    private static GpBrush* CreateBrush(ARGB color)
    {
        GpBrush* brush;
        Interop.GdipCreateSolidFill(color, (GpSolidFill**)&brush).ThrowIfFailed();
        return brush;
    }
}