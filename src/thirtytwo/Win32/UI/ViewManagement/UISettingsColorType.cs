// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.ViewManagement;

/// <summary>
///  Identifies a semantic color exposed by UISettings.
/// </summary>
internal enum UISettingsColorType
{
    /// <summary>
    ///  Background color.
    /// </summary>
    Background,

    /// <summary>
    ///  Foreground color.
    /// </summary>
    Foreground,

    /// <summary>
    ///  Darkest accent variant.
    /// </summary>
    AccentDark3,

    /// <summary>
    ///  Second darkest accent variant.
    /// </summary>
    AccentDark2,

    /// <summary>
    ///  Third darkest accent variant.
    /// </summary>
    AccentDark1,

    /// <summary>
    ///  Base accent color.
    /// </summary>
    Accent,

    /// <summary>
    ///  First lighter accent variant.
    /// </summary>
    AccentLight1,

    /// <summary>
    ///  Second lighter accent variant.
    /// </summary>
    AccentLight2,

    /// <summary>
    ///  Lightest accent variant.
    /// </summary>
    AccentLight3,

    /// <summary>
    ///  Complementary color.
    /// </summary>
    Complement
}