// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatchCcw : IComIID, IVTable<IDispatchCcw, IDispatchCcw.Vtbl>
{
    private static readonly Guid s_iid = IDispatch.IID_Guid;

    static ref readonly Guid IComIID.Guid => ref s_iid;

    public static void PopulateVTable(Vtbl* vtable)
    {
        vtable->GetTypeInfoCount_4 = &GetTypeInfoCount;
        vtable->GetTypeInfo_5 = &GetTypeInfo;
        vtable->GetIDsOfNames_6 = &GetIDsOfNames;
        vtable->Invoke_7 = &Invoke;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetTypeInfoCount(IDispatch* @this, uint* pctinfo)
        => ComExtensions.UnwrapAndInvoke<IDispatch, Interface>(@this, o => o.GetTypeInfoCount(pctinfo));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetTypeInfo(IDispatch* @this, uint iTInfo, uint lcid, ITypeInfo** ppTInfo)
        => ComExtensions.UnwrapAndInvoke<IDispatch, Interface>(@this, o => o.GetTypeInfo(iTInfo, lcid, ppTInfo));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetIDsOfNames(IDispatch* @this, Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId)
        => ComExtensions.UnwrapAndInvoke<IDispatch, Interface>(
            @this,
            o => o.GetIDsOfNames(riid, rgszNames, cNames, lcid, rgDispId));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
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
        => ComExtensions.UnwrapAndInvoke<IDispatch, Interface>(
            @this,
            o => o.Invoke(dispIdMember, riid, lcid, dwFlags, pDispParams, pVarResult, pExcepInfo, pArgErr));

}
