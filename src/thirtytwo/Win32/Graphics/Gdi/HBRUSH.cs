// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI brush handle (<c>HBRUSH</c>).
/// </summary>
public unsafe partial struct HBRUSH : IDisposable
{
    /// <summary>
    ///  Sentinel brush value used by APIs that interpret <c>(HBRUSH)-1</c> specially.
    /// </summary>
    public static HBRUSH Invalid => new(-1);

    /// <summary>
    ///  Creates an owned solid brush.
    /// </summary>
    /// <param name="color">Solid fill color.</param>
    /// <returns>A brush handle returned by <c>CreateSolidBrush</c>.</returns>
    public static HBRUSH CreateSolid(Color color) => PInvoke.CreateSolidBrush((COLORREF)color);

    /// <summary>
    ///  Gets a borrowed system-color brush.
    /// </summary>
    /// <param name="color">System color value.</param>
    /// <returns>A cached system brush from <c>GetSysColorBrush</c>. The OS owns this handle.</returns>
    public static implicit operator HBRUSH(SystemColor color) => PInvoke.GetSysColorBrush((SYS_COLOR_INDEX)color);

    /// <summary>
    ///  Gets a borrowed system-color brush.
    /// </summary>
    /// <param name="color">System color index.</param>
    /// <returns>A cached system brush from <c>GetSysColorBrush</c>. The OS owns this handle.</returns>
    public static implicit operator HBRUSH(SYS_COLOR_INDEX color) => PInvoke.GetSysColorBrush(color);

    /// <summary>
    ///  Gets a borrowed stock brush.
    /// </summary>
    /// <param name="brush">Stock brush selector.</param>
    /// <returns>A stock-object brush handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HBRUSH(StockBrush brush) => (HBRUSH)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush);

    /// <summary>
    ///  Reinterprets a generic GDI object handle as a brush handle.
    /// </summary>
    /// <param name="handle">Source handle expected to be <c>OBJ_BRUSH</c> or null.</param>
    /// <returns>The same native value typed as <see cref="HBRUSH"/>.</returns>
    public static explicit operator HBRUSH(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)PInvoke.GetObjectType(handle) == OBJ_TYPE.OBJ_BRUSH);
        return new(handle.Value);
    }

    /// <summary>
    ///  Deletes the brush with <c>DeleteObject</c> when the handle is not null and not <see cref="Invalid"/>.
    /// </summary>
    /// <remarks>
    ///  This method does not validate ownership. Handles borrowed from stock/system brush APIs are not owned by
    ///  the caller and should generally not be disposed through this wrapper.
    /// </remarks>
    public void Dispose()
    {
        if ((nint)Value != 0 && (nint)Value != -1)
        {
            PInvoke.DeleteObject(this);
            Unsafe.AsRef(in this) = default;
        }
    }
}