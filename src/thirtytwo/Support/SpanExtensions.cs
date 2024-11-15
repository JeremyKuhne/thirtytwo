﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace System;

public static class SpanExtensions
{
    /// <summary>
    ///  Slice the given <paramref name="span"/> at null, if present.
    /// </summary>
    public static ReadOnlySpan<char> SliceAtNull(this ReadOnlySpan<char> span)
    {
        int index = span.IndexOf('\0');
        return index == -1 ? span : span[..index];
    }

    /// <summary>
    ///  Slice the given <paramref name="span"/> at null, if present.
    /// </summary>
    public static Span<char> SliceAtNull(this Span<char> span)
    {
        int index = span.IndexOf('\0');
        return index == -1 ? span : span[..index];
    }

    /// <summary>
    ///  Splits into strings on the given <paramref name="delimiter"/>.
    /// </summary>
    public static IEnumerable<string> SplitToEnumerable(this ReadOnlySpan<char> span, char delimiter, bool includeEmptyStrings = false)
    {
        List<string> strings = [];
        SpanReader<char> reader = new(span);
        while (reader.TryReadTo(delimiter, out var next))
        {
            if (includeEmptyStrings || !next.IsEmpty)
            {
                strings.Add(next.ToString());
            }
        }

        return strings;
    }

    /// <summary>
    ///  Converts a span of <see cref="BSTR"/>s to an array of <see langword="string"/>;
    /// </summary>
    public static string[] ToStringArray(this ReadOnlySpan<BSTR> bstrs)
    {
        if (bstrs.IsEmpty)
        {
            return [];
        }

        string[] strings = new string[bstrs.Length];
        for (int i = 0; i < bstrs.Length; i++)
        {
            strings[i] = bstrs[i].ToString();
        }

        return strings;
    }

    /*
        Currently not possible: https://github.com/dotnet/runtime/issues/109874

        public static T[] ToArray<T>(this MemoryExtensions.SpanSplitEnumerator<T> enumerator)
            where T : IEquatable<T>
        {
            using BufferScope<Range> ranges = new(stackalloc Range[10]);
            int index = -1;

            while (enumerator.MoveNext())
            {
                ranges.EnsureCapacity(index++);
                ranges[index] = enumerator.Current;
            }

            if (index == -1)
            {
                return [];
            }

            T[] result = new T[ranges.Length];
            for (int i = 0; i < ranges.Length; i++)
            {
                // The enumerator does not expose the span
                // result[i] = enumerator.Span.Slice(ranges[i])
            }
        }
    */
}