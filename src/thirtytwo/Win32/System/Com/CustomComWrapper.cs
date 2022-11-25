// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

internal unsafe sealed class CustomComWrapper : ComWrappers
{
    private static ComInterfaceEntry* InitializeEntry<TComInterface, TVTable>()
        where TComInterface : unmanaged, IInitializeVTable<TVTable>, IComIID
        where TVTable : unmanaged
    {
        TVTable* vtable = (TVTable*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface), sizeof(TVTable));

        IUnknown.Vtbl* unknown = (IUnknown.Vtbl*)vtable;

        GetIUnknownImpl(out IntPtr fpQueryInterface, out IntPtr fpAddRef, out IntPtr fpRelease);
        unknown->QueryInterface_1 = (delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT>)fpQueryInterface;
        unknown->AddRef_2 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpAddRef;
        unknown->Release_3 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpRelease;

        TComInterface.PopulateVTable(vtable);

        ComInterfaceEntry* wrapperEntry = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(CustomComWrapper), sizeof(ComInterfaceEntry));
        wrapperEntry->IID = *IID.Get<TComInterface>();
        wrapperEntry->Vtable = (nint)(void*)vtable;
        return wrapperEntry;
    }

    protected override unsafe ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        throw new NotImplementedException();
    }

    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags)
    {
        return null;
    }

    protected override void ReleaseObjects(IEnumerable objects) => throw new NotImplementedException();

}