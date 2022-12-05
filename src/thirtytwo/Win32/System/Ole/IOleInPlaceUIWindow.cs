// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleInPlaceUIWindow : IVTable<IOleInPlaceUIWindow, IOleInPlaceUIWindow.Vtbl>
{
    static void IVTable<IOleInPlaceUIWindow, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->GetWindow_4 = &GetWindow;
        vtable->ContextSensitiveHelp_5 = &ContextSensitiveHelp;
        vtable->GetBorder_6 = &GetBorder;
        vtable->RequestBorderSpace_7 = &RequestBorderSpace;
        vtable->SetBorderSpace_8 = &SetBorderSpace;
        vtable->SetActiveObject_9 = &SetActiveObject;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetWindow(IOleInPlaceUIWindow* @this, HWND* phwnd)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.GetWindow(phwnd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ContextSensitiveHelp(IOleInPlaceUIWindow* @this, BOOL fEnterMode)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.ContextSensitiveHelp(fEnterMode));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetBorder(IOleInPlaceUIWindow* @this, RECT* lprectBorder)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.GetBorder(lprectBorder));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT RequestBorderSpace(IOleInPlaceUIWindow* @this, RECT* pborderwidths)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.RequestBorderSpace(pborderwidths));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetBorderSpace(IOleInPlaceUIWindow* @this, RECT* pborderwidths)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.SetBorderSpace(pborderwidths));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT SetActiveObject(IOleInPlaceUIWindow* @this, IOleInPlaceActiveObject* pActiveObject, PCWSTR pszObjName)
        => UnwrapAndInvoke<IOleInPlaceUIWindow, Interface>(@this, o => o.SetActiveObject(pActiveObject, pszObjName));
}