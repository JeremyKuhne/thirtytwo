// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace System;

internal static unsafe class ManagedWrapperExtensions
{
    /// <summary>
    ///  Gets a COM callable wrapper (CCW) of the given <typeparamref name="TInterface"/>.
    /// </summary>
    public static ComScope<TInterface> GetComCallableWrapper<TInterface>(this IManagedWrapper wrapper)
        where TInterface : unmanaged, IComIID
        => ComScope<TInterface>.GetComCallableWrapper(wrapper);

    /// <summary>
    ///  Gets a COM callable wrapper (CCW).
    /// </summary>
    public static ComScope<IUnknown> GetComCallableWrapper(this IManagedWrapper wrapper)
        => ComScope<IUnknown>.GetComCallableWrapper(wrapper);
}