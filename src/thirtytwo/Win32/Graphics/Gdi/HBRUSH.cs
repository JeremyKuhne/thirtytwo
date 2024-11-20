// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HBRUSH : IDisposable
{
    public static HBRUSH Invalid => new(-1);

    public static HBRUSH CreateSolid(Color color) => Interop.CreateSolidBrush((COLORREF)color);

    public static implicit operator HBRUSH(SystemColor color) => Interop.GetSysColorBrush((SYS_COLOR_INDEX)color);
    public static implicit operator HBRUSH(SYS_COLOR_INDEX color) => Interop.GetSysColorBrush(color);
    public static implicit operator HBRUSH(StockBrush brush) => (HBRUSH)Interop.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush);

    public static explicit operator HBRUSH(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)Interop.GetObjectType(handle) == OBJ_TYPE.OBJ_BRUSH);
        return new(handle.Value);
    }

    public void Dispose()
    {
        if ((nint)Value != 0 && (nint)Value != -1)
        {
            Interop.DeleteObject(this);
            Unsafe.AsRef(in this) = default;
        }
    }
}