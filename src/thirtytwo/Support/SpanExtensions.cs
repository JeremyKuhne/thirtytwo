// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System;

public static class SpanExtensions
{
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