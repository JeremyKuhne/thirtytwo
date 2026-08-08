// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Specifies how an application chooses its light or dark color palette.</summary>
public enum ApplicationColorMode
{
    /// <summary>
    ///  Follows the current Windows application color preference and High Contrast changes.
    /// </summary>
    System,

    /// <summary>Uses the dark application palette.</summary>
    Dark,

    /// <summary>Uses the light application palette.</summary>
    Light
}