// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum FontQuality : byte
{
    /// <summary>
    ///  Appearance doesn't matter. Will smooth if SPI_GETFONTSMOOTHING is true.
    /// </summary>
    Default = FONT_QUALITY.DEFAULT_QUALITY,

    /// <summary>
    ///  Can scale raster fonts to get desired font size. Bold, italic, etc. may be synthesized if needed.
    /// </summary>
    Draft = FONT_QUALITY.DRAFT_QUALITY,

    /// <summary>
    ///  Will not scale raster fonts, picks closes size. Bold, italic, etc. may be synthesized if needed.
    /// </summary>
    Proof = FONT_QUALITY.PROOF_QUALITY,

    /// <summary>
    ///  Do not antialias.
    /// </summary>
    NonAntialiased = FONT_QUALITY.NONANTIALIASED_QUALITY,

    /// <summary>
    ///  Antialiased if the font supports it and it isn't too small or large.
    /// </summary>
    Antialiased = FONT_QUALITY.ANTIALIASED_QUALITY,

    /// <summary>
    ///  Text is rendered using ClearType antialiasing if possible.
    /// </summary>
    ClearType = FONT_QUALITY.CLEARTYPE_QUALITY,

    /// <summary>
    ///  Text is rendered using ClearType if possible, glyph widths may vary from non-antialised
    ///  width to avoid distortion.
    /// </summary>
    ClearTypeNatural = (byte)Interop.CLEARTYPE_NATURAL_QUALITY
}