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
/// <param name="dispatch">The backing dispatch implementation to adapt as <see cref="IDispatchEx"/>.</param>
public unsafe class DispatchExAdapter(IDispatchCcw.Interface dispatch) : IDispatchEx.Interface
{
    private readonly IDispatchCcw.Interface _dispatch = dispatch;

    /// <summary>
    ///  Gets a DISPID for a member name.
    /// </summary>
    /// <param name="bstrName">Member name to resolve.</param>
    /// <param name="grfdex">Resolution flags as defined by <see cref="IDispatchEx"/>.</param>
    /// <param name="pid">Receives the resolved DISPID on success.</param>
    /// <returns>
    ///  <see cref="HRESULT.S_OK"/> on success, <see cref="PInvoke.DISP_E_UNKNOWNNAME"/> when not found,
    ///  <see cref="PInvoke.E_NOTIMPL"/> when creation is requested, or an error HRESULT.
    /// </returns>
    public virtual HRESULT GetDispID(BSTR bstrName, uint grfdex, int* pid)
    {
        if (bstrName.IsNull || pid is null)
        {
            return HRESULT.E_POINTER;
        }

        HRESULT hr = _dispatch.GetIDsOfNames(IID.Empty(), (PWSTR*)bstrName.Value, 1, 0, pid);

        if (hr == PInvoke.DISP_E_UNKNOWNNAME && (grfdex & PInvoke.fdexNameEnsure) != 0)
        {
            // Can't create a new member.
            return PInvoke.E_NOTIMPL;
        }

        return hr;
    }

    /// <summary>
    ///  Invokes a member by DISPID.
    /// </summary>
    /// <param name="id">DISPID to invoke.</param>
    /// <param name="lcid">Locale identifier for argument conversion.</param>
    /// <param name="wFlags">Dispatch flags.</param>
    /// <param name="pdp">Pointer to invocation arguments.</param>
    /// <param name="pvarRes">Receives the method result variant.</param>
    /// <param name="pei">Receives exception information from dispatch.</param>
    /// <param name="pspCaller">Optional caller service provider pointer.</param>
    /// <returns>The HRESULT from the underlying <see cref="IDispatch"/> invocation.</returns>
    /// <remarks>
    ///  <para>
    ///   Pointer arguments are borrowed for the duration of the call and are not retained.
    ///  </para>
    /// </remarks>
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

    /// <summary>
    ///  Gets property capabilities for a member.
    /// </summary>
    /// <param name="id">DISPID to inspect.</param>
    /// <param name="grfdexFetch">Capability mask to fetch.</param>
    /// <param name="pgrfdex">Receives filtered capability flags.</param>
    /// <returns>
    ///  <see cref="HRESULT.S_OK"/> on success,
    ///  <see cref="PInvoke.DISP_E_UNKNOWNNAME"/> when the member is not found, or an error HRESULT.
    /// </returns>
    public virtual HRESULT GetMemberProperties(int id, uint grfdexFetch, FDEX_PROP_FLAGS* pgrfdex)
    {
        if (pgrfdex is null)
        {
            return HRESULT.E_POINTER;
        }

        if (id == PInvoke.DISPID_UNKNOWN)
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
        Debug.Assert(typeAttr->memidConstructor == PInvoke.MEMBERID_NIL);
        Debug.Assert(typeAttr->memidDestructor == PInvoke.MEMBERID_NIL);

        HRESULT result = PInvoke.DISP_E_UNKNOWNNAME;

        FUNCDESC* funcdesc;
        for (uint i = 0; result == PInvoke.DISP_E_UNKNOWNNAME && i < functionCount; i++)
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

    /// <summary>
    ///  Gets the member name for a DISPID.
    /// </summary>
    /// <param name="id">DISPID to resolve.</param>
    /// <param name="pbstrName">Receives the allocated name string.</param>
    /// <returns>The HRESULT from <see cref="ITypeInfo.GetNames(int, BSTR*, uint, uint*)"/>.</returns>
    /// <remarks>
    ///  <para>
    ///   On success, the caller owns the returned <see cref="BSTR"/> and is responsible for releasing it.
    ///  </para>
    /// </remarks>
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

        uint count;
        hr = typeInfo.Pointer->GetNames(id, pbstrName, 1, &count);
        return hr;
    }

    /// <summary>
    ///  Enumerates the next available DISPID.
    /// </summary>
    /// <param name="grfdex">Enumeration flags.</param>
    /// <param name="id">Current DISPID or <see cref="PInvoke.DISPID_STARTENUM"/> to begin.</param>
    /// <param name="pid">Receives the next DISPID when found.</param>
    /// <returns>
    ///  <see cref="HRESULT.S_OK"/> when a value is returned, <see cref="PInvoke.S_FALSE"/> at end of sequence,
    ///  or an error HRESULT.
    /// </returns>
    public virtual HRESULT GetNextDispID(uint grfdex, int id, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = PInvoke.DISPID_UNKNOWN;

        if ((grfdex & ~(PInvoke.fdexEnumDefault | PInvoke.fdexEnumAll)) != 0)
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
        Debug.Assert(typeAttr->memidConstructor == PInvoke.MEMBERID_NIL);
        Debug.Assert(typeAttr->memidDestructor == PInvoke.MEMBERID_NIL);

        bool next = id == PInvoke.DISPID_STARTENUM;

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

        return PInvoke.S_FALSE;
    }

    /// <summary>
    ///  Deletes a member by name.
    /// </summary>
    /// <param name="bstrName">Member name.</param>
    /// <param name="grfdex">Deletion flags.</param>
    /// <returns><see cref="PInvoke.E_NOTIMPL"/>.</returns>
    public virtual HRESULT DeleteMemberByName(BSTR bstrName, uint grfdex) => PInvoke.E_NOTIMPL;

    /// <summary>
    ///  Deletes a member by DISPID.
    /// </summary>
    /// <param name="id">DISPID to delete.</param>
    /// <returns><see cref="PInvoke.E_NOTIMPL"/>.</returns>
    public virtual HRESULT DeleteMemberByDispID(int id) => PInvoke.E_NOTIMPL;

    /// <summary>
    ///  Gets the namespace parent object.
    /// </summary>
    /// <param name="ppunk">Receives the parent object pointer.</param>
    /// <returns><see cref="PInvoke.E_NOTIMPL"/> when the operation is not supported.</returns>
    public virtual HRESULT GetNameSpaceParent(IUnknown** ppunk)
    {
        if (ppunk is null)
        {
            return HRESULT.E_POINTER;
        }

        *ppunk = null;
        return PInvoke.E_NOTIMPL;
    }

    /// <summary>
    ///  Converts <see cref="FUNCDESC"/> invocation metadata into <see cref="FDEX_PROP_FLAGS"/> values.
    /// </summary>
    /// <param name="funcdesc">Borrowed function description pointer.</param>
    /// <returns>Computed dispatch extension capability flags.</returns>
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