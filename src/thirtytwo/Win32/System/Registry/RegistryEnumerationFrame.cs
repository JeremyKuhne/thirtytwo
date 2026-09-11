// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Stores traversal state for one open registry key.
/// </summary>
/// <param name="key">Open key handle.</param>
/// <param name="ownsKey">Whether the enumerator owns the key handle.</param>
/// <param name="pathLength">Relative path length for the key.</param>
/// <param name="depth">Key depth.</param>
internal struct RegistryEnumerationFrame(HKEY key, bool ownsKey, int pathLength, int depth)
{
    /// <summary>
    ///  Open key handle.
    /// </summary>
    internal HKEY Key = key;

    /// <summary>
    ///  Whether the enumerator owns the key handle.
    /// </summary>
    internal bool OwnsKey = ownsKey;

    /// <summary>
    ///  Relative path length for the key.
    /// </summary>
    internal int PathLength = pathLength;

    /// <summary>
    ///  Key depth.
    /// </summary>
    internal int Depth = depth;

    /// <summary>
    ///  Index of the next value to enumerate.
    /// </summary>
    internal uint ValueIndex;

    /// <summary>
    ///  Index of the next subkey to enumerate.
    /// </summary>
    internal uint SubKeyIndex;

    /// <summary>
    ///  Current enumeration phase.
    /// </summary>
    internal RegistryEnumerationPhase Phase;
}