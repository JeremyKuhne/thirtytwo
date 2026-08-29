// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Wraps a native GDI+ solid brush object.
/// </summary>
public unsafe class SolidBrush : Brush, IPointer<GpSolidFill>
{
    /// <summary>
    ///  Gets the underlying native solid brush pointer.
    /// </summary>
    public new GpSolidFill* Pointer => (GpSolidFill*)base.Pointer;

    /// <summary>
    ///  Creates a solid brush with the specified color.
    /// </summary>
    /// <param name="color">The ARGB color to use when filling.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public SolidBrush(ARGB color) : base(CreateBrush(color))
    {
    }

    private static GpBrush* CreateBrush(ARGB color)
    {
        GpBrush* brush;
        PInvoke.GdipCreateSolidFill(color, (GpSolidFill**)&brush).ThrowIfFailed();
        return brush;
    }
}