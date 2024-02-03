// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class for providing <see cref="IDispatch"/> services around an existing <see cref="ITypeInfo"/>.
/// </summary>
public unsafe abstract class StandardDispatch : IDispatch.Interface, IDispatchEx.Interface, IDisposable
{
    private ITypeInfo* _typeInfo;

    /// <summary>
    ///  Construct a new instance with the specified backing <see cref="ITypeInfo"/>.
    /// </summary>
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

    HRESULT IDispatch.Interface.GetTypeInfoCount(uint* pctinfo)
    {
        if (pctinfo is null)
        {
            return HRESULT.E_POINTER;
        }

        *pctinfo = 1;
        return HRESULT.S_OK;
    }

    HRESULT IDispatch.Interface.GetTypeInfo(uint iTInfo, uint lcid, ITypeInfo** ppTInfo)
    {
        if (ppTInfo is null)
        {
            return HRESULT.E_POINTER;
        }

        if (iTInfo != 0)
        {
            *ppTInfo = null;
            return HRESULT.DISP_E_BADINDEX;
        }

        _typeInfo->AddRef();
        *ppTInfo = _typeInfo;
        return HRESULT.S_OK;
    }

    HRESULT IDispatch.Interface.GetIDsOfNames(Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId)
    {
        // This must bee IID_NULL
        if (riid != IID.Empty())
        {
            return HRESULT.DISP_E_UNKNOWNINTERFACE;
        }

        return _typeInfo->GetIDsOfNames(rgszNames, cNames, rgDispId);
    }

    HRESULT IDispatch.Interface.Invoke(
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
            return HRESULT.DISP_E_UNKNOWNINTERFACE;
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

        if (hr != HRESULT.DISP_E_MEMBERNOTFOUND)
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
    protected virtual ComScope GetComCallableWrapper() => default;

    HRESULT IDispatchEx.Interface.GetDispID(BSTR bstrName, uint grfdex, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = Interop.DISPID_UNKNOWN;
        return bstrName.IsNull ? HRESULT.E_POINTER : GetDispID(bstrName, grfdex, pid);
    }

    /// <summary>
    ///  Override to provide a dispatch id for the given name. Return <see cref="HRESULT.DISP_E_UNKNOWNNAME"/>
    ///  if the name isn't supported.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/reference/idispatchex-getdispid">
    ///    Official documentation.
    ///   </see>
    ///  </para>
    /// </remarks>
    protected virtual HRESULT GetDispID(BSTR bstrName, uint grfdex, int* pid) => HRESULT.DISP_E_UNKNOWNNAME;

    HRESULT IDispatchEx.Interface.GetMemberName(int id, BSTR* pbstrName)
        => pbstrName is null ? HRESULT.E_POINTER : GetMemberName(id, pbstrName);

    /// <summary>
    ///  Override to provide the name for a given dispatch id. Return <see cref="HRESULT.DISP_E_UNKNOWNNAME"/> if the
    ///  name isn't known.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/reference/idispatchex-getmembername">
    ///    Official documentation.
    ///   </see>
    ///  </para>
    /// </remarks>
    protected virtual HRESULT GetMemberName(int id, BSTR* pbstrName) => HRESULT.DISP_E_UNKNOWNNAME;

    HRESULT IDispatchEx.Interface.GetNextDispID(uint grfdex, int id, int* pid)
    {
        if (pid is null)
        {
            return HRESULT.E_POINTER;
        }

        *pid = Interop.DISPID_UNKNOWN;

        return GetNextDispID(grfdex, id, pid);
    }

    /// <param name="id">
    ///  <see cref="Interop.DISPID_STARTENUM"/> to start enumeration, or the last id returned by a previous call to
    ///  <see cref="GetNextDispID(uint, int, int*)"/>.
    /// </param>
    /// <param name="pid">The next dispatch id.</param>
    /// <returns>The next dispatch id, or <see cref="HRESULT.S_FALSE"/> if there are no more.</returns>
    protected virtual HRESULT GetNextDispID(uint grfdex, int id, int* pid) => HRESULT.S_FALSE;

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

        if (hr != HRESULT.DISP_E_MEMBERNOTFOUND)
        {
            return hr;
        }

        // The override couldn't find it, pass our own interface along so it can be dispatche
        using ComScope @interface = GetComCallableWrapper();
        return @interface.IsNull ? hr : _typeInfo->Invoke(@interface, id, (DISPATCH_FLAGS)wFlags, pdp, pvarRes, pei, puArgErr: null);
    }

    protected virtual HRESULT Invoke(
        int dispId,
        uint lcid,
        DISPATCH_FLAGS flags,
        DISPPARAMS* parameters,
        VARIANT* result,
        EXCEPINFO* exceptionInfo,
        IServiceProvider* serviceProvider,
        uint* argumentError)
        => HRESULT.DISP_E_MEMBERNOTFOUND;

    HRESULT IDispatchEx.Interface.GetMemberProperties(int id, uint grfdexFetch, FDEX_PROP_FLAGS* pgrfdex)
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

    protected virtual HRESULT GetMemberProperties(int dispId, out FDEX_PROP_FLAGS properties)
    {
        properties = default;
        return HRESULT.E_NOTIMPL;
    }

    // .NET COM Interop returns E_NOTIMPL for these three.

    HRESULT IDispatchEx.Interface.DeleteMemberByName(BSTR bstrName, uint grfdex) => HRESULT.E_NOTIMPL;
    HRESULT IDispatchEx.Interface.DeleteMemberByDispID(int id) => HRESULT.E_NOTIMPL;

    HRESULT IDispatchEx.Interface.GetNameSpaceParent(IUnknown** ppunk)
    {
        if (ppunk is null)
        {
            return HRESULT.E_POINTER;
        }

        *ppunk = null;
        return HRESULT.E_NOTIMPL;
    }

    private static HRESULT MapDotNetHRESULTs(HRESULT hr)
    {
        if (hr == HRESULT.COR_E_OVERFLOW)
        {
            return HRESULT.DISP_E_OVERFLOW;
        }
        else if (hr == HRESULT.COR_E_INVALIDOLEVARIANTTYPE)
        {
            return HRESULT.DISP_E_BADVARTYPE;
        }
        else if (hr == HRESULT.COR_E_ARGUMENT)
        {
            return HRESULT.E_INVALIDARG;
        }
        else if (hr == HRESULT.COR_E_SAFEARRAYTYPEMISMATCH)
        {
            return HRESULT.DISP_E_TYPEMISMATCH;
        }
        else if (hr == HRESULT.COR_E_MISSINGMEMBER || hr == HRESULT.COR_E_MISSINGMETHOD)
        {
            return HRESULT.DISP_E_MEMBERNOTFOUND;
        }

        // .NET maps this, we would need to populate EXCEPINFO to do the same
        //
        // else if (hr == HRESULT.COR_E_TARGETINVOCATION)
        // {
        //     return HRESULT.DISP_E_EXCEPTION;
        // }

        return hr;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_typeInfo is not null)
        {
            _typeInfo->Release();
            _typeInfo = null;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}