// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public static class Conversion
{
    /// <summary>
    ///  Wraps a span around a buffer that points to a null terminated string.
    /// </summary>
    public static unsafe ReadOnlySpan<char> NullTerminatedStringToSpan(char* buffer)
    {
        if (buffer == null)
            return default;

        char* end = buffer;
        while (*(++end) != '\0') { }

        return new ReadOnlySpan<char>(buffer, (int)(end - buffer));
    }

    public static ulong HighLowToLong(uint high, uint low) => ((ulong)high) << 32 | low;
    public static long HighLowToLong(int high, int low) => (long)HighLowToLong((uint)high, (uint)low);
    public static uint HighLowToInt(ushort high, ushort low) => ((uint)high) << 16 | low;
    public static int HighLowToInt(short high, short low) => (int)HighLowToInt((ushort)high, (ushort)low);
}