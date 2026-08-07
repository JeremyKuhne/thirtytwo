// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal static class BoundedTextReader
{
    private const int ReadBufferLength = 4096;

    internal static async Task<(string Text, bool Truncated)> ReadAsync(StreamReader reader, int maximumLength)
    {
        if (maximumLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength));
        }

        char[] readBuffer = new char[ReadBufferLength];
        char[] retained = new char[maximumLength];
        int retainedLength = 0;
        bool truncated = false;

        while (true)
        {
            int read = await reader.ReadAsync(readBuffer).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            int available = retained.Length - retainedLength;
            int copyLength = Math.Min(read, available);
            if (copyLength > 0)
            {
                readBuffer.AsSpan(0, copyLength).CopyTo(retained.AsSpan(retainedLength));
                retainedLength += copyLength;
            }

            truncated |= copyLength != read;
        }

        return (new string(retained, 0, retainedLength), truncated);
    }
}
