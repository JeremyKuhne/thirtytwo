// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.UI.Accessibility;

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
            _ => VARIANT.Empty
        };
    }

    IRawElementProviderSimple* IRawElementProviderSimple.Interface.HostRawElementProvider => null;
}