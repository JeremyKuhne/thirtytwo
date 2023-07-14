// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

/// <summary>
///  Base class that provides an <see cref="IDispatchEx"/> projection of all it's public properties.
/// </summary>
public unsafe abstract class ReflectPropertiesDispatch : StandardDispatch
{
    private readonly ClassPropertyDispatchAdapter _dispatchAdapter;

    public ReflectPropertiesDispatch() : base()
    {
        _dispatchAdapter = new(this);
    }

    protected override HRESULT GetDispID(BSTR bstrName, uint grfdex, int* pid)
    {
        if (_dispatchAdapter.TryGetDispID(bstrName.ToString(), out int dispid))
        {
            *pid = dispid;
            return HRESULT.S_OK;
        }

        *pid = Interop.DISPID_UNKNOWN;
        return HRESULT.DISP_E_UNKNOWNNAME;
    }

    protected override HRESULT GetMemberName(int id, BSTR* pbstrName)
    {
        if (_dispatchAdapter.TryGetMemberName(id, out string? name))
        {
            *pbstrName = new(name);
            return HRESULT.S_OK;
        }

        *pbstrName = default;
        return HRESULT.DISP_E_UNKNOWNNAME;
    }

    protected override HRESULT GetNextDispID(uint grfdex, int id, int* pid)
    {
        if (_dispatchAdapter.TryGetNextDispId(id, out int dispId))
        {
            *pid = dispId;
            return HRESULT.S_OK;
        }

        *pid = Interop.DISPID_UNKNOWN;
        return HRESULT.S_FALSE;
    }

    protected override HRESULT Invoke(
        int dispId,
        uint lcid,
        DISPATCH_FLAGS flags,
        DISPPARAMS* parameters,
        VARIANT* result,
        EXCEPINFO* exceptionInfo,
        IServiceProvider* pspCaller,
        uint* argumentError) => _dispatchAdapter.Invoke(dispId, lcid, flags, parameters, result);

    protected override HRESULT GetMemberProperties(int dispId, out FDEX_PROP_FLAGS properties)
    {
        if (_dispatchAdapter.TryGetMemberProperties(dispId, out properties))
        {
            return HRESULT.S_OK;
        }

        properties = default;
        return HRESULT.DISP_E_UNKNOWNNAME;
    }
}