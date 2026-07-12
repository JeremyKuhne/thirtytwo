// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32;

public static unsafe class PInvokeExtensions
{
    extension(PInvoke)
    {
        /// <returns>Created <see cref="IShellItem"/>.</returns>
        /// <inheritdoc cref="Interop.SHCreateShellItem(ITEMIDLIST*, IShellFolder*, ITEMIDLIST*, IShellItem**)"/>
        public static ComScope<IShellItem> SHCreateShellItem(string path)
        {
            ComScope<IShellItem> shellItem = new(null);
            PInvoke.SHParseDisplayName(path, pbc: null, out ITEMIDLIST* ppidl, sfgaoIn: 0).ThrowOnFailure();
            PInvoke.SHCreateShellItem(pidlParent: null, psfParent: null, ppidl, shellItem).ThrowOnFailure();
            return shellItem;
        }
    }
}