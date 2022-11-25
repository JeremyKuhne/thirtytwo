// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Com;

namespace Windows.Win32;

public static partial class Interop
{
    /// <returns/>
    /// <inheritdoc cref="GetClassLong(HWND, GET_CLASS_LONG_INDEX)"/>
    [DllImport("USER32.dll", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
    public static extern nuint GetClassLongPtr(
        HWND hWnd,
        GET_CLASS_LONG_INDEX nIndex);

    /// <returns/>
    /// <inheritdoc cref="SetWindowLong(HWND, WINDOW_LONG_PTR_INDEX, int)"/>
    [DllImport("USER32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static extern nint SetWindowLongPtr(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex,
        nint dwNewLong);

    /// <returns>Created <see cref="IShellItem"/>.</returns>
    /// <inheritdoc cref="SHCreateShellItem(ITEMIDLIST*, IShellFolder*, ITEMIDLIST*, IShellItem**)"/>
    public static unsafe ComScope<IShellItem> SHCreateShellItem(string path)
    {
        ComScope<IShellItem> shellItem = new(null);
        SHParseDisplayName(path, pbc: null, out ITEMIDLIST* ppidl, sfgaoIn: 0, psfgaoOut: null).ThrowOnFailure();
        SHCreateShellItem(pidlParent: null, psfParent: null, ppidl, shellItem).ThrowOnFailure();
        return shellItem;
    }
}