// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Foundation;

/// <summary>
///  Buffer specficially for allocating an array of <see cref="BSTR"/>s. Disposing the buffer will free all
///  of the contained <see cref="BSTR"/>s.
/// </summary>
/// <remarks>
///  <para>
///   This type cannot be nested in another ref type as it will result in it getting initialized in a copy which will
///   make the data in the buffer point to random stack data.
///  </para>
/// </remarks>
public unsafe ref struct BstrBuffer
{
    private const int StackSpace = 16;

    private StackBuffer _stackBuffer;
    private BufferScope<BSTR> _bufferScope;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
    public BstrBuffer(int length) => _bufferScope = new(_stackBuffer, length);
#pragma warning restore CS9084

    public readonly ref BSTR GetPinnableReference() => ref _bufferScope.GetPinnableReference();

    public void Clear()
    {
        for (int i = 0; i < _bufferScope.Length; i++)
        {
            _bufferScope[i].Dispose();
        }
    }

    public ref BSTR this[int i] => ref _bufferScope[i];

    public readonly Span<BSTR> this[Range range] => _bufferScope[range];

    public static implicit operator BSTR*(in BstrBuffer scope) =>
        (BSTR*)Unsafe.AsPointer(ref Unsafe.AsRef(in scope._stackBuffer._element0));

    public void Dispose()
    {
        Clear();
        _bufferScope.Dispose();
    }

    [InlineArray(StackSpace)]
    private struct StackBuffer
    {
        internal BSTR _element0;
    }
}