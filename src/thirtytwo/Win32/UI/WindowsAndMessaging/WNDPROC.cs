// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe readonly struct WNDPROC(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> value)
{
    public readonly delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> Value = value;

    public bool IsNull => Value is null;

    public static implicit operator WNDPROC(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> value)
        => new(value);
    public static implicit operator delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>(WNDPROC value)
        => value.Value;

    public static explicit operator WNDPROC(nint value)
        => new((delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)value);
    public static explicit operator WNDPROC(nuint value)
        => new((delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)value);
}