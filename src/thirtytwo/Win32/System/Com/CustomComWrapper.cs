// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Dialogs;

namespace Windows.Win32.System.Com;

internal unsafe sealed class CustomComWrapper : ComWrappers
{
    internal static CustomComWrapper Instance { get; } = new();

    private static readonly ComInterfaceEntry* s_fileDialogEvents = InitializeEntry<IFileDialogEvents, IFileDialogEvents.Vtbl>();

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

    internal static IUnknown* GetComInterfaceForObject(object obj)
        => Instance.ComputeVtables(obj, default, out _) is not null
            ? (IUnknown*)Instance.GetOrCreateComInterfaceForObject(obj, CreateComInterfaceFlags.None)
            : null;

    protected override unsafe ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        if (obj is FileDialog.FileDialogEvents)
        {
            count = 1;
            return s_fileDialogEvents;
        }

        count = 0;
        return null;
    }

    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags)
    {
        return null;
    }

    protected override void ReleaseObjects(IEnumerable objects) => throw new NotImplementedException();

}