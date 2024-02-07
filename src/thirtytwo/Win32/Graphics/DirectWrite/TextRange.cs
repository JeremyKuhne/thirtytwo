// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Specifies a range of text positions where format is applied.
///  [<see cref="DWRITE_TEXT_RANGE"/>]
/// </summary>
public unsafe readonly struct TextRange
{
    /// <summary>
    ///  The start text position of the range.
    /// </summary>
    public readonly uint StartPosition;

    /// <summary>
    ///  The number of text positions in the range.
    /// </summary>
    public readonly uint Length;

    public TextRange(uint startPosition, uint length)
    {
        StartPosition = startPosition;
        Length = length;
    }

    public static implicit operator TextRange((int StartPosition, int Length) tuple)
        => new((uint)tuple.StartPosition, (uint)tuple.Length);

    public static implicit operator DWRITE_TEXT_RANGE(TextRange range) =>
        Unsafe.As<TextRange, DWRITE_TEXT_RANGE>(ref range);
}