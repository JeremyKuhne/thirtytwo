// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Ole;

namespace Windows;

/// <summary>Provides Unicode text through the OLE <see cref="IDataObject"/> contract.</summary>
/// <remarks>Exposes only <see cref="CLIPBOARD_FORMAT.CF_UNICODETEXT"/> using movable global memory.</remarks>
internal sealed unsafe class UnicodeTextDataObject : IDataObject.Interface, IManagedWrapper<IDataObject>
{
    private readonly string _text;

    internal UnicodeTextDataObject(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
    }

    HRESULT IDataObject.Interface.GetData(FORMATETC* format, STGMEDIUM* medium)
    {
        if (format is null || medium is null)
        {
            return HRESULT.E_POINTER;
        }

        *medium = default;
        HRESULT result = QueryFormat(format);
        if (result.Failed)
        {
            return result;
        }

        nuint byteLength;
        try
        {
            byteLength = checked((nuint)(_text.Length + 1) * sizeof(char));
        }
        catch (OverflowException)
        {
            return PInvoke.STG_E_MEDIUMFULL;
        }

        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, byteLength);
        if (global.IsNull)
        {
            return PInvoke.STG_E_MEDIUMFULL;
        }

        bool returnGlobal = false;
        try
        {
            void* memory = PInvoke.GlobalLock(global);
            if (memory is null)
            {
                return PInvoke.STG_E_MEDIUMFULL;
            }

            try
            {
                Span<char> buffer = new(memory, _text.Length + 1);
                _text.AsSpan().CopyTo(buffer);
                buffer[^1] = '\0';
            }
            finally
            {
                _ = PInvoke.GlobalUnlock(global);
            }

            medium->tymed = TYMED.TYMED_HGLOBAL;
            medium->u.hGlobal = global;
            medium->pUnkForRelease = null;
            returnGlobal = true;
            return HRESULT.S_OK;
        }
        finally
        {
            if (!returnGlobal)
            {
                PInvoke.GlobalFree(global);
            }
        }
    }

    HRESULT IDataObject.Interface.GetDataHere(FORMATETC* format, STGMEDIUM* medium)
        => PInvoke.E_NOTIMPL;

    HRESULT IDataObject.Interface.QueryGetData(FORMATETC* format)
        => format is null ? HRESULT.E_POINTER : QueryFormat(format);

    HRESULT IDataObject.Interface.GetCanonicalFormatEtc(FORMATETC* input, FORMATETC* output)
    {
        if (input is null || output is null)
        {
            return HRESULT.E_POINTER;
        }

        *output = *input;
        output->ptd = null;
        return PInvoke.DATA_S_SAMEFORMATETC;
    }

    HRESULT IDataObject.Interface.SetData(FORMATETC* format, STGMEDIUM* medium, BOOL release)
        => PInvoke.E_NOTIMPL;

    HRESULT IDataObject.Interface.EnumFormatEtc(uint direction, IEnumFORMATETC** formats)
    {
        if (formats is null)
        {
            return HRESULT.E_POINTER;
        }

        *formats = null;
        if (direction != (uint)DATADIR.DATADIR_GET)
        {
            return PInvoke.E_NOTIMPL;
        }

        *formats = new UnicodeTextFormatEnumerator().GetComPointer<IEnumFORMATETC>();
        return HRESULT.S_OK;
    }

    HRESULT IDataObject.Interface.DAdvise(FORMATETC* format, uint flags, IAdviseSink* sink, uint* connection)
    {
        if (connection is not null)
        {
            *connection = 0;
        }

        return PInvoke.OLE_E_ADVISENOTSUPPORTED;
    }

    HRESULT IDataObject.Interface.DUnadvise(uint connection) => PInvoke.OLE_E_ADVISENOTSUPPORTED;

    HRESULT IDataObject.Interface.EnumDAdvise(IEnumSTATDATA** enumerator)
    {
        if (enumerator is not null)
        {
            *enumerator = null;
        }

        return PInvoke.OLE_E_ADVISENOTSUPPORTED;
    }

    private static HRESULT QueryFormat(FORMATETC* format)
    {
        if (format->cfFormat != (ushort)CLIPBOARD_FORMAT.CF_UNICODETEXT)
        {
            return PInvoke.DV_E_FORMATETC;
        }

        if (format->ptd is not null)
        {
            return PInvoke.DV_E_DVTARGETDEVICE;
        }

        if (format->dwAspect != (uint)DVASPECT.DVASPECT_CONTENT)
        {
            return PInvoke.DV_E_DVASPECT;
        }

        if (format->lindex != -1)
        {
            return PInvoke.DV_E_LINDEX;
        }

        return (format->tymed & (uint)TYMED.TYMED_HGLOBAL) == 0
            ? PInvoke.DV_E_TYMED
            : HRESULT.S_OK;
    }
}
