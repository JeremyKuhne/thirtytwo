// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleContainer : IVTable<IOleContainer, IOleContainer.Vtbl>
{
    static void IVTable<IOleContainer, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->ParseDisplayName_4 = &ParseDisplayName;
        vtable->EnumObjects_5 = &EnumObjects;
        vtable->LockContainer_6 = &LockContainer;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ParseDisplayName(IOleContainer* @this, IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut)
        => UnwrapAndInvoke<IOleContainer, Interface>(@this, o => o.ParseDisplayName(pbc, pszDisplayName, pchEaten, ppmkOut));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT EnumObjects(IOleContainer* @this, OLECONTF grfFlags, IEnumUnknown** ppenum)
        => UnwrapAndInvoke<IOleContainer, Interface>(@this, o => o.EnumObjects(grfFlags, ppenum));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT LockContainer(IOleContainer* @this, BOOL fLock)
        => UnwrapAndInvoke<IOleContainer, Interface>(@this, o => o.LockContainer(fLock));
}