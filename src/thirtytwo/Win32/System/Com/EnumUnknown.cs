// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public unsafe abstract class EnumUnknown : IEnumUnknown.Interface, IManagedWrapper<IEnumUnknown>
{
    private readonly int _count;
    private int _index;

    public EnumUnknown(int count, int index = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        _count = count;
        _index = index;
    }

    /// <inheritdoc cref="IEnumUnknown.Next(uint, IUnknown**, uint*)"/>
    HRESULT IEnumUnknown.Interface.Next(uint celt, IUnknown** rgelt, uint* pceltFetched)
    {
        if (rgelt is null)
        {
            return HRESULT.E_POINTER;
        }

        if (celt <= 0 || (celt > 1 && pceltFetched is null))
        {
            return HRESULT.E_INVALIDARG;
        }

        uint fetched = 0;
        for (; _index < _count && fetched < celt; _index++)
        {
            rgelt[fetched] = GetAtIndex(_index);
            fetched++;
        }

        if (pceltFetched is not null)
        {
            *pceltFetched = fetched;
        }

        return fetched == celt ? HRESULT.S_OK : HRESULT.S_FALSE;
    }

    /// <summary>
    ///  Gets the <see cref="IUnknown"/> at the specified index. It should be add ref'ed.
    /// </summary>
    protected abstract IUnknown* GetAtIndex(int index);

    /// <inheritdoc cref="IEnumUnknown.Skip(uint)"/>
    HRESULT IEnumUnknown.Interface.Skip(uint celt)
    {
        if (celt > _count - _index)
        {
            return HRESULT.S_FALSE;
        }

        _index += (int)celt;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IEnumUnknown.Reset()"/>
    HRESULT IEnumUnknown.Interface.Reset()
    {
        _index = 0;
        return HRESULT.S_OK;
    }

    /// <inheritdoc cref="IEnumUnknown.Clone(IEnumUnknown**)"/>
    HRESULT IEnumUnknown.Interface.Clone(IEnumUnknown** ppenum)
    {
        if (ppenum is null)
        {
            return HRESULT.E_INVALIDARG;
        }

        EnumUnknown clone = Clone();
        clone._index = _index;

        *ppenum = ComHelpers.TryGetComPointer<IEnumUnknown>(clone, out HRESULT hr);
        return hr;
    }

    /// <summary>
    ///  Clones the current object with the same enumeration state.
    /// </summary>
    protected abstract EnumUnknown Clone();
}