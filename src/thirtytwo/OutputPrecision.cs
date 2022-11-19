// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum OutputPrecision : byte
{
    /// <summary>
    ///  Default.
    /// </summary>
    Default = FONT_OUTPUT_PRECISION.OUT_DEFAULT_PRECIS,

    /// <summary>
    ///  Returned when enumerating raster fonts.
    /// </summary>
    String = FONT_OUTPUT_PRECISION.OUT_STRING_PRECIS,

    /// <summary>
    ///  Not used.
    /// </summary>
    Character = FONT_OUTPUT_PRECISION.OUT_CHARACTER_PRECIS,

    /// <summary>
    ///  Returned when enumerating vector fonts.
    /// </summary>
    Stroke = FONT_OUTPUT_PRECISION.OUT_STROKE_PRECIS,

    /// <summary>
    ///  Choose a TrueType font when there are multiple fonts of the same name.
    /// </summary>
    TrueType = FONT_OUTPUT_PRECISION.OUT_TT_PRECIS,

    /// <summary>
    ///  Choose a Device font when there are multiple fonts of the same name.
    /// </summary>
    Device = FONT_OUTPUT_PRECISION.OUT_DEVICE_PRECIS,

    /// <summary>
    ///  Choose a raster font when there are multiple fonts of the same name.
    /// </summary>
    Raster = FONT_OUTPUT_PRECISION.OUT_RASTER_PRECIS,

    /// <summary>
    ///  Choose from TrueType fonts only (if any are installed).
    /// </summary>
    TrueTypeOnly = FONT_OUTPUT_PRECISION.OUT_TT_ONLY_PRECIS,

    /// <summary>
    ///  Choose from TrueType and other outline based fonts.
    /// </summary>
    Outline = FONT_OUTPUT_PRECISION.OUT_OUTLINE_PRECIS,

    /// <summary>
    ///  Prefer TrueType and other outline based fonts.
    /// </summary>
    ScreenOutline = FONT_OUTPUT_PRECISION.OUT_SCREEN_OUTLINE_PRECIS,

    /// <summary>
    ///  Choose from PostScript fonts only (if any are installed).
    /// </summary>
    PostScriptOnly = FONT_OUTPUT_PRECISION.OUT_PS_ONLY_PRECIS
}
