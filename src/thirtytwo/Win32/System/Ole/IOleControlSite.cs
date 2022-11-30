// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleControlSite : IVTable<IOleControlSite, IOleControlSite.Vtbl>
{
    static void IVTable<IOleControlSite, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->OnControlInfoChanged_4 = &OnControlInfoChanged;
        vtable->LockInPlaceActive_5 = &LockInPlaceActive;
        vtable->GetExtendedControl_6 = &GetExtendedControl;
        vtable->TransformCoords_7 = &TransformCoords;
        vtable->TranslateAccelerator_8 = &TranslateAccelerator;
        vtable->OnFocus_9 = &OnFocus;
        vtable->ShowPropertyFrame_10 = &ShowPropertyFrame;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnControlInfoChanged(IOleControlSite* @this)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.OnControlInfoChanged());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT LockInPlaceActive(IOleControlSite* @this, BOOL fLock)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.LockInPlaceActive(fLock));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetExtendedControl(IOleControlSite* @this, IDispatch** ppDisp)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.GetExtendedControl(ppDisp));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT TransformCoords(IOleControlSite* @this, POINTL* pPtlHimetric, PointF* pPtfContainer, XFORMCOORDS dwFlags)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.TransformCoords(pPtlHimetric, pPtfContainer, dwFlags));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT TranslateAccelerator(IOleControlSite* @this, MSG* pMsg, KEYMODIFIERS grfModifiers)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.TranslateAccelerator(pMsg, grfModifiers));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnFocus(IOleControlSite* @this, BOOL fGotFocus)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.OnFocus(fGotFocus));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ShowPropertyFrame(IOleControlSite* @this)
        => UnwrapAndInvoke<IOleControlSite, Interface>(@this, o => o.ShowPropertyFrame());
}