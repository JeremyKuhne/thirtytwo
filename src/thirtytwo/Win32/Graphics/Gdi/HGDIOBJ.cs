// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  Generic GDI object handle (<c>HGDIOBJ</c>).
/// </summary>
public partial struct HGDIOBJ
{
    /// <summary>
    ///  Gets the runtime object type reported by Win32.
    /// </summary>
    /// <returns>
    ///  The <see cref="OBJ_TYPE"/> returned by <c>GetObjectType</c>. Returns zero when Win32 cannot
    ///  determine a type for the handle.
    /// </returns>
    public OBJ_TYPE GetObjectType() => (OBJ_TYPE)PInvoke.GetObjectType(this);

    /// <summary>
    ///  Gets a borrowed stock font handle as a generic GDI object.
    /// </summary>
    /// <param name="font">Stock font selector.</param>
    /// <returns>The stock object handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HGDIOBJ(StockFont font) => new((nint)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)font));

    /// <summary>
    ///  Gets a borrowed stock brush handle as a generic GDI object.
    /// </summary>
    /// <param name="brush">Stock brush selector.</param>
    /// <returns>The stock object handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HGDIOBJ(StockBrush brush) => new((nint)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush));

    /// <summary>
    ///  Gets a borrowed stock brush mapped from a system color as a generic GDI object.
    /// </summary>
    /// <param name="color">System color selector.</param>
    /// <returns>The stock object handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HGDIOBJ(SystemColor color) => new((nint)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)color));

    /// <summary>
    ///  Gets a borrowed stock pen handle as a generic GDI object.
    /// </summary>
    /// <param name="pen">Stock pen selector.</param>
    /// <returns>The stock object handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HGDIOBJ(StockPen pen) => new((nint)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)pen));

    /// <summary>
    ///  Gets a borrowed stock object handle.
    /// </summary>
    /// <param name="stockObject">Stock object selector.</param>
    /// <returns>The stock object handle returned by <c>GetStockObject</c>.</returns>
    public static implicit operator HGDIOBJ(GET_STOCK_OBJECT_FLAGS stockObject) => new((nint)PInvoke.GetStockObject(stockObject));
}