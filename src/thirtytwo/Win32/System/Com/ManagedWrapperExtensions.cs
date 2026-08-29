// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace System;

/// <summary>
///  Extension methods for obtaining COM callable wrappers from <see cref="IManagedWrapper"/> instances.
/// </summary>
internal static unsafe class ManagedWrapperExtensions
{
    /// <summary>
    ///  Gets a COM callable wrapper (CCW) of the given <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">The COM interface pointer type to retrieve.</typeparam>
    /// <param name="wrapper">The managed wrapper instance.</param>
    /// <returns>A scope over the returned interface pointer. Disposing the scope releases the COM reference.</returns>
    public static ComScope<TInterface> GetComCallableWrapper<TInterface>(this IManagedWrapper wrapper)
        where TInterface : unmanaged, IComIID
        => new(wrapper.GetComPointer<TInterface>());

    /// <summary>
    ///  Gets a COM callable wrapper (CCW).
    /// </summary>
    /// <param name="wrapper">The managed wrapper instance.</param>
    /// <returns>
    ///  A scope over the returned <see cref="IUnknown"/> pointer. Disposing the scope releases the COM reference.
    /// </returns>
    public static ComScope<IUnknown> GetComCallableWrapper(this IManagedWrapper wrapper)
        => new(wrapper.GetComPointer<IUnknown>());
}