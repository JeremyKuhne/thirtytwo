// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Support;

/// <summary>
///  Simple buffer that has 16 <typeparamref name="T"/>s on the stack. If the buffer needs to be larger than 16
///  elements it will be allocated on the heap.
/// </summary>
/// <remarks>
///  <para>
///   This type cannot be nested in another ref type as it will result in it getting initialized in a copy which will
///   make the data in the buffer point to random stack data.
///  </para>
/// </remarks>
public unsafe ref struct StackBufferScope16<T> where T : unmanaged
{
    private const int StackSpace = 16;

    private StackBuffer _stackBuffer;
    private BufferScope<T> _bufferScope;

    public StackBufferScope16(int length)
    {
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        _bufferScope = new BufferScope<T>(_stackBuffer, length);
#pragma warning restore CS9084
    }

    public ref T this[int i] => ref _bufferScope[i];

    public readonly Span<T> this[Range range] => _bufferScope[range];
    public readonly int Length => _bufferScope.Length;

    public readonly Span<T>.Enumerator GetEnumerator() => _bufferScope.GetEnumerator();
    public readonly ref T GetPinnableReference() => ref _bufferScope.GetPinnableReference();
    public void Dispose() => _bufferScope.Dispose();

    [InlineArray(StackSpace)]
    private struct StackBuffer
    {
        internal T _element0;
    }
}