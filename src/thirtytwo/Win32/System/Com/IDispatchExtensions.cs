// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public static unsafe class IDispatchExtensions
{
    extension(ref IDispatch dispatch)
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
                HRESULT hr = dispatch.GetIDsOfNames(IID.Empty(), (PWSTR*)(char**)namesArg, (uint)names.Length, lcid: 0, i);
                if (hr.Failed && hr != PInvoke.DISP_E_UNKNOWNNAME)
                {
                    hr.ThrowOnFailure();
                }
            }

            return ids;
        }

        public int GetIdOfName(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            int id = PInvoke.DISPID_UNKNOWN;
            fixed (char* n = name)
            {
                PWSTR* p = (PWSTR*)n;
                HRESULT hr = dispatch.GetIDsOfNames(IID.Empty(), (PWSTR*)&p, 1, lcid: 0, &id);
                if (hr.Failed && hr != PInvoke.DISP_E_UNKNOWNNAME)
                {
                    hr.ThrowOnFailure();
                }
            }

            return id;
        }

        public VARIANT GetPropertyValue(string name)
        {
            int dispid = dispatch.GetIdOfName(name);
            if (dispid == PInvoke.DISPID_UNKNOWN)
            {
                return default;
            }

            return dispatch.GetPropertyValue(dispid);
        }

        public VARIANT GetPropertyValue(int dispatchId)
        {
            Guid guid = Guid.Empty;
            EXCEPINFO exceptionInfo = default;
            DISPPARAMS parameters = default;
            VARIANT value = default;

            dispatch.Invoke(
                dispatchId,
                &guid,
                PInvoke.GetThreadLocale(),
                DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
                &parameters,
                &value,
                &exceptionInfo,
                null);

            return value;
        }

        public HRESULT SetPropertyValue(int dispatchId, VARIANT value)
        {
            Guid guid = Guid.Empty;
            EXCEPINFO exceptionInfo = default;
            VARIANT* argument = &value;
            int putDispatchId = PInvoke.DISPID_PROPERTYPUT;

            DISPPARAMS parameters = new()
            {
                cArgs = 1,
                cNamedArgs = 1,
                // You HAVE to name the put argument or you'll get DISP_E_PARAMNOTFOUND
                rgdispidNamedArgs = &putDispatchId,
                rgvarg = argument
            };

            uint argumentError;

            return dispatch.Invoke(
                dispatchId,
                &guid,
                PInvoke.GetThreadLocale(),
                DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
                &parameters,
                null,
                &exceptionInfo,
                &argumentError);
        }
    }
}