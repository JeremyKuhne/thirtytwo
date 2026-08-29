// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a buffer for <see cref="BSTR"/> values.
/// </summary>
/// <remarks>
///  <para>
///   Disposing this buffer disposes each contained <see cref="BSTR"/> and then releases the backing storage.
///  </para>
///  <para>
///   This type cannot be nested in another ref type as it will result in it getting initialized in a copy which will
///   make the data in the buffer point to random stack data.
///  </para>
/// </remarks>
[NonCopyable]
public unsafe ref partial struct BstrBuffer
{
    /// <summary>
    ///  The number of <see cref="BSTR"/> elements available in inline stack storage.
    /// </summary>
    private const int StackSpace = 16;

    /// <summary>
    ///  Inline stack storage used to initialize the backing buffer.
    /// </summary>
    private StackBuffer _stackBuffer;

    /// <summary>
    ///  Backing storage scope for the requested <see cref="BSTR"/> length.
    /// </summary>
    private BufferScope<BSTR> _bufferScope;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
    /// <summary>
    ///  Initializes a <see cref="BstrBuffer"/> with the requested element count.
    /// </summary>
    /// <param name="length">The number of <see cref="BSTR"/> elements to expose.</param>
    public BstrBuffer(int length) => _bufferScope = new(_stackBuffer, length);
#pragma warning restore CS9084

    /// <summary>
    ///  Gets a pinnable reference to the first <see cref="BSTR"/> element in the active buffer.
    /// </summary>
    /// <returns>A reference to the first element.</returns>
    public readonly ref BSTR GetPinnableReference() => ref _bufferScope.GetPinnableReference();

    /// <summary>
    ///  Disposes all currently stored <see cref="BSTR"/> values in-place.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _bufferScope.Length; i++)
        {
            _bufferScope[i].Dispose();
        }
    }

    /// <summary>
    ///  Gets a reference to the <see cref="BSTR"/> at the specified index.
    /// </summary>
    /// <param name="i">The zero-based index into the buffer.</param>
    /// <returns>A reference to the indexed <see cref="BSTR"/> slot.</returns>
    public ref BSTR this[int i] => ref _bufferScope[i];

    /// <summary>
    ///  Gets a span view over a subrange of the buffer.
    /// </summary>
    /// <param name="range">The range to project from the current buffer.</param>
    /// <returns>A span over the requested range.</returns>
    public readonly Span<BSTR> this[Range range] => _bufferScope[range];

    /// <summary>
    ///  Converts the buffer to a native pointer to the first inline stack element.
    /// </summary>
    /// <param name="scope">The source buffer.</param>
    /// <returns>A <see cref="BSTR"/> pointer to the embedded stack storage.</returns>
    /// <remarks>
    ///  The returned pointer is only valid while <paramref name="scope"/> remains in scope and must not be cached.
    /// </remarks>
    public static implicit operator BSTR*(in BstrBuffer scope) =>
        (BSTR*)Unsafe.AsPointer(ref Unsafe.AsRef(in scope._stackBuffer._element0));

    /// <summary>
    ///  Disposes every contained <see cref="BSTR"/>, then disposes the backing storage scope.
    /// </summary>
    public void Dispose()
    {
        Clear();
        _bufferScope.Dispose();
    }
}