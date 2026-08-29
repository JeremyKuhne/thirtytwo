// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
///  Strongly typed wrapper for a window procedure callback pointer.
/// </summary>
/// <param name="value">
///  Callback address using <c>Stdcall</c> ABI with signature
///  <c>(HWND, uint, WPARAM, LPARAM) -> LRESULT</c>.
/// </param>
public unsafe readonly struct WNDPROC(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> value)
{
    /// <summary>
    ///  Gets the underlying function pointer value.
    /// </summary>
    public readonly delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> Value = value;

    /// <summary>
    ///  Gets whether the callback pointer is null.
    /// </summary>
    public bool IsNull => Value is null;

    /// <summary>
    ///  Wraps a raw callback pointer.
    /// </summary>
    /// <param name="value">Callback pointer to wrap.</param>
    /// <returns>A <see cref="WNDPROC"/> wrapper.</returns>
    public static implicit operator WNDPROC(delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT> value)
        => new(value);

    /// <summary>
    ///  Unwraps the raw callback pointer.
    /// </summary>
    /// <param name="value">Wrapper instance to unwrap.</param>
    /// <returns>The underlying callback pointer.</returns>
    public static implicit operator delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>(WNDPROC value)
        => value.Value;

    /// <summary>
    ///  Reinterprets a native signed integer as a callback pointer.
    /// </summary>
    /// <param name="value">Native integer callback address.</param>
    /// <returns>A <see cref="WNDPROC"/> wrapper for <paramref name="value"/>.</returns>
    public static explicit operator WNDPROC(nint value)
        => new((delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)value);

    /// <summary>
    ///  Reinterprets a native unsigned integer as a callback pointer.
    /// </summary>
    /// <param name="value">Native unsigned integer callback address.</param>
    /// <returns>A <see cref="WNDPROC"/> wrapper for <paramref name="value"/>.</returns>
    public static explicit operator WNDPROC(nuint value)
        => new((delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)value);
}