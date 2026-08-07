// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal sealed class BoundedTextReader
{
    private const int ReadBufferLength = 4096;
    private readonly Lock _lock = new();
    private readonly char[] _retained;
    private int _retainedLength;
    private bool _truncated;

    internal BoundedTextReader(int maximumLength)
    {
        if (maximumLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength));
        }

        _retained = new char[maximumLength];
    }

    internal string Text
    {
        get
        {
            lock (_lock)
            {
                return new string(_retained, 0, _retainedLength);
            }
        }
    }

    internal bool Truncated
    {
        get
        {
            lock (_lock)
            {
                return _truncated;
            }
        }
    }

    internal async Task ReadAsync(StreamReader reader, CancellationToken cancellationToken = default)
    {
        char[] readBuffer = new char[ReadBufferLength];

        try
        {
            while (true)
            {
                int read = await reader.ReadAsync(readBuffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                lock (_lock)
                {
                    int available = _retained.Length - _retainedLength;
                    int copyLength = Math.Min(read, available);
                    if (copyLength > 0)
                    {
                        readBuffer.AsSpan(0, copyLength).CopyTo(_retained.AsSpan(_retainedLength));
                        _retainedLength += copyLength;
                    }

                    _truncated |= copyLength != read;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
