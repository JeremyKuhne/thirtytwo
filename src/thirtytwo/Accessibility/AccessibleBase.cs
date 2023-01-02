// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace Windows.Accessibility;

/// <summary>
///  Base accessibility class that implements <see cref="IAccessible"/>. Derive from either <see cref="LegacyAccessibleBase"/>
///  for just Active Accessibility or <see cref="UiaBase"/> for UI Automation support as well.
/// </summary>
/// <remarks>
///  Trying to use this class directly will not get picked up by <see cref="CustomComWrappers"/> and will end up
///  with the .NET provided <see cref="IDispatch"/>.
/// </remarks>
public unsafe abstract class AccessibleBase : StandardDispatch, IAccessible.Interface
{
    // https://learn.microsoft.com/windows/win32/winauto/active-accessibility-user-interface-services-dev-guide

    // https://learn.microsoft.com/windows/win32/winauto/window
    // https://learn.microsoft.com/windows/win32/winauto/client-object

    // The accessibility TypeLib- lives in oleacc.dll
    private static readonly Guid s_accessibilityTypeLib = new("1ea4dbf0-3c3b-11cf-810c-00aa00389b71");

    public static VARIANT Self { get; } = (VARIANT)(int)Interop.CHILDID_SELF;

    public static Rectangle InvalidBounds { get; } = new(int.MinValue, int.MinValue, int.MinValue, int.MinValue);

    private readonly IAccessible.Interface? _childHandler;

    /// <param name="childHandler">
    ///  Used to delegate calls to when referring to a child id other than <see cref="Interop.CHILDID_SELF"/>.
    /// </param>
    public AccessibleBase(IAccessible.Interface? childHandler = default)
        : base(s_accessibilityTypeLib, 1, 1, IAccessible.IID_Guid)
    {
        _childHandler = childHandler;
    }

    protected VARIANT AsVariant(AccessibleBase accessible)
        => accessible == this ? Self : (VARIANT)ComHelpers.GetComPointer<IDispatch>(accessible);

    HRESULT IAccessible.Interface.get_accDescription(VARIANT varChild, BSTR* pszDescription)
    {
        if (pszDescription is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        // OBJID_WINDOW doesn't have a description, but it's children do.
        // OBJID_CLIENT doesn't have this by default. Docs recommend not to support this as UIA doesn't use it.

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            return _childHandler?.get_accDescription(varChild, pszDescription) ?? HRESULT.S_FALSE;
        }

        if (Description is not { } description)
        {
            *pszDescription = default;
            return HRESULT.S_FALSE;
        }

        *pszDescription = new(description);
        return HRESULT.S_OK;
    }

    public virtual string? Description => null;

    HRESULT IAccessible.Interface.get_accRole(VARIANT varChild, VARIANT* pvarRole)
    {
        if (pvarRole is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pvarRole = VARIANT.Empty;
            return _childHandler?.get_accRole(varChild, pvarRole) ?? HRESULT.S_FALSE;
        }

        // For OBJID_WINDOW this is ROLE_SYSTEM_WINDOW. For OBJID_CLIENT this is ROLE_SYSTEM_CLIENT.

        *pvarRole = (VARIANT)(int)Role;
        return HRESULT.S_OK;
    }

    /// <summary>
    ///  Returns the <see href="https://learn.microsoft.com/windows/win32/winauto/object-roles">role</see> of the object.
    /// </summary>
    public virtual ObjectRoles Role => ObjectRoles.Client;

    HRESULT IAccessible.Interface.get_accState(VARIANT varChild, VARIANT* pvarState)
    {
        if (pvarState is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        // For OBJID_WINDOW & OBJID_CLIENT this maps the following:
        //
        //  !WS_VISIBLE         STATE_SYSTEM_INVISIBLE
        //  WS_DISABLED         STATE_SYSTEM_UNAVAILABLE
        //  WS_THICKFRAME       STATE_SYSTEM_SIZEABLE
        //  WS_CAPTION          STATE_SYSTEM_MOVEABLE | STATE_SYSTEM_FOCUSABLE
        //
        // OBJID_CLIENT always adds STATE_SYSTEM_FOCUSABLE and adds STATE_SYSTEM_FOCUSED if the HWND has focus

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pvarState = VARIANT.Empty;
            return _childHandler?.get_accRole(varChild, pvarState) ?? HRESULT.S_FALSE;
        }

        *pvarState = (VARIANT)(int)State;
        return HRESULT.S_OK;
    }

    /// <summary>
    ///  Returns the <see href="https://learn.microsoft.com/windows/win32/winauto/object-state-constants">state flags</see>
    ///  for the object.
    /// </summary>
    public virtual ObjectState State => default;

    HRESULT IAccessible.Interface.get_accHelp(VARIANT varChild, BSTR* pszHelp)
    {
        if (pszHelp is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pszHelp = default;
            return _childHandler?.get_accHelp(varChild, pszHelp) ?? HRESULT.S_FALSE;
        }

        if (Help is not { } help)
        {
            *pszHelp = default;
            return HRESULT.S_FALSE;
        }

        *pszHelp = new(help);
        return HRESULT.S_OK;
    }

