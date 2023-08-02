// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;
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
                case ObjectRoles.MenuItem:
                case ObjectRoles.Link:
                case ObjectRoles.PushButton:
                case ObjectRoles.ButtonDropDown:
                case ObjectRoles.ButtonMenu:
                case ObjectRoles.ButtonDropDownGrid:
                case ObjectRoles.Clock:
                case ObjectRoles.SplitButton:
                case ObjectRoles.CheckButton:
                case ObjectRoles.Cell:
                case ObjectRoles.ListItem:
                    return true;

                case ObjectRoles.Sound:
                case ObjectRoles.Cursor:
                case ObjectRoles.Caret:
                case ObjectRoles.Alert:
                case ObjectRoles.Client:
                case ObjectRoles.Chart:
                case ObjectRoles.Dialog:
                case ObjectRoles.Border:
                case ObjectRoles.Column:
                case ObjectRoles.Row:
                case ObjectRoles.HelpBalloon:
                case ObjectRoles.Character:
                case ObjectRoles.PageTab:
                case ObjectRoles.PropertyPage:
                case ObjectRoles.DropList:
                case ObjectRoles.Dial:
                case ObjectRoles.HotkeyField:
                case ObjectRoles.Diagram:
                case ObjectRoles.Animation:
                case ObjectRoles.Equation:
                case ObjectRoles.WhiteSpace:
                case ObjectRoles.IPAddress:
                case ObjectRoles.OutlineButton:
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