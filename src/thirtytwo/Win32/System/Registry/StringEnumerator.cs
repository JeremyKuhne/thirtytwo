// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.Win32.System.Registry;

/// <summary>
///  Enumerates string data from a registry value without allocating.
/// </summary>
/// <remarks>
///  Returned spans reference ephemeral registry value data and are valid only for the duration of the value callback.
/// </remarks>
public ref struct StringEnumerator
{
    private const byte Done = 0;
    private const byte SingleString = 1;
    private const byte MultipleStrings = 2;

    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private byte _state;

    /// <summary>
    ///  Initializes an enumerator for the supplied registry value data.
    /// </summary>
    /// <param name="data">Raw registry value data.</param>
    /// <param name="type">Registry value type.</param>
    /// <param name="hasData">Whether the raw data was read.</param>
    internal StringEnumerator(ReadOnlySpan<byte> data, REG_VALUE_TYPE type, bool hasData)
    {
        _remaining = default;
        _current = default;
        _state = Done;

        if (!hasData)
        {
            return;
        }

        _state = type switch
        {
            REG_VALUE_TYPE.REG_SZ or REG_VALUE_TYPE.REG_EXPAND_SZ or REG_VALUE_TYPE.REG_LINK => SingleString,
            REG_VALUE_TYPE.REG_MULTI_SZ => MultipleStrings,
            _ => Done,
        };

        if (_state != Done)
        {
            int completeByteLength = data.Length & ~1;
            _remaining = MemoryMarshal.Cast<byte, char>(data[..completeByteLength]);
        }
    }

    /// <summary>
    ///  Gets this instance for pattern-based enumeration.
    /// </summary>
    /// <returns>A copy positioned at the same point in the string data.</returns>
    public readonly StringEnumerator GetEnumerator() => this;

    /// <summary>
    ///  Gets the string found by the most recent successful call to <see cref="MoveNext"/>.
    /// </summary>
    public readonly ReadOnlySpan<char> Current => _current;

    /// <summary>
    ///  Advances to the next string.
    /// </summary>
    /// <returns><see langword="true"/> when another string was found; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext()
    {
        if (_state == SingleString)
        {
            _state = Done;
            _current = !_remaining.IsEmpty && _remaining[^1] == '\0'
                ? _remaining[..^1]
                : _remaining;
            _remaining = default;
            return true;
        }

        if (_state != MultipleStrings || _remaining.IsEmpty)
        {
            _current = default;
            return false;
        }

        int terminator = _remaining.IndexOf('\0');
        if (terminator == 0)
        {
            _state = Done;
            _remaining = default;
            _current = default;
            return false;
        }

        if (terminator < 0)
        {
            _state = Done;
            _current = _remaining;
            _remaining = default;
            return true;
        }

        _current = _remaining[..terminator];
        _remaining = _remaining[(terminator + 1)..];
        return true;
    }
}