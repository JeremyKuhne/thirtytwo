﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Simple run length encoder (RLE) that works on spans.
/// </summary>
/// <remarks>
///  <para>
///   Format used is a byte for the count, followed by a byte for the value.
///  </para>
/// </remarks>
public static class RunLengthEncoder
{
    /// <summary>
    ///  Get the encoded length, in bytes, of the given data.
    /// </summary>
    public static int GetEncodedLength(ReadOnlySpan<byte> data)
    {
        SpanReader<byte> reader = new(data);

        int length = 0;
        while (reader.TryRead(out byte value))
        {
            int count = reader.AdvancePast(value) + 1;
            while (count > 0)
            {
                // 1 byte for the count, 1 byte for the value
                length += 2;
                count -= 0xFF;
            }
        }

        return length;
    }

    /// <summary>
    ///  Get the decoded length, in bytes, of the given encoded data.
    /// </summary>
    public static int GetDecodedLength(ReadOnlySpan<byte> encoded)
    {
        int length = 0;
        for (int i = 0; i < encoded.Length; i += 2)
        {
            length += encoded[i];
        }

        return length;
    }

    /// <summary>
    ///  Encode the given data into the given <paramref name="encoded"/> span.
    /// </summary>
    /// <returns>
    ///  <see langword="false"/> if the <paramref name="encoded"/> span was not large enough to hold the encoded data.
    /// </returns>
    public static bool TryEncode(ReadOnlySpan<byte> data, Span<byte> encoded, out int written)
    {
        SpanReader<byte> reader = new(data);
        SpanWriter<byte> writer = new(encoded);

        while (reader.TryRead(out byte value))
        {
            int count = reader.AdvancePast(value) + 1;
            while (count > 0)
            {
                if (!writer.TryWrite((byte)Math.Min(count, 0xFF)) || !writer.TryWrite(value))
                {
                    written = writer.Position;
                    return false;
                }

                count -= 0xFF;
            }
        }

        written = writer.Position;
        return true;
    }

    public static bool TryDecode(ReadOnlySpan<byte> encoded, Span<byte> data)
    {
        SpanReader<byte> reader = new(encoded);
        SpanWriter<byte> writer = new(data);

        while (reader.TryRead(out byte count))
        {
            if (!reader.TryRead(out byte value) || !writer.TryWrite(count, value))
            {
                return false;
            }
        }

        return true;
    }
}