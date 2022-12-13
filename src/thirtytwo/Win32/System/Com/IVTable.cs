// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Non generic interface that allows constraining against a COM wrapper type directly. COM structs should
///  implement <see cref="IVTable{TComInterface, TVTable}"/>.
/// </summary>
internal unsafe interface IVTable
{
    static abstract IUnknown.Vtbl* GetVTable();
}

internal unsafe interface IVTable<TComInterface, TVTable> : IVTable
    where TVTable : unmanaged
    where TComInterface : unmanaged, IComIID, IVTable<TComInterface, TVTable>
{
    private static sealed TVTable* VTable { get; set; }

    /// <summary>
    ///  Populate the <see cref="VTable"/> with the relevant function pointers.
    /// </summary>
    private protected static abstract void PopulateVTable(TVTable* vtable);

    static IUnknown.Vtbl* IVTable.GetVTable()
    {
        if (VTable is null)
        {
            TVTable* vtable = (TVTable*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TVTable), sizeof(TVTable));
            ComHelpers.PopulateIUnknown<TComInterface>((IUnknown.Vtbl*)vtable);
            VTable = vtable;
            TComInterface.PopulateVTable(VTable);
        }

        return (IUnknown.Vtbl*)VTable;
    }
}