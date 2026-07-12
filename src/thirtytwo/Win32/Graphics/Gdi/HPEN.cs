// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HPEN : IDisposable
{
    public static HPEN CreatePen(Color color, int width = 1) =>
        PInvoke.CreatePen(PEN_STYLE.PS_SOLID, width, (COLORREF)color);

    public static implicit operator HPEN(StockPen brush) => (HPEN)PInvoke.GetStockObject((GET_STOCK_OBJECT_FLAGS)brush);

    public static explicit operator HPEN(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)PInvoke.GetObjectType(handle) == OBJ_TYPE.OBJ_PEN);
        return new(handle.Value);
    }

    public void Dispose()
    {
        if ((nint)Value != 0 && (nint)Value != -1)
        {
            PInvoke.DeleteObject(this);
            Unsafe.AsRef(in this) = default;
        }
    }
}