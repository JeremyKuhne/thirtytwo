// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.UI.Accessibility;

public unsafe partial struct IAccessible : IVTable<IAccessible, IAccessible.Vtbl>
{
    static void IVTable<IAccessible, Vtbl>.InitializeVTable(Vtbl* vtable)
    {
        IDispatch.InitializeVTable((IDispatch.Vtbl*)vtable);
        vtable->get_accParent_8 = &get_accParent;
        vtable->get_accChildCount_9 = &get_accChildCount;
        vtable->get_accChild_10 = &get_accChild;
        vtable->get_accName_11 = &get_accName;
        vtable->get_accValue_12 = &get_accValue;
        vtable->get_accDescription_13 = &get_accDescription;
        vtable->get_accRole_14 = &get_accRole;
        vtable->get_accState_15 = &get_accState;
        vtable->get_accHelp_16 = &get_accHelp;
        vtable->get_accHelpTopic_17 = &get_accHelpTopic;
        vtable->get_accKeyboardShortcut_18 = &get_accKeyboardShortcut;
        vtable->get_accFocus_19 = &get_accFocus;
        vtable->get_accSelection_20 = &get_accSelection;
        vtable->get_accDefaultAction_21 = &get_accDefaultAction;
        vtable->accSelect_22 = &accSelect;
        vtable->accLocation_23 = &accLocation;
        vtable->accNavigate_24 = &accNavigate;
        vtable->accHitTest_25 = &accHitTest;
        vtable->accDoDefaultAction_26 = &accDoDefaultAction;
        vtable->put_accName_27 = &put_accName;
        vtable->put_accValue_28 = &put_accValue;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accParent(IAccessible* @this, IDispatch** ppdispParent)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o =>
        {
            if (ppdispParent is null)
            {
                return HRESULT.E_POINTER;
            }

            *ppdispParent = o.accParent;
            return HRESULT.S_OK;
        });

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accChildCount(IAccessible* @this, int* pcountChildren)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o =>
        {
            if (pcountChildren is null)
            {
                return HRESULT.E_POINTER;
            }

            *pcountChildren = o.accChildCount;
            return HRESULT.S_OK;
        });

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accChild(IAccessible* @this, VARIANT varChild, IDispatch** ppdispChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accChild(varChild, ppdispChild));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accName(IAccessible* @this, VARIANT varChild, BSTR* pszName)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accName(varChild, pszName));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accValue(IAccessible* @this, VARIANT varChild, BSTR* pszValue)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accValue(varChild, pszValue));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accDescription(IAccessible* @this, VARIANT varChild, BSTR *pszDescription)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accDescription(varChild, pszDescription));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accRole(IAccessible* @this, VARIANT varChild, VARIANT *pvarRole)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accRole(varChild, pvarRole));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accState(IAccessible* @this, VARIANT varChild, VARIANT *pvarState)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accState(varChild, pvarState));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accHelp(IAccessible* @this, VARIANT varChild, BSTR *pszHelp)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accHelp(varChild, pszHelp));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accHelpTopic(IAccessible* @this, BSTR* pszHelpFile, VARIANT varChild, int* pidTopic)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accHelpTopic(pszHelpFile, varChild, pidTopic));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accKeyboardShortcut(IAccessible* @this, VARIANT varChild, BSTR *pszKeyboardShortcut)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accKeyboardShortcut(varChild, pszKeyboardShortcut));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accFocus(IAccessible* @this, VARIANT* pvarChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o =>
        {
            if (pvarChild is null)
            {
                return HRESULT.E_POINTER;
            }

            *pvarChild = o.accFocus;
            return HRESULT.S_OK;
        });

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accSelection(IAccessible* @this, VARIANT *pvarChildren)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o =>
        {
            if (pvarChildren is null)
            {
                return HRESULT.E_POINTER;
            }

            *pvarChildren = o.accSelection;
            return HRESULT.S_OK;
        });

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT get_accDefaultAction(IAccessible* @this, VARIANT varChild, BSTR *pszDefaultAction)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.get_accDefaultAction(varChild, pszDefaultAction));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT accSelect(IAccessible* @this, int flagsSelect, VARIANT varChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.accSelect(flagsSelect, varChild));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT accLocation(IAccessible* @this, int* pxLeft, int* pyTop, int* pcxWidth, int* pcyHeight, VARIANT varChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.accLocation(pxLeft, pyTop, pcxWidth, pcyHeight, varChild));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT accNavigate(IAccessible* @this, int navDir, VARIANT varStart, VARIANT *pvarEndUpAt)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.accNavigate(navDir, varStart, pvarEndUpAt));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT accHitTest(IAccessible* @this, int xLeft, int yTop, VARIANT* pvarChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.accHitTest(xLeft, yTop, pvarChild));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT accDoDefaultAction(IAccessible* @this, VARIANT varChild)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.accDoDefaultAction(varChild));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT put_accName(IAccessible* @this, VARIANT varChild, BSTR szName)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.put_accName(varChild, szName));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT put_accValue(IAccessible* @this, VARIANT varChild, BSTR szValue)
        => UnwrapAndInvoke<IAccessible, Interface>(@this, o => o.put_accValue(varChild, szValue));
}