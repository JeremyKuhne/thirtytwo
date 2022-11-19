// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Gdi;

public partial struct HDC : IHandle<HDC>
{
    HDC IHandle<HDC>.Handle => this;
    object? IHandle<HDC>.Wrapper => null;

    public static explicit operator HDC(HGDIOBJ handle)
    {
        Debug.Assert(handle.IsNull || (OBJ_TYPE)Interop.GetObjectType(handle) == OBJ_TYPE.OBJ_DC);
        return new(handle.Value);
    }

    public static implicit operator HGDIOBJ(HDC handle) => new(handle.Value);
}
