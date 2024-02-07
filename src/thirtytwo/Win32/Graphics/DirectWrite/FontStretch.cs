// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  The font stretch enumeration describes relative change from the normal aspect ratio as specified by a font
///  designer for the glyphs in a font. [<see cref="DWRITE_FONT_STRETCH"/>]
/// </summary>
/// <remarks>
///  Values less than 1 or greater than 9 are considered to be invalid, and they are rejected by font API functions.
/// </remarks>
public enum FontStretch : uint
{
    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_UNDEFINED"/>
    Undefined = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_UNDEFINED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED"/>
    UltraCondensed = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_CONDENSED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXTRA_CONDENSED"/>
    ExtraCondensed = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXTRA_CONDENSED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_CONDENSED"/>
    Condensed = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_CONDENSED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_SEMI_CONDENSED"/>
    SemiCondensed = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_SEMI_CONDENSED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL"/>
    Normal = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_SEMI_EXPANDED"/>
    SemiExpanded = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_SEMI_EXPANDED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXPANDED"/>
    Expanded = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXPANDED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXTRA_EXPANDED"/>
    ExtraExpanded = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_EXTRA_EXPANDED,

    /// <inheritdoc cref="DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_EXPANDED"/>
    UltraExpanded = DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_ULTRA_EXPANDED
}