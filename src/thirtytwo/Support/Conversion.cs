// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Provides helper methods for pointer-to-span and high/low word conversions.
/// </summary>
public static class Conversion
{
    /// <summary>
    ///  Wraps a span around a buffer that points to a null-terminated string.
    /// </summary>
    /// <param name="buffer">
    ///  A pointer to the first character of a null-terminated string, or <see langword="null"/>.
    /// </param>
    /// <returns>
    ///  A span over the characters before the terminator, or an empty span when <paramref name="buffer"/> is null.
    /// </returns>
    public static unsafe ReadOnlySpan<char> NullTerminatedStringToSpan(char* buffer)
    {
        if (buffer is null)
            return default;

        char* end = buffer;
        while (*(++end) != '\0') { }

        return new ReadOnlySpan<char>(buffer, (int)(end - buffer));
    }

    /// <summary>
    ///  Combines high and low 32-bit unsigned values into a 64-bit unsigned value.
    /// </summary>
    /// <param name="high">The high-order 32 bits.</param>
    /// <param name="low">The low-order 32 bits.</param>
    /// <returns>The combined 64-bit value.</returns>
    public static ulong HighLowToLong(uint high, uint low) => ((ulong)high) << 32 | low;

    /// <summary>
    ///  Combines high and low 32-bit signed values into a 64-bit signed value.
    /// </summary>
    /// <param name="high">The high-order 32 bits.</param>
    /// <param name="low">The low-order 32 bits.</param>
    /// <returns>The combined 64-bit value.</returns>
    public static long HighLowToLong(int high, int low) => (long)HighLowToLong((uint)high, (uint)low);

    /// <summary>
    ///  Combines high and low 16-bit unsigned values into a 32-bit unsigned value.
    /// </summary>
    /// <param name="high">The high-order 16 bits.</param>
    /// <param name="low">The low-order 16 bits.</param>
    /// <returns>The combined 32-bit value.</returns>
    public static uint HighLowToInt(ushort high, ushort low) => ((uint)high) << 16 | low;

    /// <summary>
    ///  Combines high and low 16-bit signed values into a 32-bit signed value.
    /// </summary>
    /// <param name="high">The high-order 16 bits.</param>
    /// <param name="low">The low-order 16 bits.</param>
    /// <returns>The combined 32-bit value.</returns>
    public static int HighLowToInt(short high, short low) => (int)HighLowToInt((ushort)high, (ushort)low);
}