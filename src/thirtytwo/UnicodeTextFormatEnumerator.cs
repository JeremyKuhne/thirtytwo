// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Windows;

internal sealed unsafe class UnicodeTextFormatEnumerator : IEnumFORMATETC.Interface, IManagedWrapper<IEnumFORMATETC>
{
    private bool _enumerated;

    internal UnicodeTextFormatEnumerator(bool enumerated = false) => _enumerated = enumerated;

    HRESULT IEnumFORMATETC.Interface.Next(uint count, FORMATETC* formats, uint* fetched)
    {
        if (fetched is not null)
        {
            *fetched = 0;
        }

        if (count == 0)
        {
            return HRESULT.S_OK;
        }

        if (formats is null || (count != 1 && fetched is null))
        {
            return HRESULT.E_POINTER;
        }

        if (_enumerated)
        {
            return PInvoke.S_FALSE;
        }

        formats[0] = CreateFormat();
        _enumerated = true;
        if (fetched is not null)
        {
            *fetched = 1;
        }

        return count == 1 ? HRESULT.S_OK : PInvoke.S_FALSE;
    }

    HRESULT IEnumFORMATETC.Interface.Skip(uint count)
    {
        if (count == 0)
        {
            return HRESULT.S_OK;
        }

        if (_enumerated)
        {
            return PInvoke.S_FALSE;
        }

        _enumerated = true;
        return count == 1 ? HRESULT.S_OK : PInvoke.S_FALSE;
    }

    HRESULT IEnumFORMATETC.Interface.Reset()
    {
        _enumerated = false;
        return HRESULT.S_OK;
    }

    HRESULT IEnumFORMATETC.Interface.Clone(IEnumFORMATETC** enumerator)
    {
        if (enumerator is null)
        {
            return HRESULT.E_POINTER;
        }

        *enumerator = new UnicodeTextFormatEnumerator(_enumerated).GetComPointer<IEnumFORMATETC>();
        return HRESULT.S_OK;
    }

    private static FORMATETC CreateFormat()
        => new()
        {
            cfFormat = (ushort)CLIPBOARD_FORMAT.CF_UNICODETEXT,
            dwAspect = (uint)DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            tymed = (uint)TYMED.TYMED_HGLOBAL
        };
}
