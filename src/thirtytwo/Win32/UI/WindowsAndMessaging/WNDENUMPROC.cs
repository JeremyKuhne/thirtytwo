// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe readonly struct WNDENUMPROC
{
    public readonly delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> Value;

    public WNDENUMPROC(delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> value) => Value = value;
    public bool IsNull => Value is null;

    public static implicit operator WNDENUMPROC(delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> value)
        => new(value);
    public static implicit operator delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL>(WNDENUMPROC value)
        => value.Value;

    public static explicit operator WNDENUMPROC(nint value)
        => new((delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL>)value);
}