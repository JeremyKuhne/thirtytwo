// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class Pen : DisposableBase.Finalizable, IPointer<GpPen>
{
    private GpPen* _pointer;

    public GpPen* Pointer => _pointer;

    public Pen(ARGB color, float width = 1.0f)
    {
        GdiPlus.Init();
        GpPen* pointer;
        Interop.GdipCreatePen1(color, width, Unit.UnitPixel, &pointer).ThrowIfFailed();
        _pointer = pointer;
    }

    protected override void Dispose(bool disposing)
    {
        Status status = Interop.GdipDeletePen(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}
