// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Specifies a range of text positions where formatting is applied.
///  [<see cref="DWRITE_TEXT_RANGE"/>]
/// </summary>
/// <remarks>Positions and lengths are expressed as UTF-16 code unit counts, matching DirectWrite APIs.</remarks>
public readonly struct TextRange
{
    /// <summary>
    ///  The start text position of the range.
    /// </summary>
    public readonly uint StartPosition;

    /// <summary>
    ///  The number of text positions in the range.
    /// </summary>
    public readonly uint Length;

    /// <summary>
    ///  Initializes a text range.
    /// </summary>
    /// <param name="startPosition">The start position, in UTF-16 code units.</param>
    /// <param name="length">The number of UTF-16 code units in the range.</param>
    public TextRange(uint startPosition, uint length)
    {
        StartPosition = startPosition;
        Length = length;
    }

    /// <summary>
    ///  Converts a tuple into a text range.
    /// </summary>
    /// <param name="tuple">The tuple containing start position and length.</param>
    /// <returns>A <see cref="TextRange"/> initialized from the tuple values.</returns>
    public static implicit operator TextRange((int StartPosition, int Length) tuple)
        => new((uint)tuple.StartPosition, (uint)tuple.Length);

    /// <summary>
    ///  Converts this value to the native DirectWrite text-range structure.
    /// </summary>
    /// <param name="range">The managed text range to convert.</param>
    /// <returns>The equivalent <see cref="DWRITE_TEXT_RANGE"/> value.</returns>
    public static implicit operator DWRITE_TEXT_RANGE(TextRange range) =>
        Unsafe.As<TextRange, DWRITE_TEXT_RANGE>(ref range);
}