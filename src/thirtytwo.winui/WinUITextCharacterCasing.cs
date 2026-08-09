// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies automatic character casing for a WinUI text editor.</summary>
public enum WinUITextCharacterCasing
{
    /// <summary>Preserves entered character casing.</summary>
    Normal = 0,

    /// <summary>Converts entered characters to lowercase.</summary>
    Lower = 1,

    /// <summary>Converts entered characters to uppercase.</summary>
    Upper = 2
}