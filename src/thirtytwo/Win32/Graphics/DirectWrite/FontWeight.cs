// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  The font weight enumeration describes common values for degree of blackness or thickness of strokes of characters in a font.
///  [<see cref="DWRITE_FONT_WEIGHT"/>]
/// </summary>
/// <remarks>
///  Font weight values less than 1 or greater than 999 are considered to be invalid, and they are rejected by font API functions.
/// </remarks>
public enum FontWeight : uint
{
    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_THIN"/>
    Thin = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_THIN,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_LIGHT"/>
    ExtraLight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_LIGHT,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_ULTRA_LIGHT"/>
    Light = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_LIGHT,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT"/>
    SemiLight = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_LIGHT,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL"/>
    Normal = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_NORMAL,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_MEDIUM"/>
    Medium = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_MEDIUM,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_BOLD"/>
    SemiBold = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_SEMI_BOLD,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD"/>
    Bold = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BOLD,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_BOLD"/>
    ExtraBold = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_BOLD,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BLACK"/>
    Black = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_BLACK,

    /// <inheritdoc cref="DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_BLACK"/>
    ExtraBlack = DWRITE_FONT_WEIGHT.DWRITE_FONT_WEIGHT_EXTRA_BLACK,
}