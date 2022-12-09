// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

public unsafe abstract class AccessibleBase : StandardDispatch, IAccessible.Interface, IManagedWrapper<IAccessible, IDispatch>
{
    // https://learn.microsoft.com/windows/win32/winauto/active-accessibility-user-interface-services-dev-guide

    private static readonly Guid s_accessibilityTypeLib = new("1ea4dbf0-3c3b-11cf-810c-00aa00389b71");

    public static VARIANT Self { get; } = (VARIANT)(int)Interop.CHILDID_SELF;

    public static Rectangle InvalidBounds { get; } = new(int.MinValue, int.MinValue, int.MinValue, int.MinValue);

    public AccessibleBase() : base(s_accessibilityTypeLib, 1, 1, IAccessible.IID_Guid)
    {
    }

    protected VARIANT AsVariant(AccessibleBase accessible)
        => accessible == this ? Self : (VARIANT)Com.GetComPointer<IDispatch>(accessible);

    HRESULT IAccessible.Interface.get_accName(VARIANT varChild, BSTR* pszName) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accValue(VARIANT varChild, BSTR* pszValue) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accDescription(VARIANT varChild, BSTR* pszDescription) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accRole(VARIANT varChild, VARIANT* pvarRole) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accState(VARIANT varChild, VARIANT* pvarState) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accHelp(VARIANT varChild, BSTR* pszHelp) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accHelpTopic(BSTR* pszHelpFile, VARIANT varChild, int* pidTopic) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accKeyboardShortcut(VARIANT varChild, BSTR* pszKeyboardShortcut) => throw new NotImplementedException();

    HRESULT IAccessible.Interface.get_accDefaultAction(VARIANT varChild, BSTR* pszDefaultAction) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.accSelect(int flagsSelect, VARIANT varChild) => throw new NotImplementedException();

    HRESULT IAccessible.Interface.accLocation(int* pxLeft, int* pyTop, int* pcxWidth, int* pcyHeight, VARIANT varChild)
    {
        if (pxLeft is null || pyTop is null || pcxWidth is null || pcyHeight is null)
        {
            return HRESULT.E_POINTER;
        }

        if (varChild.vt != VARENUM.VT_I4)
        {
            return HRESULT.E_INVALIDARG;
        }

        Rectangle bounds = GetLocation((int)varChild);
        if (bounds == InvalidBounds)
        {
            return HRESULT.DISP_E_MEMBERNOTFOUND;
        }

        *pxLeft = bounds.Left;
        *pyTop = bounds.Top;
        *pcxWidth = bounds.Width;
        *pcyHeight = bounds.Height;

        return HRESULT.S_OK;
    }

    /// <summary>
    ///  Gets the bounds of the specified object in screen coordinates.
    /// </summary>
    /// <param name="id"><see cref="Interop.CHILDID_SELF"/> or a child element's id.</param>
    /// <returns>The bounds or <see cref="InvalidBounds"/> if the object is not a visual object.</returns>
    public virtual Rectangle GetLocation(int id) => InvalidBounds;

    protected bool ValidateNavigationDirection(int direction, int childId)
    {
        if (direction <= Interop.NAVDIR_MIN || direction >= Interop.NAVDIR_MAX)
        {
            return false;
        }

        return (uint)direction switch
        {
            Interop.NAVDIR_FIRSTCHILD or Interop.NAVDIR_LASTCHILD => childId == Interop.CHILDID_SELF,
            _ => true,
        };
    }

    protected bool ValidateChild(ref VARIANT child)
    {
        if (child.vt == (VARENUM.VT_VARIANT | VARENUM.VT_BYREF))
        {
            VARIANT* variant = child.data.pvarVal;
            if (variant is null)
            {
                return false;
            }

            child = *variant;
        }

        if (child.vt == VARENUM.VT_EMPTY)
        {
            child = (VARIANT)0;
        }

        return child.vt == VARENUM.VT_I4;
    }

    HRESULT IAccessible.Interface.accNavigate(int navDir, VARIANT varStart, VARIANT* pvarEndUpAt)
    {
        if (pvarEndUpAt is null)
        {
            return HRESULT.E_POINTER;
        }

        *pvarEndUpAt = VARIANT.Empty;

        if (!ValidateChild(ref varStart) || !ValidateNavigationDirection(navDir, (int)varStart))
        {
            return HRESULT.E_INVALIDARG;
        }

        if (!Navigate(navDir, (int)varStart, out VARIANT result))
        {
            return HRESULT.S_FALSE;
        }

        // OBJID_WINDOW Up/Down/Left/Right navigation is as follows:
        //
        //      OBJID_SYSMENU  ↔  OBJID_TITLEBAR
        //                 ↓      ↕
        //                OBJID_MENU
        //                 ↕      ↑
        //       OBJID_CLIENT  ↔  OBJID_VSCROLL
        //                 ↕      ↕
        //      OBJID_HSCROLL  ↔  OBJID_SIZEGRIP
        //
        // Next/Previous navigation for OBJID_WINDOW is index based between OBJID_SYSMENU and OBJID_SIZEGRIP (negated).
        // Presence of these in a window can be determined by looking at the window style. In order:
        //
        //      OBJID_SYSMENU       WS_SYSMENU
        //      OBJID_TITLEBAR      WS_CAPTION
        //      OBJID_MENU          !WS_CHILD && GetMenu() != null
        //      OBJID_CLIENT        Always there
        //      OBJID_VSCROLL       WS_VSCROLL
        //      OBJID_HSCROLL       WS_HSCROLL
        //      OBJID_SIZEGRIP      WS_HSCROLL && WS_VSCROLL
        //
        //
        // OBJID_CLIENT navigation:
        //
        // If the varStart is CHILDID_SELF the WM_GETOBJECT is used to get OBJID_WINDOW and navigation is passed to it
        // with OBJID_CLIENT as the starting position. Otherise varStart is interpreted to be an index into children
        // for the window after dropping the high bit (which allows overloading 0).
        //
        //      NAVDIR_FIRSTCHILD   The first visible child
        //      NAVDIR_LASTCHILD    The last visible child
        //      NAVDIR_NEXT         The next visible child from varStart
        //      NAVDIR_PREVIOUS     The previous visible child from varStart
        //
        // All directional navigation works off of finding the closest visible window bounds in the requested direction.

        * pvarEndUpAt = result;
        return HRESULT.S_OK;
    }

