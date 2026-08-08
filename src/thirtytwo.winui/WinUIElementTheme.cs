// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies the theme requested for a hosted WinUI element.</summary>
public enum WinUIElementTheme
{
    /// <summary>Uses the theme inherited from the WinUI application.</summary>
    Default,

    /// <summary>Uses the light theme.</summary>
    Light,

    /// <summary>Uses the dark theme.</summary>
    Dark
}