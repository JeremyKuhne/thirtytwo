﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Dialogs;

namespace Windows.Win32.System.Com;

internal unsafe sealed class CustomComWrapper : ComWrappers
{
    internal static CustomComWrapper Instance { get; } = new();

    internal static void PopulateIUnknown(IUnknown.Vtbl* vtable)
    {
        GetIUnknownImpl(out nint fpQueryInterface, out nint fpAddRef, out nint fpRelease);
        vtable->QueryInterface_1 = (delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT>)fpQueryInterface;
        vtable->AddRef_2 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpAddRef;
        vtable->Release_3 = (delegate* unmanaged[Stdcall]<IUnknown*, uint>)fpRelease;
    }

    internal static IUnknown* GetComInterfaceForObject(object obj)
        => obj is IManagedWrapper
            ? (IUnknown*)Instance.GetOrCreateComInterfaceForObject(obj, CreateComInterfaceFlags.None)
            : null;

    protected override unsafe ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        if (obj is not IManagedWrapper wrapper)
        {
            count = 0;
            return null;
        }

        ComInterfaceTable table = wrapper.GetInterfaceTable();
        count = table.Count;
        return table.Entries;
    }

    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags)
    {
        return null;
    }

    protected override void ReleaseObjects(IEnumerable objects) => throw new NotImplementedException();
}