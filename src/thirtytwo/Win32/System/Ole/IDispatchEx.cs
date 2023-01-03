// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.System.Ole;

public unsafe partial struct IDispatchEx : IVTable<IDispatchEx, IDispatchEx.Vtbl>
{
    public VARIANT GetPropertyValue(int dispatchId)
    {
        VARIANT result = TryGetPropertyValue(dispatchId, out HRESULT hr);
        hr.ThrowOnFailure();
        return result;
    }

    public VARIANT TryGetPropertyValue(int dispatchId, out HRESULT hr)
    {
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        hr = InvokeEx(
            dispatchId,
            Interop.GetThreadLocale(),
            (ushort)DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            null,
            null);

        return value;
    }

    public HRESULT TrySetPropertyValue(
        int dispatchId,
        VARIANT value)
    {
        VARIANT* arg = &value;
        int putDispatchID = Interop.DISPID_PROPERTYPUT;

        DISPPARAMS dispParams = new()
        {
            cArgs = 1,
            cNamedArgs = 1,
            // You HAVE to name the put argument or you'll get DISP_E_PARAMNOTFOUND
            rgdispidNamedArgs = &putDispatchID,
            rgvarg = arg
        };

        HRESULT hr = InvokeEx(
            dispatchId,
            Interop.GetThreadLocale(),
            (ushort)DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &dispParams,
            null,
            null);

        return hr;
    }

    public IDictionary<string, int> GetAllDispatchIds()
    {
        Dictionary<string, int> dispatchIds = new();
        int dispid = Interop.DISPID_STARTENUM;
        while (GetNextDispID(Interop.fdexEnumAll, dispid, &dispid) == HRESULT.S_OK)
        {
            BSTR name = default;
            HRESULT hr = GetMemberName(dispid, &name);
            if (hr.Succeeded)
            {
                dispatchIds.Add(name.ToStringAndFree(), dispid);
            }
            else
            {
                Debug.Fail($"Failed to get member name: {hr.ToStringWithDescription()}");
            }
        }

        return dispatchIds;
    }
}