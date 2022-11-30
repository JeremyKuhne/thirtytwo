// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleInPlaceSite : IVTable<IOleInPlaceSite, IOleInPlaceSite.Vtbl>
{
    static void IVTable<IOleInPlaceSite, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->GetWindow_4 = &GetWindow;
        vtable->ContextSensitiveHelp_5 = &ContextSensitiveHelp;
        vtable->CanInPlaceActivate_6 = &CanInPlaceActivate;
        vtable->OnInPlaceActivate_7 = &OnInPlaceActivate;
        vtable->OnUIActivate_8 = &OnUIActivate;
        vtable->GetWindowContext_9 = &GetWindowContext;
        vtable->Scroll_10 = &Scroll;
        vtable->OnUIDeactivate_11 = &OnUIDeactivate;
        vtable->OnInPlaceDeactivate_12 = &OnInPlaceDeactivate;
        vtable->DiscardUndoState_13 = &DiscardUndoState;
        vtable->DeactivateAndUndo_14 = &DeactivateAndUndo;
        vtable->OnPosRectChange_15 = &OnPosRectChange;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetWindow(IOleInPlaceSite* @this, HWND* phwnd)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.GetWindow(phwnd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ContextSensitiveHelp(IOleInPlaceSite* @this, BOOL fEnterMode)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.ContextSensitiveHelp(fEnterMode));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT CanInPlaceActivate(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.CanInPlaceActivate());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnInPlaceActivate(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.OnInPlaceActivate());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnUIActivate(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.OnUIActivate());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetWindowContext(
        IOleInPlaceSite* @this,
        IOleInPlaceFrame** ppFrame,
        IOleInPlaceUIWindow** ppDoc,
        RECT* lprcPosRect,
        RECT* lprcClipRect,
        OLEINPLACEFRAMEINFO* lpFrameInfo)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.GetWindowContext(ppFrame, ppDoc, lprcPosRect, lprcClipRect, lpFrameInfo));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Scroll(IOleInPlaceSite* @this, SIZE scrollExtant)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.Scroll(scrollExtant));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnUIDeactivate(IOleInPlaceSite* @this, BOOL fUndoable)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.OnUIDeactivate(fUndoable));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnInPlaceDeactivate(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.OnInPlaceDeactivate());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT DiscardUndoState(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.DiscardUndoState());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT DeactivateAndUndo(IOleInPlaceSite* @this)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.DeactivateAndUndo());

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT OnPosRectChange(IOleInPlaceSite* @this, RECT* lprcPosRect)
        => UnwrapAndInvoke<IOleInPlaceSite, Interface>(@this, o => o.OnPosRectChange(lprcPosRect));
}