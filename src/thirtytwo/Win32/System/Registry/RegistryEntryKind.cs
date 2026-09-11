// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Identifies a kind of registry entry.
/// </summary>
public enum RegistryEntryKind
{
    /// <summary>
    ///  A registry key.
    /// </summary>
    Key,

    /// <summary>
    ///  A registry value.
    /// </summary>
    Value,
}