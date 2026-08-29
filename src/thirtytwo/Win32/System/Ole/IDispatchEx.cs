// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Ole;

/// <summary>
///  <para>
///   Convenience helpers for invoking <see cref="IDispatchEx"/> members as late-bound properties.
///  </para>
/// </summary>
public unsafe partial struct IDispatchEx
{
    /// <summary>
    ///  <para>
    ///   Gets the property value for the specified <paramref name="dispatchId"/>.
    ///  </para>
    /// </summary>
    /// <param name="dispatchId">
    ///  <para>
    ///   DISPID of the property to read.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   The property value returned by
    ///   <see cref="InvokeEx(IDispatchEx*, int, uint, ushort, DISPPARAMS*, VARIANT*, EXCEPINFO*, IServiceProvider*)"/>.
    ///  </para>
    /// </returns>
    /// <exception cref="Exception">
    ///  <para>
    ///   Thrown when the dispatch invocation returns a failing HRESULT.
    ///  </para>
    /// </exception>
    public VARIANT GetPropertyValue(int dispatchId)
    {
        VARIANT result = TryGetPropertyValue(dispatchId, out HRESULT hr);
        hr.ThrowOnFailure();
        return result;
    }

    /// <summary>
    ///  <para>
    ///   Attempts to get the property value for the specified <paramref name="dispatchId"/>.
    ///  </para>
    /// </summary>
    /// <param name="dispatchId">
    ///  <para>
    ///   DISPID of the property to read.
    ///  </para>
    /// </param>
    /// <param name="hr">
    ///  <para>
    ///   Result code returned from
    ///   <see cref="InvokeEx(IDispatchEx*, int, uint, ushort, DISPPARAMS*, VARIANT*, EXCEPINFO*, IServiceProvider*)"/>.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   The value buffer passed to the dispatch call. The value is meaningful when <paramref name="hr"/>
    ///   indicates success.
    ///  </para>
    /// </returns>
    public VARIANT TryGetPropertyValue(int dispatchId, out HRESULT hr)
    {
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        hr = InvokeEx(
            dispatchId,
            PInvoke.GetThreadLocale(),
            (ushort)DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            null,
            null);

        return value;
    }

    /// <summary>
    ///  <para>
    ///   Attempts to set the property value for the specified <paramref name="dispatchId"/>.
    ///  </para>
    /// </summary>
    /// <param name="dispatchId">
    ///  <para>
    ///   DISPID of the property to write.
    ///  </para>
    /// </param>
    /// <param name="value">
    ///  <para>
    ///   Value to assign to the dispatch property.
    ///  </para>
    /// </param>
    /// <returns>
    ///  <para>
    ///   The HRESULT returned by the underlying dispatch invocation.
    ///  </para>
    /// </returns>
    public HRESULT TrySetPropertyValue(
        int dispatchId,
        VARIANT value)
    {
        VARIANT* arg = &value;
        int putDispatchID = PInvoke.DISPID_PROPERTYPUT;

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
            PInvoke.GetThreadLocale(),
            (ushort)DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &dispParams,
            null,
            null);

        return hr;
    }

    /// <summary>
    ///  <para>
    ///   Enumerates all member names and DISPIDs visible through this dispatch interface.
    ///  </para>
    /// </summary>
    /// <returns>
    ///  <para>
    ///   A dictionary that maps member name to DISPID.
    ///  </para>
    /// </returns>
    /// <remarks>
    ///  <para>
    ///   Names returned by <see cref="GetMemberName(int, BSTR*)"/> are copied to managed strings and the source
    ///   <see cref="BSTR"/> values are freed.
    ///  </para>
    /// </remarks>
    public IDictionary<string, int> GetAllDispatchIds()
    {
        Dictionary<string, int> dispatchIds = [];
        int dispid = PInvoke.DISPID_STARTENUM;
        while (GetNextDispID((uint)PInvoke.fdexEnumAll, dispid, out dispid) == HRESULT.S_OK)
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