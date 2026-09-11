// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Provides an ephemeral view of a key whose contents are being enumerated.
/// </summary>
/// <remarks>
///  Span properties reference reusable enumerator storage and are valid only for the duration of the callback.
/// </remarks>
public readonly ref struct RegistryKeyContext
{
    /// <summary>
    ///  Initializes a registry key context.
    /// </summary>
    /// <param name="key">Borrowed handle for the key.</param>
    /// <param name="relativePath">Key path relative to the enumeration root.</param>
    /// <param name="depth">Key depth, with the enumeration root at depth zero.</param>
    internal RegistryKeyContext(HKEY key, ReadOnlySpan<char> relativePath, int depth)
    {
        Key = key;
        RelativePath = relativePath;
        Depth = depth;
    }

    /// <summary>
    ///  Gets the borrowed handle for this key.
    /// </summary>
    public HKEY Key { get; }

    /// <summary>
    ///  Gets the key path relative to the enumeration root.
    /// </summary>
    public ReadOnlySpan<char> RelativePath { get; }

    /// <summary>
    ///  Gets the key depth, with the enumeration root at depth zero.
    /// </summary>
    public int Depth { get; }
}