// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatch
{
    public int[] GetIdsOfNames(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        if (names.Length == 0)
        {
            return [];
        }

        using StringParameterArray namesArg = new(names);
        int[] ids = new int[names.Length];
        fixed (int* i = ids)
        {
            HRESULT hr = GetIDsOfNames(IID.Empty(), (PWSTR*)(char**)namesArg, (uint)names.Length, lcid: 0, i);
            if (hr.Failed && hr != HRESULT.DISP_E_UNKNOWNNAME)
            {
                hr.ThrowOnFailure();
            }
        }

        return ids;
    }

    public int GetIdOfName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        int id = Interop.DISPID_UNKNOWN;
        fixed (char* n = name)
        {
            PWSTR* p = (PWSTR*)n;
            HRESULT hr = GetIDsOfNames(IID.Empty(), (PWSTR*)&p, 1, lcid: 0, &id);
            if (hr.Failed && hr != HRESULT.DISP_E_UNKNOWNNAME)
            {
                hr.ThrowOnFailure();
            }
        }

        return id;
    }

    public VARIANT GetPropertyValue(string name)
    {
        int dispid = GetIdOfName(name);
        if (dispid == Interop.DISPID_UNKNOWN)
        {
            return default;
        }

        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        HRESULT hr = Invoke(
            dispid,
            &guid,
            Interop.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            &pExcepInfo,
            null);

        Debug.Assert(hr.Succeeded);

        return value;
    }

    public VARIANT GetPropertyValue(int dispatchId)
    {
        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        Invoke(
            dispatchId,
            &guid,
            Interop.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            &pExcepInfo,
            null);

        return value;
    }

    public HRESULT SetPropertyValue(int dispatchId, VARIANT value)
    {
        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
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

        uint argumentError;

        HRESULT hr = Invoke(
            dispatchId,
            &guid,
            Interop.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &dispParams,
            null,
            &pExcepInfo,
            &argumentError);

        return hr;
    }
}