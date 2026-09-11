// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Specifies options for registry enumeration.
/// </summary>
public sealed class RegistryEnumerationOptions
{
    private int _maxRecursionDepth = int.MaxValue;

    /// <summary>
    ///  Gets or sets whether accepted subkeys are recursively enumerated.
    /// </summary>
    public bool RecurseSubKeys { get; set; }

    /// <summary>
    ///  Gets or sets the maximum subkey depth to enter, with the enumeration root at depth zero.
    /// </summary>
    public int MaxRecursionDepth
    {
        get => _maxRecursionDepth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _maxRecursionDepth = value;
        }
    }

    /// <summary>
    ///  Gets or sets whether access-denied errors are ignored.
    /// </summary>
    public bool IgnoreInaccessible { get; set; }
}