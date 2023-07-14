// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Ole;

public unsafe abstract class EnumVARIANT : IEnumVARIANT.Interface, IManagedWrapper<IEnumVARIANT>
{
    private readonly int _count;
    private int _index;

    public EnumVARIANT(int count, int index = 0)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _count = count;
        _index = index;
    }

    /// <inheritdoc cref="IEnumVARIANT.Next(uint, VARIANT*, uint*)"/>
    HRESULT IEnumVARIANT.Interface.Next(uint celt, VARIANT* rgVar, uint* pCeltFetched)
    {
        if (rgVar is null)
        {
            return HRESULT.E_POINTER;
        }

        if (celt <= 0 || (celt > 1 && pCeltFetched is null))
        {
            return HRESULT.E_INVALIDARG;
        }

        uint fetched = 0;
        for (; _index < _count && fetched < celt; _index++)
        {
            rgVar[fetched] = GetAtIndex(_index);
            fetched++;
        }

        if (pCeltFetched is not null)
        {
            *pCeltFetched = fetched;
        }

        return fetched == celt ? HRESULT.S_OK : HRESULT.S_FALSE;
    }

    /// <summary>
    ///  Gets the <see cref="VARIANT"/> at the specified index.
    /// </summary>
    protected abstract VARIANT GetAtIndex(int index);

    /// <inheritdoc cref="IEnumVARIANT.Skip(uint)"/>
    HRESULT IEnumVARIANT.Interface.Skip(uint celt)
    {
        if (celt > _count - _index)
        {
            return HRESULT.S_FALSE;
        }

        _index += (int)celt;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IEnumVARIANT.Reset()"/>
    HRESULT IEnumVARIANT.Interface.Reset()
    {
        _index = 0;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IEnumVARIANT.Clone(IEnumVARIANT**)"/>
    HRESULT IEnumVARIANT.Interface.Clone(IEnumVARIANT** ppEnum)
    {
        if (ppEnum is null)
        {
            return HRESULT.E_INVALIDARG;
        }

        EnumVARIANT clone = Clone();
        clone._index = _index;

        *ppEnum = ComHelpers.TryGetComPointer<IEnumVARIANT>(clone, out HRESULT hr);
        return hr;
    }

    /// <summary>
    ///  Clones the current object with the same enumeration state.
    /// </summary>
    protected abstract EnumVARIANT Clone();
}