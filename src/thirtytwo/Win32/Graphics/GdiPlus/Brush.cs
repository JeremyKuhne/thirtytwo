// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Wraps a native GDI+ brush object.
/// </summary>
public unsafe class Brush : DisposableBase.Finalizable, IPointer<GpBrush>
{
    private GpBrush* _pointer;

    /// <summary>
    ///  Gets the underlying native brush pointer.
    /// </summary>
    public GpBrush* Pointer => _pointer;

    /// <summary>
    ///  Initializes a brush wrapper from an existing native pointer.
    /// </summary>
    /// <param name="pointer">The native GDI+ brush pointer to wrap.</param>
    /// <remarks>
    ///  <para>
    ///   The wrapper assumes ownership of the native object and releases it when disposed.
    ///  </para>
    /// </remarks>
    public Brush(GpBrush* pointer) => _pointer = pointer;

    protected override void Dispose(bool disposing)
    {
        Status status = PInvoke.GdipDeleteBrush(_pointer);
        if (disposing)
        {
            status.ThrowIfFailed();
        }

        _pointer = null;
    }
}