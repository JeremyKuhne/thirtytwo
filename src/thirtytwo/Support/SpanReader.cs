// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace Windows.Support;

/// <summary>
///  Simple span reader. Follows <see cref="SequenceReader{T}"/>.
/// </summary>
public ref struct SpanReader<T>(ReadOnlySpan<T> span) where T : unmanaged, IEquatable<T>
{
    public ReadOnlySpan<T> Span { get; } = span;
    public int Index { get; private set; }

    public readonly ReadOnlySpan<T> Remaining => Span[Index..];

    /// <summary>
    ///  Try to read everything up to the given <paramref name="delimiter"/>.
    /// </summary>
    /// <param name="span">The read data, if any.</param>
    /// <param name="delimiter">The delimiter to look for.</param>
    /// <param name="advancePastDelimiter"><see langword="true"/> to move past the <paramref name="delimiter"/> if found.</param>
    /// <returns><see langword="true"/> if the <paramref name="delimiter"/> was found.</returns>
    public bool TryReadTo(out ReadOnlySpan<T> span, T delimiter, bool advancePastDelimiter = true)
    {
        bool found = false;
        ReadOnlySpan<T> remaining = Remaining;
        int index = remaining.IndexOf(delimiter);

        if (index != -1)
        {
            span = index == 0 ? default : remaining[..index];
            Index += index + (advancePastDelimiter ? 1 : 0);
            found = true;
        }
        else
        {
            span = default;
        }

        return found;
    }
}