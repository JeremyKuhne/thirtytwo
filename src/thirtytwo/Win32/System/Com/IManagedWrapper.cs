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
    ComInterfaceTable GetInterfaceTable();
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given <typeparamref name="TComInterface"/>. The class
///  must also derive from the given COM wrapper struct's nested Interface.
/// </summary>
internal interface IManagedWrapper<TComInterface> : IManagedWrapper
    where TComInterface : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; set; } = ComInterfaceTable.Create<TComInterface>();

    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}

/// <summary>
///  Apply to a class to apply a COM callable wrapper of the given <typeparamref name="TComInterface1"/> and
///  <typeparamref name="TComInterface2"/>. The class must also derive from the given COM wrapper structs' nested
///  Interfaces.
/// </summary>
internal interface IManagedWrapper<TComInterface1, TComInterface2> : IManagedWrapper
    where TComInterface1 : unmanaged, IComIID, IVTable
    where TComInterface2 : unmanaged, IComIID, IVTable
{
    private static ComInterfaceTable InterfaceTable { get; set; } = ComInterfaceTable.Create<TComInterface1, TComInterface2>();

    ComInterfaceTable IManagedWrapper.GetInterfaceTable() => InterfaceTable;
}