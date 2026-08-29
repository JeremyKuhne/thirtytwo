// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatchCcw
{
    /// <summary>
    ///  Managed interface implemented by objects exposed through the <see cref="IDispatchCcw"/> COM callable wrapper.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Members use unmanaged pointer parameters to preserve native COM <c>IDispatch</c> ABI behavior.
    ///  </para>
    /// </remarks>
    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface Interface
    {
        /// <summary>
        ///  Retrieves the number of type information interfaces provided by the object.
        /// </summary>
        /// <param name="pctinfo">Receives the type information count.</param>
        /// <returns>An <c>HRESULT</c> indicating success or failure.</returns>
        [PreserveSig]
        HRESULT GetTypeInfoCount(uint* pctinfo);

        /// <summary>
        ///  Retrieves type information for an object.
        /// </summary>
        /// <param name="iTInfo">The type information index.</param>
        /// <param name="lcid">The locale identifier.</param>
        /// <param name="ppTInfo">Receives the type information pointer.</param>
        /// <returns>An <c>HRESULT</c> indicating success or failure.</returns>
        [PreserveSig]
        HRESULT GetTypeInfo(uint iTInfo, uint lcid, ITypeInfo** ppTInfo);

        /// <summary>
        ///  Maps member names to dispatch identifiers.
        /// </summary>
        /// <param name="riid">Reserved and expected to be <c>IID_NULL</c> by callers.</param>
        /// <param name="rgszNames">Array of member names.</param>
        /// <param name="cNames">Count of names in <paramref name="rgszNames"/>.</param>
        /// <param name="lcid">The locale identifier.</param>
        /// <param name="rgDispId">Receives dispatch identifiers for the provided names.</param>
        /// <returns>An <c>HRESULT</c> indicating success or failure.</returns>
        [PreserveSig]
        HRESULT GetIDsOfNames(Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId);

        /// <summary>
        ///  Invokes a property or method by dispatch identifier.
        /// </summary>
        /// <param name="dispIdMember">The member dispatch identifier.</param>
        /// <param name="riid">Reserved and expected to be <c>IID_NULL</c> by callers.</param>
        /// <param name="lcid">The locale identifier.</param>
        /// <param name="wFlags">Invoke flags describing how to invoke the member.</param>
        /// <param name="pDispParams">Pointer to invoke arguments.</param>
        /// <param name="pVarResult">Receives the result variant when requested by the caller.</param>
        /// <param name="pExcepInfo">Receives exception information for invocation failures.</param>
        /// <param name="pArgErr">Receives the argument index when type conversion fails.</param>
        /// <returns>An <c>HRESULT</c> indicating success or failure.</returns>
        [PreserveSig]
        HRESULT Invoke(
            int dispIdMember,
            Guid* riid,
            uint lcid,
            DISPATCH_FLAGS wFlags,
            DISPPARAMS* pDispParams,
            VARIANT* pVarResult,
            EXCEPINFO* pExcepInfo,
            uint* pArgErr);
    }
}