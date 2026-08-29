// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class for providing <see cref="IDispatch"/> services around an existing <see cref="ITypeInfo"/>.
/// </summary>
public unsafe abstract class StandardDispatch : DisposableBase, IDispatchCcw.Interface, IDispatchEx.Interface
{
    private ITypeInfo* _typeInfo;

    /// <summary>
    ///  Construct a new instance with the specified backing <see cref="ITypeInfo"/>.
    /// </summary>
    /// <param name="typeInfo">The backing type information pointer.</param>
    /// <param name="interfaceGuid">The expected interface identifier for debug validation.</param>
    public StandardDispatch(ITypeInfo* typeInfo, Guid interfaceGuid)
    {
        if (typeInfo is null)
        {
            throw new ArgumentNullException(nameof(typeInfo));
        }

#if DEBUG
        typeInfo->GetTypeAttr(out TYPEATTR* typeAttributes).ThrowOnFailure();
        try
        {
            if (typeAttributes->guid != interfaceGuid)
            {
                throw new ArgumentException("Interface guid doesn't match type info", nameof(typeInfo));
            }
        }
        finally
        {
            typeInfo->ReleaseTypeAttr(typeAttributes);
        }
#endif

        _typeInfo = typeInfo;
        _typeInfo->AddRef();
    }

