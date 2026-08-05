// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatchCcw
{
    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface Interface
    {
        [PreserveSig]
        HRESULT GetTypeInfoCount(uint* pctinfo);

        [PreserveSig]
        HRESULT GetTypeInfo(uint iTInfo, uint lcid, ITypeInfo** ppTInfo);

        [PreserveSig]
        HRESULT GetIDsOfNames(Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId);

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