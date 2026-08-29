// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  <para>
///   Wraps a native GDI+ pen object.
///  </para>
/// </summary>
public unsafe class Pen : DisposableBase.Finalizable, IPointer<GpPen>
{
    private GpPen* _pointer;

    /// <summary>
    ///  <para>
    ///   Gets the underlying native pen pointer.
    ///  </para>
    /// </summary>
    public GpPen* Pointer => _pointer;

    /// <summary>
    ///  <para>
    ///   Creates a pen with the specified color and width.
    ///  </para>
    /// </summary>
    /// <param name="color">
    ///  <para>
    ///   The ARGB color of the pen.
    ///  </para>
    /// </param>
    /// <param name="width">
    ///  <para>
    ///   The pen width in pixels.
    ///  </para>
    /// </param>
    /// <exception cref="Exception">
    ///  <para>
    ///   The underlying GDI+ operation failed.
    ///  </para>
    /// </exception>
    public Pen(ARGB color, float width = 1.0f)
    {
        GdiPlus.Init();
        GpPen* pointer;
        PInvoke.GdipCreatePen1(color, width, Unit.UnitPixel, &pointer).ThrowIfFailed();
        _pointer = pointer;
    }

    protected override void Dispose(bool disposing)
    {
        Status status = PInvoke.GdipDeletePen(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}