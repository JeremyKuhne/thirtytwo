// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI pen handle (<c>HPEN</c>).
/// </summary>
public unsafe partial struct HPEN : IDisposable
{
    /// <summary>
    ///  Creates an owned solid pen.
    /// </summary>
    /// <param name="color">Pen color.</param>
    /// <param name="width">Pen width in logical units of the destination device context.</param>
    /// <returns>A pen handle returned by <c>CreatePen</c>.</returns>
    public static HPEN CreatePen(Color color, int width = 1) =>
        PInvoke.CreatePen(PEN_STYLE.PS_SOLID, width, (COLORREF)color);

    /// <summary>
    ///  Gets a borrowed stock pen.
    /// </summary>
    /// <param name="brush">Stock pen selector.</param>
    /// <returns>A stock-object pen handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HPEN(StockPen brush) => (HPEN)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush);

    /// <summary>
    ///  Reinterprets a generic GDI object handle as a pen handle.
    /// </summary>
    /// <param name="handle">Source handle expected to be <c>OBJ_PEN</c> or null.</param>
    /// <returns>The same native value typed as <see cref="HPEN"/>.</returns>
    public static explicit operator HPEN(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)PInvoke.GetObjectType(handle) == OBJ_TYPE.OBJ_PEN);
        return new(handle.Value);
    }

    /// <summary>
    ///  Deletes the pen with <c>DeleteObject</c> when the handle is not null and not <c>(HPEN)-1</c>.
    /// </summary>
    /// <remarks>
    ///  This method does not validate ownership. Handles borrowed from stock-pen APIs are not owned by the caller
    ///  and should generally not be disposed through this wrapper.
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