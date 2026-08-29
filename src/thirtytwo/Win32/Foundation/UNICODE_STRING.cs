// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe partial struct UNICODE_STRING
{
    /// <summary>
    ///  Gets the current string length in UTF-16 characters.
    /// </summary>
    public readonly int LengthInChars => Length / sizeof(char);

    /// <summary>
    ///  Gets the maximum buffer length in UTF-16 characters.
    /// </summary>
    public readonly int MaximumLengthInChars => MaximumLength / sizeof(char);

    /// <summary>
    ///  Gets a read-only span over the current value described by <see cref="Length"/>.
    /// </summary>
    /// <remarks>
    ///  The returned span references unmanaged memory owned by the native buffer and is valid only while that memory
    ///  remains valid.
    /// </remarks>
    [UnscopedRef]
    public readonly ReadOnlySpan<char> CurrentValue => new(Buffer, LengthInChars);

    /// <summary>
    ///  Gets a writable span over the full buffer capacity described by <see cref="MaximumLength"/>.
    /// </summary>
    /// <remarks>
    ///  The returned span references unmanaged memory owned by the native buffer and is valid only while that memory
    ///  remains valid.
    /// </remarks>
    [UnscopedRef]
    public readonly Span<char> FullBuffer => new(Buffer, MaximumLengthInChars);

    /// <summary>
    ///  Creates a managed string from <see cref="CurrentValue"/>.
    /// </summary>
    /// <returns>A managed copy of the current UTF-16 character data.</returns>
    public override readonly string ToString() => CurrentValue.ToString();
}