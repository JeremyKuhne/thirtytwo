// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HGDIOBJ
{
    public OBJ_TYPE GetObjectType() => (OBJ_TYPE)Interop.GetObjectType(this);

    public static implicit operator HGDIOBJ(StockFont font) => new(Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)font));
    public static implicit operator HGDIOBJ(StockBrush brush) => new(Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush));
    public static implicit operator HGDIOBJ(SystemColor color) => new(Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)color));
    public static implicit operator HGDIOBJ(StockPen pen) => new(Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)pen));
    public static implicit operator HGDIOBJ(GET_STOCK_OBJECT_FLAGS stockObject) => new(Interop.GetStockObject(stockObject));
}