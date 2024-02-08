// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Provides an <see cref="IDispatchEx"/> view of <see cref="IDispatch"/>.
/// </summary>
/// <devdoc>
///  <para>
///   This is only how I would expect things to work just by looking at the interfaces. Haven't seen documentation
///   yet that validates that everything in <see cref="IDispatch"/> should be available in <see cref="IDispatchEx"/>.
///  </para>
/// </devdoc>
public unsafe class DispatchExAdapter(IDispatch.Interface dispatch) : IDispatchEx.Interface
{
    private readonly IDispatch.Interface _dispatch = dispatch;

    public virtual HRESULT GetDispID(BSTR bstrName, uint grfdex, int* pid)
    {
        if (bstrName.IsNull || pid is null)
        {
            return HRESULT.E_POINTER;
        }

        HRESULT hr = _dispatch.GetIDsOfNames(IID.Empty(), (PWSTR*)bstrName.Value, 1, 0, pid);

        if (hr == HRESULT.DISP_E_UNKNOWNNAME && (grfdex & Interop.fdexNameEnsure) != 0)
        {
            // Can't create a new member.
            return HRESULT.E_NOTIMPL;
        }

        return hr;
    }

    public virtual HRESULT InvokeEx(
        int id,
        uint lcid,
        ushort wFlags,
        DISPPARAMS* pdp,
        VARIANT* pvarRes,
        EXCEPINFO* pei,
        IServiceProvider* pspCaller)
    {
        if (pdp is null || pvarRes is null)
        {
            return HRESULT.E_POINTER;
        }

        return _dispatch.Invoke(id, IID.Empty(), lcid, (DISPATCH_FLAGS)wFlags, pdp, pvarRes, pei, null);
    }

    public virtual HRESULT GetMemberProperties(int id, uint grfdexFetch, FDEX_PROP_FLAGS* pgrfdex)
    {
        if (pgrfdex is null)
        {
            return HRESULT.E_POINTER;
        }

        if (id == Interop.DISPID_UNKNOWN)
        {
            return HRESULT.E_INVALIDARG;
        }

        *pgrfdex = default;

        using ComScope<ITypeInfo> typeInfo = new(null);
        HRESULT hr = _dispatch.GetTypeInfo(0, 0, typeInfo);
        if (hr.Failed)
        {
            return hr;
        }

        hr = typeInfo.Pointer->GetTypeAttr(out TYPEATTR* typeAttr);
        if (hr.Failed)
        {
            return hr;
        }

        ushort functionCount = typeAttr->cFuncs;

        // Presuming these won't come up for now.
        Debug.Assert(typeAttr->cVars == 0);
        Debug.Assert(typeAttr->memidConstructor == Interop.MEMBERID_NIL);
        Debug.Assert(typeAttr->memidDestructor == Interop.MEMBERID_NIL);

        HRESULT result = HRESULT.DISP_E_UNKNOWNNAME;

        FUNCDESC* funcdesc;
        for (uint i = 0; result == HRESULT.DISP_E_UNKNOWNNAME && i < functionCount; i++)
        {
            hr = typeInfo.Pointer->GetFuncDesc(i, &funcdesc);
            if (hr.Failed)
            {
                return hr;
            }

            if (funcdesc->memid == id)
            {
                // Found the specified DISPID
                *pgrfdex = GetFuncDescProperties(funcdesc) & (FDEX_PROP_FLAGS)grfdexFetch;
                result = HRESULT.S_OK;
            }

            typeInfo.Pointer->ReleaseFuncDesc(funcdesc);
        }

        return result;
    }

    public virtual HRESULT GetMemberName(int id, BSTR* pbstrName)
    {
        if (pbstrName is null)
        {
            return HRESULT.E_POINTER;
        }

        using ComScope<ITypeInfo> typeInfo = new(null);
        HRESULT hr = _dispatch.GetTypeInfo(0, 0, typeInfo);
        if (hr.Failed)
        {
            return hr;
        }

        hr = typeInfo.Pointer->GetNames(id, pbstrName, 1, out uint count);
        return hr;
    }

    public virtual HRESULT GetNextDispID(uint grfdex, int id, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = Interop.DISPID_UNKNOWN;

        if ((grfdex & ~(Interop.fdexEnumDefault | Interop.fdexEnumAll)) != 0)
        {
            // fdexEnumDefault and fdexEnumAll are the only valid options
            return HRESULT.E_INVALIDARG;
        }

        using ComScope<ITypeInfo> typeInfo = new(null);
        HRESULT hr = _dispatch.GetTypeInfo(0, 0, typeInfo);
        if (hr.Failed)
        {
            return hr;
        }

        hr = typeInfo.Pointer->GetTypeAttr(out TYPEATTR* typeAttr);
        if (hr.Failed)
        {
            return hr;
        }

        ushort functionCount = typeAttr->cFuncs;

        // Presuming these won't come up for now.
        Debug.Assert(typeAttr->cVars == 0);
        Debug.Assert(typeAttr->memidConstructor == Interop.MEMBERID_NIL);
        Debug.Assert(typeAttr->memidDestructor == Interop.MEMBERID_NIL);

        bool next = id == Interop.DISPID_STARTENUM;

        FUNCDESC* funcdesc;
        for (uint i = 0; i < functionCount; i++)
        {
            hr = typeInfo.Pointer->GetFuncDesc(i, &funcdesc);
            if (hr.Failed)
            {
                return hr;
            }

            int currentId = funcdesc->memid;

            typeInfo.Pointer->ReleaseFuncDesc(funcdesc);

            if (next)
            {
                *pid = currentId;
                return HRESULT.S_OK;
            }

            next = id == currentId;
        }

        return HRESULT.S_FALSE;
    }

    public virtual HRESULT DeleteMemberByName(BSTR bstrName, uint grfdex) => HRESULT.E_NOTIMPL;
    public virtual HRESULT DeleteMemberByDispID(int id) => HRESULT.E_NOTIMPL;

    public virtual HRESULT GetNameSpaceParent(IUnknown** ppunk)
    {
        if (ppunk is null)
        {
            return HRESULT.E_POINTER;
        }

        *ppunk = null;
        return HRESULT.E_NOTIMPL;
    }

    private static FDEX_PROP_FLAGS GetFuncDescProperties(FUNCDESC* funcdesc)
    {
        FDEX_PROP_FLAGS flags = default;

        INVOKEKIND invokekind = funcdesc->invkind;
        flags |= invokekind.HasFlag(INVOKEKIND.INVOKE_PROPERTYPUT)
            ? FDEX_PROP_FLAGS.fdexPropCanPut
            : FDEX_PROP_FLAGS.fdexPropCannotPut;

        flags |= invokekind.HasFlag(INVOKEKIND.INVOKE_PROPERTYPUTREF)
            ? FDEX_PROP_FLAGS.fdexPropCanPutRef
            : FDEX_PROP_FLAGS.fdexPropCannotPutRef;

        flags |= invokekind.HasFlag(INVOKEKIND.INVOKE_PROPERTYGET)
            ? FDEX_PROP_FLAGS.fdexPropCanGet
            : FDEX_PROP_FLAGS.fdexPropCannotGet;

        flags |= invokekind.HasFlag(INVOKEKIND.INVOKE_FUNC)
            ? FDEX_PROP_FLAGS.fdexPropCanCall
            : FDEX_PROP_FLAGS.fdexPropCannotCall;

        flags |= FDEX_PROP_FLAGS.fdexPropCannotConstruct | FDEX_PROP_FLAGS.fdexPropCannotSourceEvents;

        return flags;
    }
}