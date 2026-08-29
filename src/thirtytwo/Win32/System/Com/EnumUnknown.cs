// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Base implementation of <see cref="IEnumUnknown"/> over an indexable sequence.
/// </summary>
public unsafe abstract class EnumUnknown : IEnumUnknown.Interface, IManagedWrapper<IEnumUnknown>
{
    private readonly int _count;
    private int _index;

    /// <summary>
    ///  Initializes an enumerator with total count and optional starting index.
    /// </summary>
    /// <param name="count">Total number of elements available for enumeration.</param>
    /// <param name="index">Initial index position.</param>
    public EnumUnknown(int count, int index = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        _count = count;
        _index = index;
    }

    /// <inheritdoc cref="IEnumUnknown.Next(uint, IUnknown**, uint*)"/>
    /// <remarks>
    ///  <para>
    ///   Each element written to <paramref name="rgelt"/> must be an AddRef'd pointer. Output pointers are
    ///   caller-owned COM references when returned.
    ///  </para>
    /// </remarks>
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

        return fetched == celt ? HRESULT.S_OK : PInvoke.S_FALSE;
    }

    /// <summary>
    ///  <para>
    ///   Gets the <see cref="IUnknown"/> at the specified index.
    ///  </para>
    /// </summary>
    /// <param name="index">Zero-based element index.</param>
    /// <returns>
    ///  <para>
    ///   A caller-owned AddRef'd pointer that will be copied into the COM enumeration output buffer.
    ///  </para>
    /// </returns>
    protected abstract IUnknown* GetAtIndex(int index);

    /// <inheritdoc cref="IEnumUnknown.Skip(uint)"/>
    HRESULT IEnumUnknown.Interface.Skip(uint celt)
    {
        if (celt > _count - _index)
        {
            return PInvoke.S_FALSE;
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
    /// <remarks>
    ///  <para>
    ///   On success, <paramref name="ppenum"/> receives a caller-owned AddRef'd enumerator pointer.
    ///  </para>
    /// </remarks>
    HRESULT IEnumUnknown.Interface.Clone(IEnumUnknown** ppenum)
    {
        if (ppenum is null)
        {
            return HRESULT.E_INVALIDARG;
        }

        EnumUnknown clone = Clone();
        clone._index = _index;

        *ppenum = clone.TryGetComPointer<IEnumUnknown>(out HRESULT hr);
        return hr;
    }

    /// <summary>
    ///  Clones the current object with the same enumeration state.
    /// </summary>
    /// <returns>A new enumerator instance.</returns>
    protected abstract EnumUnknown Clone();
}