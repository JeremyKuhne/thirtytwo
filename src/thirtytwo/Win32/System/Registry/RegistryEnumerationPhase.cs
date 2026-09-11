// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Registry;

/// <summary>
///  Identifies the current phase for one registry traversal frame.
/// </summary>
internal enum RegistryEnumerationPhase : byte
{
    /// <summary>
    ///  The key has not started enumeration.
    /// </summary>
    Start,

    /// <summary>
    ///  The key's values are being enumerated.
    /// </summary>
    Values,

    /// <summary>
    ///  The key is about to start subkey enumeration.
    /// </summary>
    StartSubKeys,

    /// <summary>
    ///  The key's immediate subkeys are being enumerated.
    /// </summary>
    SubKeys,

    /// <summary>
    ///  The key has finished enumeration.
    /// </summary>
    Finished,
}