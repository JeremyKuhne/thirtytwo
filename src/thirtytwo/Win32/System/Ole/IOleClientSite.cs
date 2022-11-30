// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleClientSite : IVTable<IOleClientSite, IOleClientSite.Vtbl>
{
    static void IVTable<IOleClientSite, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->SaveObject_4 = &SaveObject;
        vtable->GetMoniker_5 = &GetMoniker;
        vtable->GetContainer_6 = &GetContainer;
        vtable->ShowObject_7 = &ShowObject;
        vtable->OnShowWindow_8 = &OnShowWindow;
        vtable->RequestNewObjectLayout_9 = &RequestNewObjectLayout;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SaveObject(IOleClientSite* @this)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.SaveObject());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetMoniker(IOleClientSite* @this, OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, IMoniker** ppmk)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.GetMoniker(dwAssign, dwWhichMoniker, ppmk));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetContainer(IOleClientSite* @this, IOleContainer** ppContainer)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.GetContainer(ppContainer));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ShowObject(IOleClientSite* @this)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.ShowObject());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnShowWindow(IOleClientSite* @this, BOOL fShow)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.OnShowWindow(fShow));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT RequestNewObjectLayout(IOleClientSite* @this)
        => UnwrapAndInvoke<IOleClientSite, Interface>(@this, o => o.RequestNewObjectLayout());
}