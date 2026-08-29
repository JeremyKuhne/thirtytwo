// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Wraps a native GDI+ graphics object.
/// </summary>
public unsafe class Graphics : DisposableBase.Finalizable, IPointer<GpGraphics>
{
    private GpGraphics* _pointer;

    /// <summary>
    ///  Gets the underlying native graphics pointer.
    /// </summary>
    public GpGraphics* Pointer => _pointer;

    /// <summary>
    ///  Initializes a graphics wrapper from an existing native pointer.
    /// </summary>
    /// <param name="pointer">The native GDI+ graphics pointer to wrap.</param>
    /// <remarks>
    ///  <para>
    ///   The wrapper assumes ownership of the native object and releases it when disposed.
    ///  </para>
    /// </remarks>
    public Graphics(GpGraphics* pointer) => _pointer = pointer;

    /// <summary>
    ///  Creates a graphics object from a GDI device context.
    /// </summary>
    /// <param name="hdc">The device context handle to bind to.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public Graphics(HDC hdc)
    {
        GdiPlus.Init();
        GpGraphics* pointer;
        PInvoke.GdipCreateFromHDC(hdc, &pointer).ThrowIfFailed();
        _pointer = pointer;
    }

    protected override void Dispose(bool disposing)
    {
        Status status = PInvoke.GdipDeleteGraphics(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}