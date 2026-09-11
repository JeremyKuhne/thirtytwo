// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Registry;

namespace thirtytwo.perf;

/// <summary>
///  Counts immediate component keys that contain a qualifying string value.
/// </summary>
internal sealed class InstallerComponentRegistryEnumerator : RegistryEnumerator<int>
{
    private static readonly RegistryEnumerationOptions s_options = new()
    {
        RecurseSubKeys = true,
        MaxRecursionDepth = 1,
    };

    private bool _matchedCurrentKey;

    /// <summary>
    ///  Initializes an enumerator for an open Installer Components key.
    /// </summary>
    /// <param name="components">Borrowed handle for the Components key.</param>
    internal InstallerComponentRegistryEnumerator(HKEY components) : base(components, s_options)
    {
    }

    /// <summary>
    ///  Counts qualifying immediate component keys beneath <paramref name="components"/>.
    /// </summary>
    /// <param name="components">Borrowed handle for the Components key.</param>
    /// <returns>The number of qualifying immediate component keys.</returns>
    internal static int Count(HKEY components)
    {
        using InstallerComponentRegistryEnumerator enumerator = new(components);
        int count = 0;
        while (enumerator.MoveNext())
        {
            count += enumerator.Current;
        }

        return count;
    }

    /// <summary>
    ///  Determines whether <paramref name="name"/> is exactly 32 hexadecimal characters.
    /// </summary>
    /// <param name="name">Value name to inspect.</param>
    /// <returns><see langword="true"/> when the name matches; otherwise, <see langword="false"/>.</returns>
    internal static bool IsHexValueName(ReadOnlySpan<char> name)
    {
        if (name.Length != 32)
        {
            return false;
        }

        foreach (char character in name)
        {
            if (!char.IsAsciiHexDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    protected override bool ShouldEnumerateValues(ref RegistryKeyContext key)
    {
        _matchedCurrentKey = false;
        return key.Depth == 1;
    }

    /// <inheritdoc/>
    protected override bool ShouldEnumerateSubKeys(ref RegistryKeyContext key) => key.Depth == 0;

    /// <inheritdoc/>
    protected override bool ShouldIncludeValue(ref RegistryValueEntry value)
    {
        if (_matchedCurrentKey
            || value.Type != REG_VALUE_TYPE.REG_SZ
            || !IsHexValueName(value.Name))
        {
            return false;
        }

        _matchedCurrentKey = true;
        return true;
    }

    /// <inheritdoc/>
    protected override bool ShouldContinueEnumeratingValues(ref RegistryValueEntry value) => !_matchedCurrentKey;

    /// <inheritdoc/>
    protected override bool ShouldIncludeKey(ref RegistryKeyEntry key) => false;

    /// <inheritdoc/>
    protected override int TransformKey(ref RegistryKeyEntry key) => 0;

    /// <inheritdoc/>
    protected override int TransformValue(ref RegistryValueEntry value) => 1;
}