// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;

namespace Windows.Win32.System.Registry;

/// <summary>
///  Provides an ephemeral view of a registry value discovered during enumeration.
/// </summary>
/// <remarks>
///  Span properties and <see cref="Strings"/> reference reusable enumerator storage and are valid only for the
///  duration of the callback.
/// </remarks>
public readonly ref struct RegistryValueEntry
{
    /// <summary>
    ///  Initializes a registry value entry.
    /// </summary>
    /// <param name="key">Borrowed handle for the containing key.</param>
    /// <param name="keyRelativePath">Containing key path relative to the enumeration root.</param>
    /// <param name="name">Value name.</param>
    /// <param name="depth">Containing key depth.</param>
    /// <param name="type">Registry value type.</param>
    /// <param name="dataLength">Value data length in bytes.</param>
    /// <param name="hasData">Whether value data was read.</param>
    /// <param name="data">Raw value data when it was read.</param>
    internal RegistryValueEntry(
        HKEY key,
        ReadOnlySpan<char> keyRelativePath,
        ReadOnlySpan<char> name,
        int depth,
        REG_VALUE_TYPE type,
        int dataLength,
        bool hasData,
        ReadOnlySpan<byte> data)
    {
        Key = key;
        KeyRelativePath = keyRelativePath;
        Name = name;
        Depth = depth;
        Type = type;
        DataLength = dataLength;
        HasData = hasData;
        Data = data;
    }

    /// <summary>
    ///  Gets the borrowed handle for the key containing this value.
    /// </summary>
    public HKEY Key { get; }

    /// <summary>
    ///  Gets the containing key path relative to the enumeration root.
    /// </summary>
    public ReadOnlySpan<char> KeyRelativePath { get; }

    /// <summary>
    ///  Gets the value name. The default value has an empty name.
    /// </summary>
    public ReadOnlySpan<char> Name { get; }

    /// <summary>
    ///  Gets the depth of the containing key, with the enumeration root at depth zero.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    ///  Gets the registry value type.
    /// </summary>
    public REG_VALUE_TYPE Type { get; }

    /// <summary>
    ///  Gets the value data length in bytes.
    /// </summary>
    public int DataLength { get; }

    /// <summary>
    ///  Gets whether value data was requested and read.
    /// </summary>
    public bool HasData { get; }

    /// <summary>
    ///  Gets the raw value data when <see cref="HasData"/> is <see langword="true"/>.
    /// </summary>
    public ReadOnlySpan<byte> Data { get; }

    /// <summary>
    ///  Gets an allocation-free enumerator over string data in this value.
    /// </summary>
    public StringEnumerator Strings => new(Data, Type, HasData);

    /// <summary>
    ///  Attempts to read a 32-bit registry integer.
    /// </summary>
    /// <param name="value">
    ///  Receives the decoded value, or zero when this method returns <see langword="false"/>.
    /// </param>
    /// <returns>
    ///  <see langword="true"/> for an exact-width <c>REG_DWORD</c> or <c>REG_DWORD_BIG_ENDIAN</c> value;
    ///  otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryReadUInt32(out uint value)
    {
        if (HasData && Data.Length == sizeof(uint))
        {
            if (Type == REG_VALUE_TYPE.REG_DWORD)
            {
                value = BinaryPrimitives.ReadUInt32LittleEndian(Data);
                return true;
            }

            if (Type == REG_VALUE_TYPE.REG_DWORD_BIG_ENDIAN)
            {
                value = BinaryPrimitives.ReadUInt32BigEndian(Data);
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>
    ///  Attempts to read a 64-bit registry integer, widening a 32-bit registry integer when necessary.
    /// </summary>
    /// <param name="value">
    ///  Receives the decoded value, or zero when this method returns <see langword="false"/>.
    /// </param>
    /// <returns>
    ///  <see langword="true"/> for an exact-width <c>REG_QWORD</c>, <c>REG_DWORD</c>, or
    ///  <c>REG_DWORD_BIG_ENDIAN</c> value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryReadUInt64(out ulong value)
    {
        if (TryReadUInt32(out uint dword))
        {
            value = dword;
            return true;
        }

        if (HasData && Type == REG_VALUE_TYPE.REG_QWORD && Data.Length == sizeof(ulong))
        {
            value = BinaryPrimitives.ReadUInt64LittleEndian(Data);
            return true;
        }

        value = 0;
        return false;
    }
}