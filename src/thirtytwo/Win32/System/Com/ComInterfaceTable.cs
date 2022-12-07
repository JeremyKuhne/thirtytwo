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
        Span<ComInterfaceEntry> entries = AllocateEntries<TComInterface>(2);
        entries[0] = GetEntry<IComCallableWrapper>();
        entries[1] = GetEntry<TComInterface>();

        return new()
        {
            Entries = (ComInterfaceEntry*)Unsafe.AsPointer(ref entries[0]),
            Count = entries.Length
        };
    }

    /// <summary>
    ///  Create an interface table for the given interfaces.
    /// </summary>
    public static ComInterfaceTable Create<TComInterface1, TComInterface2>()
        where TComInterface1 : unmanaged, IComIID, IVTable
        where TComInterface2 : unmanaged, IComIID, IVTable
    {
        Span<ComInterfaceEntry> entries = AllocateEntries<TComInterface1>(3);
        entries[0] = GetEntry<IComCallableWrapper>();
        entries[1] = GetEntry<TComInterface1>();
        entries[2] = GetEntry<TComInterface2>();

        return new()
        {
            Entries = (ComInterfaceEntry*)Unsafe.AsPointer(ref entries[0]),
            Count = entries.Length
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
        Span<ComInterfaceEntry> entries = AllocateEntries<TComInterface1>(4);
        entries[0] = GetEntry<IComCallableWrapper>();
        entries[1] = GetEntry<TComInterface1>();
        entries[2] = GetEntry<TComInterface2>();
        entries[3] = GetEntry<TComInterface3>();

        return new()
        {
            Entries = (ComInterfaceEntry*)Unsafe.AsPointer(ref entries[0]),
            Count = entries.Length
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
        Span<ComInterfaceEntry> entries = AllocateEntries<TComInterface1>(5);
        entries[0] = GetEntry<IComCallableWrapper>();
        entries[1] = GetEntry<TComInterface1>();
        entries[2] = GetEntry<TComInterface2>();
        entries[3] = GetEntry<TComInterface3>();
        entries[4] = GetEntry<TComInterface4>();

        return new()
        {
            Entries = (ComInterfaceEntry*)Unsafe.AsPointer(ref entries[0]),
            Count = entries.Length
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
        Span<ComInterfaceEntry> entries = AllocateEntries<TComInterface1>(6);
        entries[0] = GetEntry<IComCallableWrapper>();
        entries[1] = GetEntry<TComInterface1>();
        entries[2] = GetEntry<TComInterface2>();
        entries[3] = GetEntry<TComInterface3>();
        entries[4] = GetEntry<TComInterface4>();
        entries[5] = GetEntry<TComInterface5>();

        return new()
        {
            Entries = (ComInterfaceEntry*)Unsafe.AsPointer(ref entries[0]),
            Count = entries.Length
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<ComInterfaceEntry> AllocateEntries<T>(int count)
        => new(
            (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(T), sizeof(ComInterfaceEntry) * count),
            count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ComInterfaceEntry GetEntry<TComInterface>() where TComInterface : unmanaged, IComIID, IVTable
        => new()
        {
            Vtable = (nint)TComInterface.GetVTable(),
            IID = *IID.Get<TComInterface>()
        };
}