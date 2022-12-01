// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IOleWindow : IVTable<IOleWindow, IOleWindow.Vtbl>
{
    static void IVTable<IOleWindow, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        vtable->GetWindow_4 = &GetWindow;
        vtable->ContextSensitiveHelp_5 = &ContextSensitiveHelp;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetWindow(IOleWindow* @this, HWND* phwnd)
        => UnwrapAndInvoke<IOleWindow, Interface>(@this, o => o.GetWindow(phwnd));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT ContextSensitiveHelp(IOleWindow* @this, BOOL fEnterMode)
        => UnwrapAndInvoke<IOleWindow, Interface>(@this, o => o.ContextSensitiveHelp(fEnterMode));
}