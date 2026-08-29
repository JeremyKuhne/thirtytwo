// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

/// <summary>
///  Strongly typed wrapper for a window-enumeration callback pointer.
/// </summary>
/// <param name="value">
///  Callback address using <c>Stdcall</c> ABI with signature
///  <c>(HWND, LPARAM) -> BOOL</c>.
/// </param>
public unsafe readonly struct WNDENUMPROC(delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> value)
{
    /// <summary>
    ///  Gets the underlying function pointer value.
    /// </summary>
    public readonly delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> Value = value;

    /// <summary>
    ///  Gets whether the callback pointer is null.
    /// </summary>
    public bool IsNull => Value is null;

    /// <summary>
    ///  Wraps a raw callback pointer.
    /// </summary>
    /// <param name="value">Callback pointer to wrap.</param>
    /// <returns>A <see cref="WNDENUMPROC"/> wrapper.</returns>
    public static implicit operator WNDENUMPROC(delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL> value)
        => new(value);

    /// <summary>
    ///  Unwraps the raw callback pointer.
    /// </summary>
    /// <param name="value">Wrapper instance to unwrap.</param>
    /// <returns>The underlying callback pointer.</returns>
    public static implicit operator delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL>(WNDENUMPROC value)
        => value.Value;

    /// <summary>
    ///  Reinterprets a native integer as a callback pointer.
    /// </summary>
    /// <param name="value">Native integer callback address.</param>
    /// <returns>A <see cref="WNDENUMPROC"/> wrapper for <paramref name="value"/>.</returns>
    public static explicit operator WNDENUMPROC(nint value)
        => new((delegate* unmanaged[Stdcall]<HWND, LPARAM, BOOL>)value);
}