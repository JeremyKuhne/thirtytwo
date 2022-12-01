// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.ComWrappers;

namespace Windows.Win32.System.Com;

/// <summary>
///  Wrapper struct for a <see cref="ComInterfaceEntry"/> table.
/// </summary>
internal unsafe readonly struct ComInterfaceTable
{
    public ComInterfaceEntry* Entries { get; init; }
    public int Count { get; init; }

    /// <summary>
    ///  Create an interface table for the given interface.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface>()
        where TComInterface : unmanaged, IComIID, IVTable
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface), sizeof(ComInterfaceEntry));
        entries[0] = new()
        {
            Vtable = (nint)TComInterface.GetVTable(),
            IID = *IID.Get<TComInterface>()
        };

        return new()
        {
            Entries = entries,
            Count = 1
        };
    }

    /// <summary>
    ///  Create an interface table for the given interfaces.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface1, TComInterface2>()
        where TComInterface1 : unmanaged, IComIID, IVTable
        where TComInterface2 : unmanaged, IComIID, IVTable
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface1), sizeof(ComInterfaceEntry) * 2);
        entries[0] = new()
        {
            Vtable = (nint)TComInterface1.GetVTable(),
            IID = *IID.Get<TComInterface1>()
        };

        entries[1] = new()
        {
            Vtable = (nint)TComInterface2.GetVTable(),
            IID = *IID.Get<TComInterface2>()
        };

        return new()
        {
            Entries = entries,
            Count = 2
        };
    }

    /// <summary>
    ///  Create an interface table for the given interfaces.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface1, TComInterface2, TComInterface3>()
        where TComInterface1 : unmanaged, IComIID, IVTable
        where TComInterface2 : unmanaged, IComIID, IVTable
        where TComInterface3 : unmanaged, IComIID, IVTable
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface1), sizeof(ComInterfaceEntry) * 3);
        entries[0] = new()
        {
            Vtable = (nint)TComInterface1.GetVTable(),
            IID = *IID.Get<TComInterface1>()
        };

        entries[1] = new()
        {
            Vtable = (nint)TComInterface2.GetVTable(),
            IID = *IID.Get<TComInterface2>()
        };

        entries[2] = new()
        {
            Vtable = (nint)TComInterface3.GetVTable(),
            IID = *IID.Get<TComInterface3>()
        };

        return new()
        {
            Entries = entries,
            Count = 3
        };
    }

    /// <summary>
    ///  Create an interface table for the given interfaces.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface1, TComInterface2, TComInterface3, TComInterface4>()
        where TComInterface1 : unmanaged, IComIID, IVTable
        where TComInterface2 : unmanaged, IComIID, IVTable
        where TComInterface3 : unmanaged, IComIID, IVTable
        where TComInterface4 : unmanaged, IComIID, IVTable
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface1), sizeof(ComInterfaceEntry) * 4);
        entries[0] = new()
        {
            Vtable = (nint)TComInterface1.GetVTable(),
            IID = *IID.Get<TComInterface1>()
        };

        entries[1] = new()
        {
            Vtable = (nint)TComInterface2.GetVTable(),
            IID = *IID.Get<TComInterface2>()
        };

        entries[2] = new()
        {
            Vtable = (nint)TComInterface3.GetVTable(),
            IID = *IID.Get<TComInterface3>()
        };

        entries[3] = new()
        {
            Vtable = (nint)TComInterface4.GetVTable(),
            IID = *IID.Get<TComInterface4>()
        };

        return new()
        {
            Entries = entries,
            Count = 4
        };
    }

    /// <summary>
    ///  Create an interface table for the given interfaces.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface1, TComInterface2, TComInterface3, TComInterface4, TComInterface5>()
        where TComInterface1 : unmanaged, IComIID, IVTable
        where TComInterface2 : unmanaged, IComIID, IVTable
        where TComInterface3 : unmanaged, IComIID, IVTable
        where TComInterface4 : unmanaged, IComIID, IVTable
        where TComInterface5 : unmanaged, IComIID, IVTable
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(TComInterface1), sizeof(ComInterfaceEntry) * 5);
        entries[0] = new()
        {
            Vtable = (nint)TComInterface1.GetVTable(),
            IID = *IID.Get<TComInterface1>()
        };

        entries[1] = new()
        {
            Vtable = (nint)TComInterface2.GetVTable(),
            IID = *IID.Get<TComInterface2>()
        };

        entries[2] = new()
        {
            Vtable = (nint)TComInterface3.GetVTable(),
            IID = *IID.Get<TComInterface3>()
        };

        entries[3] = new()
        {
            Vtable = (nint)TComInterface4.GetVTable(),
            IID = *IID.Get<TComInterface4>()
        };

        entries[3] = new()
        {
            Vtable = (nint)TComInterface5.GetVTable(),
            IID = *IID.Get<TComInterface5>()
        };

        return new()
        {
            Entries = entries,
            Count = 5
        };
    }
}