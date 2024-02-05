// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class Brush : DisposableBase.Finalizable, IPointer<GpBrush>
{
    private GpBrush* _pointer;

    public GpBrush* Pointer => _pointer;

    public Brush(GpBrush* pointer) => _pointer = pointer;

    protected override void Dispose(bool disposing)
    {
        Status status = Interop.GdipDeleteBrush(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}