// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Ole;

/// <summary>
///  Base implementation of <see cref="IEnumVARIANT"/> over an indexable managed source.
/// </summary>
/// <remarks>
///  <para>
///   This type tracks enumeration position and implements COM <c>Next</c>, <c>Skip</c>, <c>Reset</c>, and
///   <c>Clone</c> semantics.
///  </para>
/// </remarks>
public unsafe abstract class EnumVARIANT : IEnumVARIANT.Interface, IManagedWrapper<IEnumVARIANT>
{
    private readonly int _count;
    private int _index;

    /// <summary>
    ///  Initializes a new enumerator with the specified total <paramref name="count"/> and starting
    ///  <paramref name="index"/>.
    /// </summary>
    /// <param name="count">Total number of elements available to enumerate.</param>
    /// <param name="index">Starting zero-based index.</param>
    public EnumVARIANT(int count, int index = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

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

        return fetched == celt ? HRESULT.S_OK : PInvoke.S_FALSE;
    }

    /// <summary>
    ///  Gets the <see cref="VARIANT"/> at the specified index.
    /// </summary>
    /// <param name="index">Zero-based item index to materialize.</param>
    /// <returns>
    ///  Value returned to the COM caller through <see cref="IEnumVARIANT.Interface.Next(uint, VARIANT*, uint*)"/>.
    /// </returns>
    /// <remarks>
    ///  <para>
    ///   Implementations should populate the returned variant as a COM out value: ownership of any contained
    ///   resources is transferred to the caller, which is responsible for clearing the returned variant.
    ///  </para>
    /// </remarks>
    protected abstract VARIANT GetAtIndex(int index);

    /// <inheritdoc cref="IEnumVARIANT.Skip(uint)"/>
    HRESULT IEnumVARIANT.Interface.Skip(uint celt)
    {
        if (celt > _count - _index)
        {
            return PInvoke.S_FALSE;
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

        *ppEnum = clone.TryGetComPointer<IEnumVARIANT>(out HRESULT hr);
        return hr;
    }

    /// <summary>
    ///  Clones the current object with the same enumeration state.
    /// </summary>
    /// <returns>A new enumerator instance that can continue from the current position.</returns>
    protected abstract EnumVARIANT Clone();
}