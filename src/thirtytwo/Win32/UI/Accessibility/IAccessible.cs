// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

/// <summary>
///  COM interface wrapper for the native <c>IAccessible</c> vtable.
/// </summary>
public unsafe partial struct IAccessible : IVTable<IAccessible, IAccessible.Vtbl>
{
    /// <summary>
    ///  Writes the managed callable wrapper entries for <c>IAccessible</c> into the provided vtable.
    /// </summary>
    /// <param name="vtable">
    ///  Destination vtable memory that receives inherited <c>IDispatch</c> entries followed by
    ///  <c>IAccessible</c>-specific entries.
    /// </param>
    static void IVTable<IAccessible, Vtbl>.PopulateVTable(Vtbl* vtable)
    {
        IDispatchCcw.PopulateVTable((IDispatchCcw.Vtbl*)vtable);
        PopulateVTable(vtable);
    }
}