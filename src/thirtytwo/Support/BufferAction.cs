// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents an operation that consumes a <see cref="ValueBuffer{T}"/> by reference.
/// </summary>
/// <typeparam name="T">The unmanaged element type stored in the buffer.</typeparam>
/// <param name="buffer">The growable buffer that can be read from or resized by the callback.</param>
public delegate void BufferAction<T>(ref ValueBuffer<T> buffer)
    where T : unmanaged;