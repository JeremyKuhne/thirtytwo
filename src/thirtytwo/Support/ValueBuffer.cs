// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Windows.Support;

/// <summary>
///  Provides a growable unmanaged value buffer over an initial span with pooled heap fallback.
/// </summary>
/// <typeparam name="T">The unmanaged element type stored by this buffer.</typeparam>
/// <devdoc>Experimental. <see cref="BufferScope{T}"/> is probably a better strategy.</devdoc>
public ref struct ValueBuffer<T> where T : unmanaged
{
    private byte[]? _buffer;

    /// <summary>
    ///  Creates the buffer over an existing span.
    /// </summary>
    /// <param name="span">The initial storage span.</param>
    /// <remarks>This is useful when starting with stack-allocated storage and growing only if needed.</remarks>
    public ValueBuffer(Span<T> span)
    {
        Span = span;
        _buffer = null;
    }

    /// <summary>
    ///  Creates the buffer with an initial minimum element capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial required element capacity.</param>
    public ValueBuffer(int initialCapacity)
    {
        Span = default;
        _buffer = null;
        EnsureCapacity(initialCapacity);
    }

    /// <summary>
    ///  Gets the current writable span storage.
    /// </summary>
    /// <remarks>
    ///  The span may point to stack memory supplied at construction or to pooled heap memory allocated by <see cref="EnsureCapacity"/>.
    /// </remarks>
    public Span<T> Span { get; private set; }

    /// <summary>
    ///  Gets the current element capacity.
    /// </summary>
    public readonly int Length => Span.Length;

    /// <summary>
    ///  Ensures the buffer can hold at least <paramref name="capacity"/> elements.
    /// </summary>
    /// <param name="capacity">The required minimum element capacity.</param>
    /// <param name="copy">True to copy the existing elements when new space is allocated.</param>
    /// <remarks>
    ///  When growth is required, storage is rented from <see cref="ArrayPool{T}.Shared"/> as bytes, aligned for <typeparamref name="T"/>,
    ///  and the previously rented storage (if any) is returned to the pool.
    /// </remarks>
    public unsafe void EnsureCapacity(int capacity, bool copy = false)
    {
        if (capacity <= Span.Length)
        {
            return;
        }

        // We want to align to highest power of 2 less than the size/ up to
        // 128 bits (16 bytes), which should handle all alignment cases.
        int sizeOfT = Unsafe.SizeOf<T>();
        int alignTo = sizeOfT >= 16 ? 16
            : (sizeOfT & 8) != 0 ? 8
            : (sizeOfT & 4) != 0 ? 4
            : (sizeOfT & 2) != 0 ? 2
            : 1;

        // Get extra for possible realignment
        int byteCapacity = (capacity * sizeOfT) + alignTo;

        byte[] newBuffer = ArrayPool<byte>.Shared.Rent(byteCapacity);
        fixed (byte* b = newBuffer)
        {
            byte* p = b;

            // Align if necessary
            int offset = (int)((ulong)b % (uint)alignTo);
            if (offset > 0)
            {
                offset = alignTo - offset;
                p += offset;
            }

            Debug.Assert(((int)((ulong)p % (uint)alignTo)) == 0);

            Span<T> newSpan = new(p, (newBuffer.Length - offset) / sizeOfT);

            if (copy)
            {
                Span.CopyTo(newSpan);
            }

            if (_buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            _buffer = newBuffer;
            Span = newSpan;
        }
    }

    /// <summary>
    ///  Gets a by-reference element accessor into the current span.
    /// </summary>
    /// <param name="index">The zero-based element index.</param>
    /// <returns>A writable reference to the element at <paramref name="index"/>.</returns>
    public readonly ref T this[int index] => ref Span[index];

    /// <summary>
    ///  Gets a pinnable reference to the start of the current span.
    /// </summary>
    /// <returns>A reference to the first element of the span storage.</returns>
    public readonly ref T GetPinnableReference() => ref MemoryMarshal.GetReference(Span);

    /// <summary>
    ///  Creates a string from the first <paramref name="length"/> characters and then disposes the buffer.
    /// </summary>
    /// <param name="length">The number of characters to convert from the beginning of the span.</param>
    /// <returns>The constructed string.</returns>
    /// <remarks>
    ///  Intended for character buffers. After this call, any pooled storage held by this instance is returned.
    /// </remarks>
    public string ToStringAndDispose(int length)
    {
        string result = Span[..length].ToString();
        Dispose();
        return result;
    }

    /// <summary>
    ///  Returns any rented pooled storage and resets pooled ownership.
    /// </summary>
    /// <remarks>Calling this method is only required when pooled storage has been rented through growth.</remarks>
    public void Dispose()
    {
        if (_buffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
        }
    }

    /// <summary>
    ///  Executes a callback with a temporary <see cref="ValueBuffer{T}"/> initialized from stack storage.
    /// </summary>
    /// <typeparam name="TBuffer">The unmanaged element type of the temporary buffer.</typeparam>
    /// <param name="action">The callback to execute.</param>
    /// <param name="stackBufferSize">The initial stack buffer size in elements.</param>
    /// <remarks>Any pooled growth performed by the callback is cleaned up before returning.</remarks>
    public static void Invoke<TBuffer>(
        BufferAction<TBuffer> action,
        int stackBufferSize = 128)
        where TBuffer : unmanaged
    {
        Span<TBuffer> initialBuffer = stackalloc TBuffer[stackBufferSize];
        ValueBuffer<TBuffer> buffer = new(initialBuffer);
        action(ref buffer);
        buffer.Dispose();
    }

    /// <summary>
    ///  Executes a callback with a temporary <see cref="ValueBuffer{T}"/> initialized from stack storage and returns its result.
    /// </summary>
    /// <typeparam name="TBuffer">The unmanaged element type of the temporary buffer.</typeparam>
    /// <typeparam name="TResult">The callback result type.</typeparam>
    /// <param name="func">The callback to execute.</param>
    /// <param name="stackBufferSize">The initial stack buffer size in elements.</param>
    /// <returns>The value produced by <paramref name="func"/>.</returns>
    /// <remarks>Any pooled growth performed by the callback is cleaned up before returning.</remarks>
    public static TResult Invoke<TBuffer, TResult>(
        BufferFunc<TBuffer, TResult> func,
        int stackBufferSize = 128)
        where TBuffer : unmanaged
    {
        Span<TBuffer> initialBuffer = stackalloc TBuffer[stackBufferSize];
        ValueBuffer<TBuffer> buffer = new(initialBuffer);
        TResult result = func(ref buffer);
        buffer.Dispose();
        return result;
    }
}