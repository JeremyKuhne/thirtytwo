// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  An interface that provides a COM callable wrapper for the implementing class.
/// </summary>
internal interface IManagedWrapper
{
    /// <summary>
    ///  Gets the COM interface table.
    /// </summary>
    /// <returns>The interface table describing COM interfaces exposed by the wrapper.</returns>
    ComInterfaceTable GetInterfaceTable();
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given <typeparamref name="TComInterface"/>. The class
///  must also derive from the given COM wrapper struct's nested Interface.
/// </summary>
/// <typeparam name="TComInterface">The COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface> : IManagedWrapper
    where TComInterface : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; } = ComInterfaceTable.Create<TComInterface>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for <typeparamref name="TComInterface"/>.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given <typeparamref name="TComInterface1"/> and
///  <typeparamref name="TComInterface2"/>. The class must also derive from the given COM wrapper structs' nested
///  Interfaces.
/// </summary>
/// <typeparam name="TComInterface1">The first COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface2">The second COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface1, TComInterface2> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; } = ComInterfaceTable.Create<TComInterface1, TComInterface2>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for the configured interface pair.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given interfaces. The class must also derive from the
///  given COM wrapper structs' nested Interfaces.
/// </summary>
/// <typeparam name="TComInterface1">The first COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface2">The second COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface3">The third COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface1, TComInterface2, TComInterface3> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
    where TComInterface3 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; }
        = ComInterfaceTable.Create<TComInterface1, TComInterface2, TComInterface3>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for the configured interface triple.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given interfaces. The class must also derive from the
///  given COM wrapper structs' nested Interfaces.
/// </summary>
/// <typeparam name="TComInterface1">The first COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface2">The second COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface3">The third COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface4">The fourth COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface1, TComInterface2, TComInterface3, TComInterface4> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
    where TComInterface3 : unmanaged, IComIID, IVTable
    where TComInterface4 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; }
        = ComInterfaceTable.Create<TComInterface1, TComInterface2, TComInterface3, TComInterface4>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for the configured interface set.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given interfaces. The class must also derive from the
///  given COM wrapper structs' nested Interfaces.
/// </summary>
/// <typeparam name="TComInterface1">The first COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface2">The second COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface3">The third COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface4">The fourth COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface5">The fifth COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface1, TComInterface2, TComInterface3, TComInterface4, TComInterface5> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
    where TComInterface3 : unmanaged, IComIID, IVTable
    where TComInterface4 : unmanaged, IComIID, IVTable
    where TComInterface5 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; }
        = ComInterfaceTable.Create<TComInterface1, TComInterface2, TComInterface3, TComInterface4, TComInterface5>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for the configured interface set.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given interfaces. The class must also derive from the
///  given COM wrapper structs' nested Interfaces.
/// </summary>
/// <typeparam name="TComInterface1">The first COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface2">The second COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface3">The third COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface4">The fourth COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface5">The fifth COM interface projection exposed by the wrapper.</typeparam>
/// <typeparam name="TComInterface6">The sixth COM interface projection exposed by the wrapper.</typeparam>
internal interface IManagedWrapper<TComInterface1, TComInterface2, TComInterface3, TComInterface4, TComInterface5, TComInterface6> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
    where TComInterface3 : unmanaged, IComIID, IVTable
    where TComInterface4 : unmanaged, IComIID, IVTable
    where TComInterface5 : unmanaged, IComIID, IVTable
    where TComInterface6 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; }
        = ComInterfaceTable.Create<TComInterface1, TComInterface2, TComInterface3, TComInterface4, TComInterface5, TComInterface6>();

    /// <summary>
    ///  Gets the cached interface table for this wrapper interface set.
    /// </summary>
    /// <returns>The COM interface table for the configured interface set.</returns>
    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}