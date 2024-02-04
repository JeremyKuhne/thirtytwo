// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class Graphics : DisposableBase.Finalizable, IPointer<GpGraphics>
{
    private GpGraphics* _pointer;

    public GpGraphics* Pointer => _pointer;

    public Graphics(GpGraphics* pointer) => _pointer = pointer;

    public Graphics(HDC hdc)
    {
        GdiPlus.Init();
        GpGraphics* pointer;
        Interop.GdipCreateFromHDC(hdc, &pointer).ThrowIfFailed();
        _pointer = pointer;
    }

    protected override void Dispose(bool disposing)
    {
        Status status = Interop.GdipDeleteGraphics(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}