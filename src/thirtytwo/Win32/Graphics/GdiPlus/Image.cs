// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class Image : DisposableBase.Finalizable, IPointer<GpImage>
{
    private GpImage* _pointer;

    public GpImage* Pointer => _pointer;

    public Image(GpImage* pointer) => _pointer = pointer;

    protected override void Dispose(bool disposing)
    {
        Status status = PInvoke.GdipDisposeImage(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }

    public PixelFormat PixelFormat
    {
        get
        {
            PixelFormat format;
            PInvoke.GdipGetImagePixelFormat(Pointer, (int*)&format).ThrowIfFailed();
            GC.KeepAlive(this);
            return format;
        }
    }

    public Guid RawFormat
    {
        get
        {
            Guid format;
            PInvoke.GdipGetImageRawFormat(Pointer, &format).ThrowIfFailed();
            GC.KeepAlive(this);
            return format;
        }
    }

    public ImageFlags Flags
    {
        get
        {
            ImageFlags flags;
            PInvoke.GdipGetImageFlags(Pointer, (uint*)&flags).ThrowIfFailed();
            GC.KeepAlive(this);
            return flags;
        }
    }

    /// <summary>
    ///  The bounds of the image in pixels.
    /// </summary>
    public RectangleF Bounds
    {
        get
        {
            RectangleF bounds;
            Unit unit;
            PInvoke.GdipGetImageBounds(Pointer, (RectF*)&bounds, &unit).ThrowIfFailed();

            // GdipGetImageBounds is hardcoded to return Unit.Pixel
            Debug.Assert(unit == Unit.UnitPixel);
            GC.KeepAlive(this);
            return bounds;
        }
    }
}