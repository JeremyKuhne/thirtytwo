// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class for providing <see cref="IDispatch"/> services through a standard dispatch implementation
///  generated from a type library.
/// </summary>
public unsafe abstract class StandardDispatch : IDispatch.Interface, IWrapperInitialize, IDisposable
{
    private readonly Guid _typeLibrary;
    private readonly ushort _majorVersion;
    private readonly ushort _minorVersion;
    private readonly Guid _interfaceId;
    private AgileComPointer<IDispatch>? _standardDispatch;

    /// <summary>
    ///  Construct a new instance from a registered type library.
    /// </summary>
    /// <param name="typeLibrary"><see cref="Guid"/> for the registered type library.</param>
    /// <param name="majorVersion">Type library major version.</param>
    /// <param name="minorVersion">Type library minor version.</param>
    /// <param name="interfaceId">The <see cref="Guid"/> for the interface the derived class presents.</param>
    public StandardDispatch(
        Guid typeLibrary,
        ushort majorVersion,
        ushort minorVersion,
        Guid interfaceId)
    {
        _typeLibrary = typeLibrary;
        _majorVersion = majorVersion;
        _minorVersion = minorVersion;
        _interfaceId = interfaceId;
    }

    void IWrapperInitialize.OnInitialized(IUnknown* unknown)
    {
        // Load the registered type library and get the relevant ITypeInfo for the specified interface.
        using ComScope<ITypeLib> typelib = new(null);
        Interop.LoadRegTypeLib(_typeLibrary, _majorVersion, _minorVersion, 0, typelib).ThrowOnFailure();

        using ComScope<ITypeInfo> typeinfo = new(null);
        typelib.Value->GetTypeInfoOfGuid(_interfaceId, typeinfo);

        // The unknown we get is a wrapper unknown.
        unknown->QueryInterface(_interfaceId, out void* instance).ThrowOnFailure();
        IUnknown* standard = default;
        Interop.CreateStdDispatch(
            unknown,
            instance,
            typeinfo.Value,
            &standard).ThrowOnFailure();

        _standardDispatch = new AgileComPointer<IDispatch>((IDispatch*)standard, takeOwnership: true);
    }

    private ComScope<IDispatch> Dispatch =>
        _standardDispatch is not { } standardDispatch
            ? throw new InvalidOperationException()
            : standardDispatch.GetInterface();

    HRESULT IDispatch.Interface.GetTypeInfoCount(uint* pctinfo)
    {
        using var dispatch = Dispatch;
        dispatch.Value->GetTypeInfoCount(pctinfo);
        return HRESULT.S_OK;
    }

    HRESULT IDispatch.Interface.GetTypeInfo(uint iTInfo, uint lcid, ITypeInfo** ppTInfo)
    {
        using var dispatch = Dispatch;
        dispatch.Value->GetTypeInfo(iTInfo, lcid, ppTInfo);
        return HRESULT.S_OK;
    }

    HRESULT IDispatch.Interface.GetIDsOfNames(Guid* riid, PWSTR* rgszNames, uint cNames, uint lcid, int* rgDispId)
    {
        using var dispatch = Dispatch;
        dispatch.Value->GetIDsOfNames(riid, rgszNames, cNames, lcid, rgDispId);
        return HRESULT.S_OK;
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
        using var dispatch = Dispatch;
        dispatch.Value->Invoke(dispIdMember, riid, lcid, wFlags, pDispParams, pVarResult, pExcepInfo, pArgErr);
        return HRESULT.S_OK;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _standardDispatch?.Dispose();
            _standardDispatch = null;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}