    protected virtual string? Help => null;

    HRESULT IAccessible.Interface.get_accHelpTopic(BSTR* pszHelpFile, VARIANT varChild, int* pidTopic)
    {
        // Docs list this method as depreciated and state that it should not be used.
        return HRESULT.DISP_E_MEMBERNOTFOUND;
    }

    HRESULT IAccessible.Interface.get_accKeyboardShortcut(VARIANT varChild, BSTR* pszKeyboardShortcut)
    {
        if (pszKeyboardShortcut is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pszKeyboardShortcut = default;
            return _childHandler?.get_accKeyboardShortcut(varChild, pszKeyboardShortcut) ?? HRESULT.S_FALSE;
        }

        if (KeyboardShortcut is not { } shortcut)
        {
            *pszKeyboardShortcut = default;
            return HRESULT.S_FALSE;
        }

        *pszKeyboardShortcut = new(shortcut);
        return HRESULT.S_OK;
    }

    protected virtual string? KeyboardShortcut => null;

    HRESULT IAccessible.Interface.get_accDefaultAction(VARIANT varChild, BSTR* pszDefaultAction)
    {
        if (pszDefaultAction is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pszDefaultAction = default;
            return _childHandler?.get_accDefaultAction(varChild, pszDefaultAction) ?? HRESULT.S_FALSE;
        }

        if (DefaultAction is not { } action)
        {
            *pszDefaultAction = default;
            return HRESULT.S_FALSE;
        }

        *pszDefaultAction = new(action);
        return HRESULT.S_OK;
    }

