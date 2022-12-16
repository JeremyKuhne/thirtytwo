// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This isn't working, getting "Fatal error. Invalid Program: attempted to call a UnmanagedCallersOnly method from managed code."
// somewhere in ComWrappers.TryGetOrCreateComInterfaceForObjectInternal. Attempting to debug hangs Visual Studio.
// #define TRACK_UNKNOWN

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

internal sealed unsafe partial class CustomComWrappers : ComWrappers
{
    private static readonly IUnknown.Vtbl* s_wrappersUnknown = GetComWrappersUnknown();

    private static IUnknown.Vtbl* GetComWrappersUnknown()
    {
        IUnknown.Vtbl* vtable = (IUnknown.Vtbl*)RuntimeHelpers.AllocateTypeAssociatedMemory(
            typeof(CustomComWrappers),
            sizeof(IUnknown.Vtbl));

        GetIUnknownImpl(out nint fpQueryInterface, out nint fpAddRef, out nint fpRelease);
        vtable->QueryInterface_1 = (delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT>)fpQueryInterface;
        vtable->AddRef_2 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpAddRef;
        vtable->Release_3 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpRelease;

        return vtable;
    }

    internal static CustomComWrappers Instance { get; } = new();

    internal static void PopulateIUnknown(IUnknown.Vtbl* vtable)
    {
#if TRACK_UNKNOWN
        vtable->QueryInterface_1 = LoggerVTable->QueryInterface_1;
        vtable->AddRef_2 = LoggerVTable->AddRef_2;
        vtable->Release_3 = LoggerVTable->Release_3;
#else
        vtable->QueryInterface_1 = s_wrappersUnknown->QueryInterface_1;
        vtable->AddRef_2 = s_wrappersUnknown->AddRef_2;
        vtable->Release_3 = s_wrappersUnknown->Release_3;
#endif
    }

    internal static IUnknown* GetComInterfaceForObject(object obj)
    {
        if (obj is not IManagedWrapper)
        {
            return null;
        }

        IUnknown* result = (IUnknown*)Instance.GetOrCreateComInterfaceForObject(
            obj,
#if TRACK_UNKNOWN
            CreateComInterfaceFlags.CallerDefinedIUnknown);
#else
            CreateComInterfaceFlags.None);
#endif

        if (obj is IWrapperInitialize initialize)
        {
            initialize.OnInitialized(result);
        }

        return result;
    }

    protected override unsafe ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        if (obj is not IManagedWrapper wrapper)
        {
            count = 0;
            return null;
        }

        ComInterfaceTable table = wrapper.GetInterfaceTable();
        count = table.Count;
        ComInterfaceEntry* entries = table.Entries;

#if TRACK_UNKNOWN
        entries[count] = new() { IID = *IID.Get<IUnknown>(), Vtable = (nint)LoggerVTable };
        count++;
#endif

        return table.Entries;
    }

    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags)
    {
        return null;
    }

    protected override void ReleaseObjects(IEnumerable objects) => throw new NotImplementedException();
}