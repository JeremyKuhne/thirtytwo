// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Wraps a native GDI+ image object.
/// </summary>
public unsafe class Image : DisposableBase.Finalizable, IPointer<GpImage>
{
    private GpImage* _pointer;

    /// <summary>
    ///  Gets the underlying native image pointer.
    /// </summary>
    public GpImage* Pointer => _pointer;

    /// <summary>
    ///  Initializes an image wrapper from an existing native pointer.
    /// </summary>
    /// <param name="pointer">The native GDI+ image pointer to wrap.</param>
    /// <remarks>
    ///  <para>
    ///   The wrapper assumes ownership of the native object and releases it when disposed.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Gets the pixel format of the image.
    /// </summary>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
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

    /// <summary>
    ///  Gets the raw image format identifier.
    /// </summary>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
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

    /// <summary>
    ///  Gets the image capability flags.
    /// </summary>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
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
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
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