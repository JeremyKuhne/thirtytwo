// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Identifies the registry operation that failed during enumeration.
/// </summary>
public enum RegistryEnumerationOperation
{
    /// <summary>
    ///  Enumerating a value entry.
    /// </summary>
    EnumerateValue,

    /// <summary>
    ///  Reading data for a matched value entry.
    /// </summary>
    ReadValueData,

    /// <summary>
    ///  Enumerating a subkey entry.
    /// </summary>
    EnumerateSubKey,

    /// <summary>
    ///  Opening a subkey for recursive enumeration.
    /// </summary>
    OpenSubKey,

    /// <summary>
    ///  Closing a subkey after recursive enumeration.
    /// </summary>
    CloseSubKey,
}