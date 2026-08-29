// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Extension helpers for invoking <see cref="IDispatch"/> members.
/// </summary>
public static unsafe class IDispatchExtensions
{
    /// <summary>
    ///  Provides extension helpers for a dispatch interface reference.
    /// </summary>
    /// <param name="dispatch">The dispatch interface reference that extension members operate on.</param>
    extension(ref IDispatch dispatch)
    {
        /// <summary>
        ///  Resolves dispatch identifiers for the provided member names.
        /// </summary>
        /// <param name="names">The member names to resolve.</param>
        /// <returns>An array of dispatch identifiers aligned with <paramref name="names"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="names"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        ///  Resolves a dispatch identifier for a single member name.
        /// </summary>
        /// <param name="name">The member name to resolve.</param>
        /// <returns>
        ///  The resolved dispatch identifier, or <see cref="PInvoke.DISPID_UNKNOWN"/> when the name is unknown.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        ///  Reads a property value by member name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The property value, or <see langword="default"/> when the name cannot be resolved.</returns>
        public VARIANT GetPropertyValue(string name)
        {
            int dispid = dispatch.GetIdOfName(name);
            if (dispid == PInvoke.DISPID_UNKNOWN)
            {
                return default;
            }

            return dispatch.GetPropertyValue(dispid);
        }

        /// <summary>
        ///  Reads a property value by dispatch identifier.
        /// </summary>
        /// <param name="dispatchId">The property dispatch identifier.</param>
        /// <returns>The value returned by <c>IDispatch::Invoke</c>.</returns>
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

        /// <summary>
        ///  Sets a property value by dispatch identifier.
        /// </summary>
        /// <param name="dispatchId">The property dispatch identifier.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>The <c>HRESULT</c> returned by <c>IDispatch::Invoke</c>.</returns>
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