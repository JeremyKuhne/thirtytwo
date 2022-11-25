// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

internal unsafe interface IInitializeVTable<TVTable>
    where TVTable : unmanaged
{
    protected internal static abstract void PopulateVTable(TVTable* vtable);
}