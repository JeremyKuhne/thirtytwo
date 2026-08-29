// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32;

/// <summary>
///  Convenience wrappers around selected PInvoke entry points.
/// </summary>
public static unsafe class PInvokeExtensions
{
    extension(PInvoke)
    {
        /// <summary>
        ///  Creates an <see cref="IShellItem"/> for the specified filesystem <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Path to parse with <c>SHParseDisplayName</c>.</param>
        /// <returns>Created shell item in a <see cref="ComScope{T}"/> owned by the caller.</returns>
        /// <inheritdoc cref="Interop.SHCreateShellItem(ITEMIDLIST*, IShellFolder*, ITEMIDLIST*, IShellItem**)"/>
        /// <exception cref="Exception">Thrown when parsing the path or creating the shell item fails.</exception>
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