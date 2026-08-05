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
            ITEMIDLIST* itemIdList = null;
            HRESULT result;

            fixed (char* pathPointer = path)
            {
                result = PInvoke.SHParseDisplayName(
                    pathPointer,
                    pbc: null,
                    &itemIdList,
                    sfgaoIn: 0,
                    psfgaoOut: null);
            }

            try
            {
                result.ThrowOnFailure();
                PInvoke.SHCreateShellItem(
                    pidlParent: null,
                    psfParent: null,
                    itemIdList,
                    shellItem).ThrowOnFailure();
                return shellItem;
            }
            finally
            {
                PInvoke.CoTaskMemFree(itemIdList);
            }
        }
    }
}