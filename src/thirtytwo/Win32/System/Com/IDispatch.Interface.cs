// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Windows.Win32.System.Com.Com;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatch : IVTable<IDispatch, IDispatch.Vtbl>
{
    public static void InitializeVTable(Vtbl* vtable)
    {
        vtable->GetTypeInfoCount_4 = &GetTypeInfoCount;
        vtable->GetTypeInfo_5 = &GetTypeInfo;
        vtable->GetIDsOfNames_6 = &GetIDsOfNames;
        vtable->Invoke_7 = &Invoke;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetTypeInfoCount(IDispatch* @this, uint* pctinfo)
        => UnwrapAndInvoke<IDispatch, Interface>(@this, o => o.GetTypeInfoCount(pctinfo));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetTypeInfo(IDispatch* @this, uint iTInfo, uint lcid, ITypeInfo** ppTInfo)
        => UnwrapAndInvoke<IDispatch, Interface>(@this, o => o.GetTypeInfo(iTInfo, lcid, ppTInfo));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT GetIDsOfNames(IDispatch* @this, Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId)
        => UnwrapAndInvoke<IDispatch, Interface>(@this, o => o.GetIDsOfNames(riid, rgszNames, cNames, lcid, rgDispId));

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static HRESULT Invoke(
        IDispatch* @this,
        int dispIdMember,
        Guid* riid,
        uint lcid,
        DISPATCH_FLAGS dwFlags,
        DISPPARAMS* pDispParams,
        VARIANT* pVarResult,
        EXCEPINFO* pExcepInfo,
        uint* pArgErr)
        => UnwrapAndInvoke<IDispatch, Interface>(
            @this,
            o => o.Invoke(dispIdMember, riid, lcid, dwFlags, pDispParams, pVarResult, pExcepInfo, pArgErr));

    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public unsafe interface Interface
    {
        [PreserveSig]
        HRESULT GetTypeInfoCount(
            uint* pctinfo);

        [PreserveSig]
        HRESULT GetTypeInfo(
            uint iTInfo,
            uint lcid,
            ITypeInfo** ppTInfo);

        [PreserveSig]
        HRESULT GetIDsOfNames(
            Guid* riid,
            PWSTR* rgszNames,
            uint cNames,
            uint lcid,
            int* rgDispId);

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