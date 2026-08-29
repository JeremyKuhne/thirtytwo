// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.InteropServices;

namespace Windows.Win32.System.Com;

/// <summary>
///  Provides COM wrapper policy for managed objects that implement <see cref="IManagedWrapper"/>.
/// </summary>
internal sealed unsafe partial class CustomComWrappers : ComWrappers
{
    /// <summary>
    ///  Gets the singleton wrapper instance used by this library.
    /// </summary>
    internal static CustomComWrappers Instance { get; } = new();

    /// <summary>
    ///  Gets an <see cref="IUnknown"/> pointer for a managed wrapper object.
    /// </summary>
    /// <param name="obj">Managed object to expose as COM.</param>
    /// <returns>
    ///  A caller-owned AddRef'd <see cref="IUnknown"/> pointer when supported; otherwise <see langword="null"/>.
    /// </returns>
    internal static IUnknown* GetComInterfaceForObject(object obj)
    {
        if (obj is not IManagedWrapper)
        {
            return null;
        }

        IUnknown* result = (IUnknown*)Instance.GetOrCreateComInterfaceForObject(
            obj,
            CreateComInterfaceFlags.None);

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>
    ///  <para>
    ///   VTables are provided only for <see cref="IManagedWrapper"/> instances and include a sentinel
    ///   <see cref="IComCallableWrapper"/> entry.
    ///  </para>
    /// </remarks>
    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
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

    /// <inheritdoc/>
    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags)
    {
        return null;
    }

    /// <inheritdoc/>
    protected override void ReleaseObjects(IEnumerable objects) => throw new NotImplementedException();
}