    protected virtual string? DefaultAction => null;

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

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            return _childHandler?.accLocation(pxLeft, pyTop, pcxWidth, pcyHeight, varChild) ?? HRESULT.S_FALSE;
        }

        Rectangle bounds = Bounds;
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
    /// <returns>The bounds or <see cref="InvalidBounds"/> if the object is not a visual object.</returns>
    public virtual Rectangle Bounds => InvalidBounds;

    private static bool ValidateNavigationDirection(int direction, int id)
    {
        if (direction <= Interop.NAVDIR_MIN || direction >= Interop.NAVDIR_MAX)
        {
            return false;
        }

        return (uint)direction switch
        {
            Interop.NAVDIR_FIRSTCHILD or Interop.NAVDIR_LASTCHILD => id == Interop.CHILDID_SELF,
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

        if (!Navigate(navDir, (int)varStart, out VARIANT result))
        {
            return HRESULT.S_FALSE;
        }

        *pvarEndUpAt = result;
        return HRESULT.S_OK;
    }

    /// <param name="startFromId"><see cref="Interop.CHILDID_SELF"/> or a child element's id to start navigation from.</param>
    public virtual bool Navigate(int direction, int startFromId, out VARIANT result)
    {
        // IMPORTANT:
        //
        // The prescribed way to return children is with IEnumVARIANT. The recommended API AccessibleChildren()
        // attempts to cast IAccessible to IEnumVARIANT first. Should avoid implementing this if at all possible.

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

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        // For default OBJID_CLIENT implementations this always returns S_FALSE.
        // For default OBJID_WINDOW implementations this requires that the type is a valid OBJID (<=0)
        //  (VT_EMPTY is converted to VT_I4/0) and WM_GETOBJECT is generated to retrieve it.

        *ppdispChild = GetChild((int)varChild);

        return *ppdispChild is null ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    /// <summary>
    ///  Returns the <see cref="IDispatch"/> for the child object if it has one. If the child is a simple element
    ///  this returns <see langword="null"/>.
    /// </summary>
    /// <param name="id"><see cref="Interop.CHILDID_SELF"/> or a child element's id.</param>
    protected virtual IDispatch* GetChild(int id) => null;

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

        // For OBJID_CLIENT the default behavior is to count each child window.
        // For OBJID_WINDOW the child count is always 7 (OBJID_SYSMENU through OBJID_SIZEGRIP).

        *pcountChildren = ChildCount;
        return HRESULT.S_OK;
    }

    public virtual int ChildCount => 0;

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
        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            return _childHandler?.accDoDefaultAction(varChild) ?? HRESULT.S_FALSE;
        }

        return !DoDefaultAction() ? HRESULT.DISP_E_MEMBERNOTFOUND : HRESULT.S_OK;
    }

    /// <summary>
    ///  Do the default action for the object, if it has one.
    /// </summary>
    /// <returns><see langword="true"/> if the object has a default action.</returns>
    public virtual bool DoDefaultAction() => false;

    HRESULT IAccessible.Interface.get_accName(VARIANT varChild, BSTR* pszName)
    {
        if (pszName is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        // OBJID_WINDOW defers to OJBID_CLIENT, which uses WM_GETTEXT as the name.

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pszName = default;
            return _childHandler?.get_accName(varChild, pszName) ?? HRESULT.S_FALSE;
        }

        if (Name is not { } name)
        {
            *pszName = default;
            return HRESULT.S_FALSE;
        }

        *pszName = new(name);
        return HRESULT.S_OK;
    }

    HRESULT IAccessible.Interface.put_accName(VARIANT varChild, BSTR szName)
    {
        // No longer supported and should return E_NOTIMPL per documentation.
        return HRESULT.E_NOTIMPL;
    }

    public virtual string? Name => null;

    HRESULT IAccessible.Interface.get_accValue(VARIANT varChild, BSTR* pszValue)
    {
        if (pszValue is null)
        {
            return HRESULT.E_POINTER;
        }

        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            *pszValue = default;
            return _childHandler?.get_accValue(varChild, pszValue) ?? HRESULT.S_FALSE;
        }

        if (GetValue() is not { } value)
        {
            *pszValue = default;
            return HRESULT.S_FALSE;
        }

        *pszValue = new(value);
        return HRESULT.S_OK;
    }

    HRESULT IAccessible.Interface.put_accValue(VARIANT varChild, BSTR szValue)
    {
        if (!ValidateChild(ref varChild) || szValue.IsNull)
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            return _childHandler?.put_accValue(varChild, szValue) ?? HRESULT.S_FALSE;
        }

        return !SetValue(szValue) ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    /// <summary>
    ///  Returns the value.
    /// </summary>
    /// <returns>The value or <see langword="null"/> if unsupported.</returns>
    protected virtual string? GetValue() => null;

    /// <summary>
    ///  Sets the value.
    /// </summary>
    /// <returns><see langword="true"/> if setting values is supported/successful.</returns>
    protected virtual bool SetValue(BSTR value) => false;

    HRESULT IAccessible.Interface.get_accFocus(VARIANT* pvarChild)
    {
        if (pvarChild is null)
        {
            return HRESULT.E_POINTER;
        }

        // OBJID_WINDOW returns OBJID_CLIENT via WM_GETOBJECT if the active HWND is itself or returns true for IsChild().
        // OBJID_CLIENT returns OBJID_SELF or OBJID_WINDOW through AccessibleObjectFromWindow for child windows.

        *pvarChild = GetFocus();
        return pvarChild->IsEmpty ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    /// <summary>
    ///  Return the focused object, if any. This can be <see cref="Self"/> or an <see cref="IDispatch"/> for the
    ///  relevant <see cref="IAccessible"/>.
    /// </summary>
    protected virtual VARIANT GetFocus() => VARIANT.Empty;

    HRESULT IAccessible.Interface.get_accSelection(VARIANT* pvarChildren)
    {
        if (pvarChildren is null)
        {
            return HRESULT.E_INVALIDARG;
        }

        if (!SupportsSelection)
        {
            // Per documentation
            return HRESULT.DISP_E_MEMBERNOTFOUND;
        }

        // OBJID_WINDOW and OBJID_CLIENT both return S_FALSE for this. This is meant for things like listbox and
        // listview selections.

        *pvarChildren = GetSelection();
        return HRESULT.S_OK;
    }

    HRESULT IAccessible.Interface.accSelect(int flagsSelect, VARIANT varChild)
    {
        if (!ValidateChild(ref varChild))
        {
            return HRESULT.E_INVALIDARG;
        }

        if ((int)varChild != Interop.CHILDID_SELF)
        {
            return _childHandler?.accSelect(flagsSelect, varChild) ?? HRESULT.S_FALSE;
        }

        return !SupportsSelection ? HRESULT.DISP_E_MEMBERNOTFOUND : SetSelection((SelectionFlags)flagsSelect);
    }

    /// <summary>
    ///  Returns <see langword="true"/> if this object supports selection (listbox is one example that does).
    /// </summary>
    protected virtual bool SupportsSelection => false;

    /// <summary>
    ///  For a single selected child returns either the <see langword="int"/> id or <see cref="IDispatch"/> for the
    ///  <see cref="IAccessible"/> object. For multiple selections must return <see cref="IEnumUnknown"/> as
    ///  <see cref="IUnknown"/>. Returns <see cref="VARIANT.Empty"/> for no selection.
    /// </summary>
    protected virtual VARIANT GetSelection() => VARIANT.Empty;

    protected virtual HRESULT SetSelection(SelectionFlags flags) => HRESULT.E_INVALIDARG;

    // Default accessibility objects implement the following publicly documented interfaces:
    //
    //      IAccessible, IEnumVARIANT, IOleWindow, IServiceProvider, IAccIdentity
    //
    // For IEnumVARIANT, Next() returns VT_I4 indicies for index based children (everything in OBJID_WINDOW's case),
    // Skip() returns false if at the end. Clone() duplicates the AccessibleObject.
    //
    // For child Windows of OBJID_CLIENT the OBJID_WINDOW object is returned as VT_DISPATCH via WM_GETOBJECT>
}