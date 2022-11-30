// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Com;

public unsafe partial struct IEnumUnknown : IVTable<IEnumUnknown, IEnumUnknown.Vtbl>
{
    static void IVTable<IEnumUnknown, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->Next_4 = &Next;
        vtable->Skip_5 = &Skip;
        vtable->Reset_6 = &Reset;
        vtable->Clone_7 = &Clone;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Next(IEnumUnknown* @this, uint celt, IUnknown** rgelt, uint* pceltFetched)
        => UnwrapAndInvoke<IEnumUnknown, Interface>(@this, o => o.Next(celt, rgelt, pceltFetched));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Skip(IEnumUnknown* @this, uint celt)
        => UnwrapAndInvoke<IEnumUnknown, Interface>(@this, o => o.Skip(celt));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Reset(IEnumUnknown* @this)
        => UnwrapAndInvoke<IEnumUnknown, Interface>(@this, o => o.Reset());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Clone(IEnumUnknown* @this, IEnumUnknown** ppenum)
        => UnwrapAndInvoke<IEnumUnknown, Interface>(@this, o => o.Clone(ppenum));
}