    public virtual bool Navigate(int direction, int startFromId, out VARIANT result)
    {
        result = default;
        return false;
    }

    HRESULT IAccessible.Interface.get_accChild(VARIANT varChild, IDispatch** ppdispChild)
    {
        if (ppdispChild is null)
        {
            return HRESULT.E_POINTER;
        }

        *ppdispChild = null;

        if (varChild.vt == VARENUM.VT_EMPTY)
        {
            varChild = (VARIANT)0;
        }
        else if (varChild.vt != VARENUM.VT_I4)
        {
            return HRESULT.E_INVALIDARG;
        }

        // For default OBJID_CLIENT implementations this always returns S_FALSE.
        // For default OBJID_WINDOW implementations this requires that the type is a valid OBJID (<=0)
        //  (VT_EMPTY is converted to VT_I4/0) and WM_GETOBJECT is generated to retrieve it.

        *ppdispChild = GetChild((int)varChild);
        int id = (int)varChild;

        return *ppdispChild is null ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    public virtual IDispatch* GetChild(int childId) => null;

    HRESULT IAccessible.Interface.get_accParent(IDispatch** ppdispParent)
    {
        if (ppdispParent is null)
        {
            return HRESULT.E_POINTER;
        }

        // For OBJID_WINDOW the default is GetAncestor(GA_PARENT)
        // For OBJID_CLIENT this is the result of WM_GETOBJECT with OBJID_WINDOW

        *ppdispParent = GetParent();
        return *ppdispParent is null ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    public virtual IDispatch* GetParent() => null;

    HRESULT IAccessible.Interface.get_accChildCount(int* pcountChildren)
    {
        if (pcountChildren is null)
        {
            return HRESULT.E_POINTER;
        }

        // For OBJID_CLIENT the default behavior is to count each child window
        // For OBJID_WINDOW the child count is always 7 (OBJID_SYSMENU through OBJID_SIZEGRIP)

        *pcountChildren = GetChildCount();
        return HRESULT.S_OK;
    }

    public virtual int GetChildCount() => 0;

    HRESULT IAccessible.Interface.accHitTest(int xLeft, int yTop, VARIANT* pvarChild)
    {
        // The Win32 AccessibleObjectFromPoint() API finds IAccessible as follows:
        //
        //  - Calls WindowFromPhysicalPoint() to get the HWND at the given point
        //  - Calls GetAncestor(GA_PARENT) until it there is no parent or the parent is the desktop
        //  - Calls AccessibleObjectFromWindow() on the found handle to get IAccessible
        //  - Calls IAccessible::accHitTest() to find the return values

        if (pvarChild is null)
        {
            return HRESULT.E_POINTER;
        }

        *pvarChild = HitTest(new(xLeft, yTop));
        return pvarChild->vt == VARENUM.VT_EMPTY ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    /// <summary>
    ///  Returns the hit accessible object.
    /// </summary>
    /// <returns>
    ///  <list type="bullet">
    ///   <item>Within the bounds of the current object, <see cref="Self"/></item>
    ///   <item>Within the bounds of a child object, <see cref="IDispatch"/></item>
    ///   <item>Within the bounds of a child element, the <see langword="int"/> identifier</item>
    ///   <item>Outside of the bounds, <see cref="VARIANT.Empty"/></item>
    ///  </list>
    /// </returns>
    /// <param name="location">Location in screen coordinates.</param>
    public virtual VARIANT HitTest(Point location) => VARIANT.Empty;

    HRESULT IAccessible.Interface.accDoDefaultAction(VARIANT varChild)
    {
        if (varChild.vt != VARENUM.VT_I4)
        {
            return HRESULT.E_INVALIDARG;
        }

        return !DoDefaultAction((int)varChild) ? HRESULT.DISP_E_MEMBERNOTFOUND : HRESULT.S_OK;
    }

    /// <summary>
    ///  Do the default action for the object, if it has one.
    /// </summary>
    /// <param name="id"><see cref="Interop.CHILDID_SELF"/> or a child element's id.</param>
    /// <returns><see langword="true"/> if the object has a default action.</returns>
    public virtual bool DoDefaultAction(int id) => false;

    HRESULT IAccessible.Interface.put_accName(VARIANT varChild, BSTR szName) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.put_accValue(VARIANT varChild, BSTR szValue) => throw new NotImplementedException();

    HRESULT IAccessible.Interface.get_accFocus(VARIANT* pvarChild) => throw new NotImplementedException();
    HRESULT IAccessible.Interface.get_accSelection(VARIANT* pvarChildren) => throw new NotImplementedException();

    // Default accessibility objects implement the following publicly documented interfaces:
    //
    //      IAccessible, IEnumVARIANT, IOleWindow, IServiceProvider, IAccIdentity
    //
    // For IEnumVARIANT, Next() returns VT_I4 indicies for index based children (everything in OBJID_WINDOW's case),
    // Skip() returns false if at the end. Clone() duplicates the AccessibleObject.
    //
    // For child Windows of OBJID_CLIENT the OBJID_WINDOW object is returned as VT_DISPATCH via WM_GETOBJECT>
}