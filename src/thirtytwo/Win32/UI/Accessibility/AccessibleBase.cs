// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

public unsafe abstract class AccessibleBase : StandardDispatch, IAccessible.Interface, IManagedWrapper<IAccessible, IDispatch>
{
    private static readonly Guid s_accessibilityTypeLib = new("1ea4dbf0-3c3b-11cf-810c-00aa00389b71");

    public AccessibleBase() : base(s_accessibilityTypeLib, 1, 1, IAccessible.IID_Guid)
    {
    }

    public virtual unsafe IDispatch* accParent => null;
    public virtual int accChildCount => 0;

    public virtual unsafe HRESULT get_accChild(VARIANT varChild, IDispatch** ppdispChild)
    {
        if (ppdispChild is null)
        {
            return HRESULT.E_POINTER;
        }

        if (varChild.vt != VARENUM.VT_I4)
        {
            *ppdispChild = null;
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild == Interop.CHILDID_SELF)
        {
            // TODO: return self
        }

        *ppdispChild = null;
        return HRESULT.S_FALSE;
    }

    public virtual HRESULT get_accName(VARIANT varChild, BSTR* pszName) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accValue(VARIANT varChild, BSTR* pszValue) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accDescription(VARIANT varChild, BSTR* pszDescription) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accRole(VARIANT varChild, VARIANT* pvarRole) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accState(VARIANT varChild, VARIANT* pvarState) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accHelp(VARIANT varChild, BSTR* pszHelp) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accHelpTopic(BSTR* pszHelpFile, VARIANT varChild, int* pidTopic) => HRESULT.E_NOTIMPL;
    public virtual HRESULT get_accKeyboardShortcut(VARIANT varChild, BSTR* pszKeyboardShortcut) => HRESULT.E_NOTIMPL;

    public virtual VARIANT accFocus => default;
    public virtual VARIANT accSelection => default;

    public virtual HRESULT get_accDefaultAction(VARIANT varChild, BSTR* pszDefaultAction) => HRESULT.E_NOTIMPL;
    public virtual HRESULT accSelect(int flagsSelect, VARIANT varChild) => HRESULT.E_NOTIMPL;
    public virtual HRESULT accLocation(int* pxLeft, int* pyTop, int* pcxWidth, int* pcyHeight, VARIANT varChild) => HRESULT.E_NOTIMPL;
    public virtual HRESULT accNavigate(int navDir, VARIANT varStart, VARIANT* pvarEndUpAt) => HRESULT.E_NOTIMPL;
    public virtual HRESULT accHitTest(int xLeft, int yTop, VARIANT* pvarChild) => HRESULT.E_NOTIMPL;
    public virtual HRESULT accDoDefaultAction(VARIANT varChild) => HRESULT.E_NOTIMPL;
    public virtual HRESULT put_accName(VARIANT varChild, BSTR szName) => HRESULT.E_NOTIMPL;
    public virtual HRESULT put_accValue(VARIANT varChild, BSTR szValue) => HRESULT.E_NOTIMPL;
}