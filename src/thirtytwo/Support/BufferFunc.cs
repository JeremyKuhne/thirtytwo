// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents an operation that consumes a <see cref="ValueBuffer{T}"/> by reference and returns a value.
/// </summary>
/// <typeparam name="TBuffer">The unmanaged element type stored in the buffer.</typeparam>
/// <typeparam name="TResult">The result type produced by the callback.</typeparam>
/// <param name="buffer">The growable buffer that can be read from or resized by the callback.</param>
/// <returns>The value produced by the callback.</returns>
public delegate TResult BufferFunc<TBuffer, TResult>(ref ValueBuffer<TBuffer> buffer)
    where TBuffer : unmanaged;