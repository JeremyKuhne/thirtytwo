// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Defines the known mode values for the private UxTheme <c>SetPreferredAppMode</c> export.</summary>
internal enum PreferredAppMode
{
    /// <summary>Requests the export's default application mode.</summary>
    Default,

    /// <summary>Requests that the export permit Dark mode without forcing it.</summary>
    AllowDark,

    /// <summary>Requests that the export force Dark mode.</summary>
    ForceDark,

    /// <summary>Requests that the export force Light mode.</summary>
    ForceLight
}