    /// <inheritdoc cref="IDispatchCcw.Interface.GetTypeInfoCount(uint*)"/>
    HRESULT IDispatchCcw.Interface.GetTypeInfoCount(uint* pctinfo)
    {
        if (pctinfo is null)
        {
            return HRESULT.E_POINTER;
        }

        *pctinfo = 1;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IDispatchCcw.Interface.GetTypeInfo(uint, uint, ITypeInfo**)"/>
    HRESULT IDispatchCcw.Interface.GetTypeInfo(uint iTInfo, uint lcid, ITypeInfo** ppTInfo)
    {
        if (ppTInfo is null)
        {
            return HRESULT.E_POINTER;
        }

        if (iTInfo != 0)
        {
            *ppTInfo = null;
            return PInvoke.DISP_E_BADINDEX;
        }

        _typeInfo->AddRef();
        *ppTInfo = _typeInfo;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IDispatchCcw.Interface.GetIDsOfNames(Guid*, PWSTR*, uint, uint, int*)"/>
    HRESULT IDispatchCcw.Interface.GetIDsOfNames(Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId)
    {
        // This must bee IID_NULL
        if (riid != IID.Empty())
        {
            return PInvoke.DISP_E_UNKNOWNINTERFACE;
        }

        return _typeInfo->GetIDsOfNames(rgszNames, cNames, rgDispId);
    }

    /// <inheritdoc cref="IDispatchCcw.Interface.Invoke(int, Guid*, uint, DISPATCH_FLAGS, DISPPARAMS*, VARIANT*, EXCEPINFO*, uint*)"/>
    HRESULT IDispatchCcw.Interface.Invoke(
        int dispIdMember,
        Guid* riid,
        uint lcid,
        DISPATCH_FLAGS wFlags,
        DISPPARAMS* pDispParams,
        VARIANT* pVarResult,
        EXCEPINFO* pExcepInfo,
        uint* pArgErr)
    {
        // This must bee IID_NULL
        if (riid != IID.Empty())
        {
            return PInvoke.DISP_E_UNKNOWNINTERFACE;
        }

        HRESULT hr = MapDotNetHRESULTs(Invoke(
            dispIdMember,
            lcid,
            wFlags,
            pDispParams,
            pVarResult,
            pExcepInfo,
            serviceProvider: null,
            pArgErr));

        if (hr != PInvoke.DISP_E_MEMBERNOTFOUND)
        {
            return hr;
        }

        // The override couldn't find it, pass it along via the ITypeInfo.
        using ComScope @interface = GetComCallableWrapper();
        return @interface.IsNull
            ? hr
            : _typeInfo->Invoke(@interface, dispIdMember, wFlags, pDispParams, pVarResult, pExcepInfo, pArgErr);
    }

    /// <summary>
    ///  If applicable, the pointer to our interface so the ITypeInfo implementation can call back via the interface.
    /// </summary>
    /// <returns>
    ///  A COM scope containing the callable wrapper pointer, or <see langword="default"/> when unavailable.
    /// </returns>
    protected virtual ComScope GetComCallableWrapper() => default;

    /// <inheritdoc cref="IDispatchEx.Interface.GetDispID(BSTR, uint, int*)"/>
    HRESULT IDispatchEx.Interface.GetDispID(BSTR bstrName, uint grfdex, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = PInvoke.DISPID_UNKNOWN;
        return bstrName.IsNull ? HRESULT.E_POINTER : GetDispID(bstrName, grfdex, pid);
    }

    /// <summary>
    ///  Override to provide a dispatch id for the given name. Return <see cref="Interop.DISP_E_UNKNOWNNAME"/>
    ///  if the name isn't supported.
    /// </summary>
    /// <param name="bstrName">The member name.</param>
    /// <param name="grfdex">Flags controlling name lookup behavior.</param>
    /// <param name="pid">Receives the resolved dispatch identifier.</param>
    /// <returns>The lookup result as an <c>HRESULT</c>.</returns>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/reference/idispatchex-getdispid">
    ///    Official documentation.
    ///   </see>
    ///  </para>
    /// </remarks>
    protected virtual HRESULT GetDispID(BSTR bstrName, uint grfdex, int* pid) => PInvoke.DISP_E_UNKNOWNNAME;

    /// <inheritdoc cref="IDispatchEx.Interface.GetMemberName(int, BSTR*)"/>
    HRESULT IDispatchEx.Interface.GetMemberName(int id, BSTR* pbstrName)
        => pbstrName is null ? HRESULT.E_POINTER : GetMemberName(id, pbstrName);

    /// <summary>
    ///  Override to provide the name for a given dispatch id. Return <see cref="Interop.DISP_E_UNKNOWNNAME"/> if the
    ///  name isn't known.
    /// </summary>
    /// <param name="id">The dispatch identifier.</param>
    /// <param name="pbstrName">Receives the member name.</param>
    /// <returns>The lookup result as an <c>HRESULT</c>.</returns>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/reference/idispatchex-getmembername">
    ///    Official documentation.
    ///   </see>
    ///  </para>
    /// </remarks>
    protected virtual HRESULT GetMemberName(int id, BSTR* pbstrName) => PInvoke.DISP_E_UNKNOWNNAME;

    /// <inheritdoc cref="IDispatchEx.Interface.GetNextDispID(uint, int, int*)"/>
    HRESULT IDispatchEx.Interface.GetNextDispID(uint grfdex, int id, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = PInvoke.DISPID_UNKNOWN;

        return GetNextDispID(grfdex, id, pid);
    }

    /// <summary>
    ///  Returns the next dispatch identifier for member enumeration.
    /// </summary>
    /// <param name="grfdex">Flags controlling enumeration behavior.</param>
    /// <param name="id">
    ///  <see cref="Interop.DISPID_STARTENUM"/> to start enumeration, or the last id returned by a previous call to
    ///  <see cref="GetNextDispID(uint, int, int*)"/>.
    /// </param>
    /// <param name="pid">The next dispatch id.</param>
    /// <returns>The next dispatch id, or <see cref="Interop.S_FALSE"/> if there are no more.</returns>
    protected virtual HRESULT GetNextDispID(uint grfdex, int id, int* pid) => PInvoke.S_FALSE;

    /// <inheritdoc cref="IDispatchEx.Interface.InvokeEx(int, uint, ushort, DISPPARAMS*, VARIANT*, EXCEPINFO*, IServiceProvider*)"/>
    HRESULT IDispatchEx.Interface.InvokeEx(
        int id,
        uint lcid,
        ushort wFlags,
        DISPPARAMS* pdp,
        VARIANT* pvarRes,
        EXCEPINFO* pei,
        IServiceProvider* pspCaller)
    {
        HRESULT hr = MapDotNetHRESULTs(Invoke(
            id,
            lcid,
            (DISPATCH_FLAGS)wFlags,
            pdp,
            pvarRes,
            pei,
            pspCaller,
            argumentError: null));

        if (hr != PInvoke.DISP_E_MEMBERNOTFOUND)
        {
            return hr;
        }

        // The override couldn't find it, pass our own interface along so it can be dispatche
        using ComScope @interface = GetComCallableWrapper();
        return @interface.IsNull ? hr : _typeInfo->Invoke(@interface, id, (DISPATCH_FLAGS)wFlags, pdp, pvarRes, pei, puArgErr: null);
    }

    /// <summary>
    ///  Overrides dispatch invocation handling for custom members.
    /// </summary>
    /// <param name="dispId">The member dispatch identifier.</param>
    /// <param name="lcid">The locale identifier.</param>
    /// <param name="flags">The dispatch invocation flags.</param>
    /// <param name="parameters">Pointer to invocation parameters.</param>
    /// <param name="result">Receives the invocation result when requested.</param>
    /// <param name="exceptionInfo">Receives invocation exception details.</param>
    /// <param name="serviceProvider">Optional caller service provider.</param>
    /// <param name="argumentError">Receives the argument index for conversion failures.</param>
    /// <returns>
    ///  <see cref="PInvoke.DISP_E_MEMBERNOTFOUND"/> by default, allowing fallback to the backing
    ///  <see cref="ITypeInfo"/>.
    /// </returns>
    protected virtual HRESULT Invoke(
        int dispId,
        uint lcid,
        DISPATCH_FLAGS flags,
        DISPPARAMS* parameters,
        VARIANT* result,
        EXCEPINFO* exceptionInfo,
        IServiceProvider* serviceProvider,
        uint* argumentError)
        => PInvoke.DISP_E_MEMBERNOTFOUND;

    /// <inheritdoc cref="IDispatchEx.Interface.GetMemberProperties(int, uint, FDEX_PROP_FLAGS*)"/>
    HRESULT IDispatchEx.Interface.GetMemberProperties(int id, uint grfdexFetch, FDEX_PROP_FLAGS* pgrfdex)
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

        HRESULT hr = GetMemberProperties(id, out FDEX_PROP_FLAGS properties);
        if (hr.Succeeded)
        {
            // Filter to the requested properties
            *pgrfdex = properties & (FDEX_PROP_FLAGS)grfdexFetch;
        }
        else
        {
            *pgrfdex = default;
        }

        return hr;
    }

    /// <summary>
    ///  Gets property flags for the specified dispatch identifier.
    /// </summary>
    /// <param name="dispId">The dispatch identifier.</param>
    /// <param name="properties">Receives the available property flags.</param>
    /// <returns>An <c>HRESULT</c> indicating whether the member was recognized.</returns>
    protected virtual HRESULT GetMemberProperties(int dispId, out FDEX_PROP_FLAGS properties)
    {
        properties = default;
        return PInvoke.E_NOTIMPL;
    }

    // .NET COM Interop returns E_NOTIMPL for these three.

    /// <inheritdoc cref="IDispatchEx.Interface.DeleteMemberByName(BSTR, uint)"/>
    HRESULT IDispatchEx.Interface.DeleteMemberByName(BSTR bstrName, uint grfdex) => PInvoke.E_NOTIMPL;

    /// <inheritdoc cref="IDispatchEx.Interface.DeleteMemberByDispID(int)"/>
    HRESULT IDispatchEx.Interface.DeleteMemberByDispID(int id) => PInvoke.E_NOTIMPL;

    /// <inheritdoc cref="IDispatchEx.Interface.GetNameSpaceParent(IUnknown**)"/>
    HRESULT IDispatchEx.Interface.GetNameSpaceParent(IUnknown** ppunk)
    {
        if (ppunk is null)
        {
            return HRESULT.E_POINTER;
        }

        *ppunk = null;
        return PInvoke.E_NOTIMPL;
    }

    /// <summary>
    ///  Maps selected .NET exception <c>HRESULT</c> values to dispatch-oriented COM results.
    /// </summary>
    /// <param name="hr">The original result code.</param>
    /// <returns>The mapped or original <c>HRESULT</c>.</returns>
    private static HRESULT MapDotNetHRESULTs(HRESULT hr)
    {
        if (hr == HRESULT.COR_E_OVERFLOW)
        {
            return PInvoke.DISP_E_OVERFLOW;
        }
        else if (hr == HRESULT.COR_E_INVALIDOLEVARIANTTYPE)
        {
            return PInvoke.DISP_E_BADVARTYPE;
        }
        else if (hr == HRESULT.COR_E_ARGUMENT)
        {
            return HRESULT.E_INVALIDARG;
        }
        else if (hr == HRESULT.COR_E_SAFEARRAYTYPEMISMATCH)
        {
            return PInvoke.DISP_E_TYPEMISMATCH;
        }
        else if (hr == HRESULT.COR_E_MISSINGMEMBER || hr == HRESULT.COR_E_MISSINGMETHOD)
        {
            return PInvoke.DISP_E_MEMBERNOTFOUND;
        }

        // .NET maps this, we would need to populate EXCEPINFO to do the same
        //
        // else if (hr == HRESULT.COR_E_TARGETINVOCATION)
        // {
        //     return HRESULT.DISP_E_EXCEPTION;
        // }

        return hr;
    }

    /// <summary>
    ///  Releases managed and unmanaged state for this dispatch wrapper.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="true"/> when called from <see cref="Dispose()"/>; otherwise <see langword="false"/>.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (_typeInfo is not null)
        {
            _typeInfo->Release();
            _typeInfo = null;
        }
    }

}