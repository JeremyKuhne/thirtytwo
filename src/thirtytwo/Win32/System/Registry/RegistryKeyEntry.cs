// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Windows.Win32.System.Registry;

/// <summary>
///  Provides an ephemeral view of a subkey discovered during enumeration.
/// </summary>
/// <remarks>
///  Span properties reference reusable enumerator storage and are valid only for the duration of the callback.
/// </remarks>
public readonly ref struct RegistryKeyEntry
{
    /// <summary>
    ///  Initializes a registry key entry.
    /// </summary>
    /// <param name="parentKey">Borrowed handle for the parent key.</param>
    /// <param name="name">Subkey name.</param>
    /// <param name="relativePath">Subkey path relative to the enumeration root.</param>
    /// <param name="depth">Subkey depth.</param>
    /// <param name="lastWriteTime">Subkey last-write time.</param>
    internal RegistryKeyEntry(
        HKEY parentKey,
        ReadOnlySpan<char> name,
        ReadOnlySpan<char> relativePath,
        int depth,
        FILETIME lastWriteTime)
    {
        ParentKey = parentKey;
        Name = name;
        RelativePath = relativePath;
        Depth = depth;
        LastWriteTime = lastWriteTime;
    }

    /// <summary>
    ///  Gets the borrowed handle for the parent key.
    /// </summary>
    public HKEY ParentKey { get; }

    /// <summary>
    ///  Gets the subkey name.
    /// </summary>
    public ReadOnlySpan<char> Name { get; }

    /// <summary>
    ///  Gets the subkey path relative to the enumeration root.
    /// </summary>
    public ReadOnlySpan<char> RelativePath { get; }

    /// <summary>
    ///  Gets the subkey depth.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    ///  Gets the subkey last-write time.
    /// </summary>
    public FILETIME LastWriteTime { get; }
}