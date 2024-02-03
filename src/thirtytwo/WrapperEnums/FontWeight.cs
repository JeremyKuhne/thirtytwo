// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum FontWeight : uint
{
    /// <summary>
    ///  Don't care.
    /// </summary>
    DoNotCare = FONT_WEIGHT.FW_DONTCARE,

    /// <summary>
    ///  Thin.
    /// </summary>
    Thin = FONT_WEIGHT.FW_THIN,

    /// <summary>
    ///  Extra Light.
    /// </summary>
    ExtraLight = FONT_WEIGHT.FW_EXTRALIGHT,

    /// <summary>
    ///  Light.
    /// </summary>
    Light = FONT_WEIGHT.FW_LIGHT,

    /// <summary>
    ///  Normal.
    /// </summary>
    Normal = FONT_WEIGHT.FW_NORMAL,

    /// <summary>
    ///  Medium.
    /// </summary>
    Medium = FONT_WEIGHT.FW_MEDIUM,

    /// <summary>
    ///  Semibold.
    /// </summary>
    Semibold = FONT_WEIGHT.FW_SEMIBOLD,

    /// <summary>
    ///  Bold.
    /// </summary>
    Bold = FONT_WEIGHT.FW_BOLD,

    /// <summary>
    ///  Extra Bold.
    /// </summary>
    ExtraBold = FONT_WEIGHT.FW_EXTRABOLD,

    /// <summary>
    ///  Heavy.
    /// </summary>
    Heavy = FONT_WEIGHT.FW_HEAVY,

    /// <summary>
    ///  Ultra Light.
    /// </summary>
    UltraLight = FONT_WEIGHT.FW_ULTRALIGHT,

    /// <summary>
    ///  Regular.
    /// </summary>
    Regular = FONT_WEIGHT.FW_REGULAR,

    /// <summary>
    ///  Demibold.
    /// </summary>
    Demibold = FONT_WEIGHT.FW_DEMIBOLD,

    /// <summary>
    ///  Ultra Bold.
    /// </summary>
    UltraBold = FONT_WEIGHT.FW_ULTRABOLD,

    /// <summary>
    ///  Black.
    /// </summary>
    Black = FONT_WEIGHT.FW_BLACK
}