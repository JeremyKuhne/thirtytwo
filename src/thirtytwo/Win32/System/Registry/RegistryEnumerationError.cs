// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Provides contextual information about an error encountered during registry enumeration.
/// </summary>
/// <remarks>
///  Span properties reference reusable enumerator storage and are valid only for the duration of the callback.
/// </remarks>
public readonly ref struct RegistryEnumerationError
{
    /// <summary>
    ///  Initializes a registry enumeration error.
    /// </summary>
    /// <param name="errorCode">Native Win32 error code.</param>
    /// <param name="operation">Operation that failed.</param>
    /// <param name="entryKind">Entry kind, or <see langword="null"/> when no entry was identified.</param>
    /// <param name="key">Borrowed current or parent key handle.</param>
    /// <param name="keyRelativePath">Current key path relative to the enumeration root.</param>
    /// <param name="entryName">Identified entry name, if any.</param>
    /// <param name="depth">Current or identified entry depth.</param>
    internal RegistryEnumerationError(
        WIN32_ERROR errorCode,
        RegistryEnumerationOperation operation,
        RegistryEntryKind? entryKind,
        HKEY key,
        ReadOnlySpan<char> keyRelativePath,
        ReadOnlySpan<char> entryName,
        int depth)
    {
        ErrorCode = errorCode;
        Operation = operation;
        EntryKind = entryKind;
        Key = key;
        KeyRelativePath = keyRelativePath;
        EntryName = entryName;
        Depth = depth;
    }

    /// <summary>
    ///  Gets the native Win32 error code.
    /// </summary>
    public WIN32_ERROR ErrorCode { get; }

    /// <summary>
    ///  Gets the operation that failed.
    /// </summary>
    public RegistryEnumerationOperation Operation { get; }

    /// <summary>
    ///  Gets the entry kind, or <see langword="null"/> when no entry was identified.
    /// </summary>
    public RegistryEntryKind? EntryKind { get; }

    /// <summary>
    ///  Gets the borrowed current or parent key handle.
    /// </summary>
    public HKEY Key { get; }

    /// <summary>
    ///  Gets the current key path relative to the enumeration root.
    /// </summary>
    public ReadOnlySpan<char> KeyRelativePath { get; }

    /// <summary>
    ///  Gets the identified entry name, if any.
    /// </summary>
    public ReadOnlySpan<char> EntryName { get; }

    /// <summary>
    ///  Gets the current or identified entry depth.
    /// </summary>
    public int Depth { get; }
}