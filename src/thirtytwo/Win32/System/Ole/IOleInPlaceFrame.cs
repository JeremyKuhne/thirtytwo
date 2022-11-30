// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleInPlaceFrame : IVTable<IOleInPlaceFrame, IOleInPlaceFrame.Vtbl>
{
    static void IVTable<IOleInPlaceFrame, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->GetWindow_4 = &GetWindow;
        vtable->ContextSensitiveHelp_5 = &ContextSensitiveHelp;
        vtable->GetBorder_6 = &GetBorder;
        vtable->RequestBorderSpace_7 = &RequestBorderSpace;
        vtable->SetBorderSpace_8 = &SetBorderSpace;
        vtable->SetActiveObject_9 = &SetActiveObject;
        vtable->InsertMenus_10 = &InsertMenus;
        vtable->SetMenu_11 = &SetMenu;
        vtable->RemoveMenus_12 = &RemoveMenus;
        vtable->SetStatusText_13 = &SetStatusText;
        vtable->EnableModeless_14 = &EnableModeless;
        vtable->TranslateAccelerator_15 = &TranslateAccelerator;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetWindow(IOleInPlaceFrame* @this, HWND* phwnd)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.GetWindow(phwnd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ContextSensitiveHelp(IOleInPlaceFrame* @this, BOOL fEnterMode)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.ContextSensitiveHelp(fEnterMode));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetBorder(IOleInPlaceFrame* @this, RECT* lprectBorder)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.GetBorder(lprectBorder));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT RequestBorderSpace(IOleInPlaceFrame* @this, RECT* pborderwidths)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.RequestBorderSpace(pborderwidths));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetBorderSpace(IOleInPlaceFrame* @this, RECT* pborderwidths)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.SetBorderSpace(pborderwidths));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetActiveObject(IOleInPlaceFrame* @this, IOleInPlaceActiveObject* pActiveObject, PCWSTR pszObjName)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.SetActiveObject(pActiveObject, pszObjName));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT InsertMenus(IOleInPlaceFrame* @this, HMENU hmenuShared, OLEMENUGROUPWIDTHS* lpMenuWidths)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.InsertMenus(hmenuShared, lpMenuWidths));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetMenu(IOleInPlaceFrame* @this, HMENU hmenuShared, nint holemenu, HWND hwndActiveObject)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.SetMenu(hmenuShared, holemenu, hwndActiveObject));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT RemoveMenus(IOleInPlaceFrame* @this, HMENU hmenuShared)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.RemoveMenus(hmenuShared));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetStatusText(IOleInPlaceFrame* @this, PCWSTR pszStatusText)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.SetStatusText(pszStatusText));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT EnableModeless(IOleInPlaceFrame* @this, BOOL fEnable)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.EnableModeless(fEnable));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT TranslateAccelerator(IOleInPlaceFrame* @this, MSG* lpmsg, ushort wID)
        => UnwrapAndInvoke<IOleInPlaceFrame, Interface>(@this, o => o.TranslateAccelerator(lpmsg, wID));
}