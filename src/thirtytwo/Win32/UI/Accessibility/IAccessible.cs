// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

public unsafe partial struct IAccessible : IVTable<IAccessible, IAccessible.Vtbl>
{
    static void IVTable<IAccessible, Vtbl>.PopulateVTable(Vtbl* vtable)
    {
        IDispatch.PopulateVTable((IDispatch.Vtbl*)vtable);
        PopulateVTable(vtable);
    }
}