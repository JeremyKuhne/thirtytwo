// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace Windows.Accessibility;

public unsafe abstract class UiaBase :
    AccessibleBase,
    IRawElementProviderSimple.Interface,
    IManagedWrapper<IRawElementProviderSimple, IAccessible, IDispatch>
{
    public UiaBase() : base()
    {
    }

    ProviderOptions IRawElementProviderSimple.Interface.ProviderOptions
        => ProviderOptions.ProviderOptions_ServerSideProvider;

    HRESULT IRawElementProviderSimple.Interface.GetPatternProvider(UIA_PATTERN_ID patternId, IUnknown** pRetVal)
    {
        return HRESULT.E_NOTIMPL;
    }

    protected virtual bool IsPatternSupported(UIA_PATTERN_ID patternId)
    {
        if (patternId == UIA_PATTERN_ID.UIA_InvokePatternId)
        {
            switch (Role)
            {
                case AccessibleRole.MenuItem:
                case AccessibleRole.Link:
                case AccessibleRole.PushButton:
                case AccessibleRole.ButtonDropDown:
                case AccessibleRole.ButtonMenu:
                case AccessibleRole.ButtonDropDownGrid:
                case AccessibleRole.Clock:
                case AccessibleRole.SplitButton:
                case AccessibleRole.CheckButton:
                case AccessibleRole.Cell:
                case AccessibleRole.ListItem:
                    return true;

                case AccessibleRole.Sound:
                case AccessibleRole.Cursor:
                case AccessibleRole.Caret:
                case AccessibleRole.Alert:
                case AccessibleRole.Client:
                case AccessibleRole.Chart:
                case AccessibleRole.Dialog:
                case AccessibleRole.Border:
                case AccessibleRole.Column:
                case AccessibleRole.Row:
                case AccessibleRole.HelpBalloon:
                case AccessibleRole.Character:
                case AccessibleRole.PageTab:
                case AccessibleRole.PropertyPage:
                case AccessibleRole.DropList:
                case AccessibleRole.Dial:
                case AccessibleRole.HotkeyField:
                case AccessibleRole.Diagram:
                case AccessibleRole.Animation:
                case AccessibleRole.Equation:
                case AccessibleRole.WhiteSpace:
                case AccessibleRole.IPAddress:
                case AccessibleRole.OutlineButton:
                    return false;

                default:
                    return !string.IsNullOrEmpty(DefaultAction);
            }
        }

        return false;
    }

    HRESULT IRawElementProviderSimple.Interface.GetPropertyValue(UIA_PROPERTY_ID propertyId, VARIANT* pRetVal)
    {
        if (pRetVal is null)
        {
            return HRESULT.E_POINTER;
        }

        *pRetVal = GetProperty(propertyId);
        return HRESULT.S_OK;
    }

    protected static VARIANT ToBSTROrEmpty(string? value) => value is null ? VARIANT.Empty : (VARIANT)new BSTR(value);

    protected virtual VARIANT GetProperty(UIA_PROPERTY_ID propertyId)
    {
        return propertyId switch
        {
            UIA_PROPERTY_ID.UIA_AccessKeyPropertyId => ToBSTROrEmpty(KeyboardShortcut),
            UIA_PROPERTY_ID.UIA_BoundingRectanglePropertyId => ((UiaRect)Bounds).ToVARIANT(),
            _ => VARIANT.Empty
        };
    }

    IRawElementProviderSimple* IRawElementProviderSimple.Interface.HostRawElementProvider => null;
}