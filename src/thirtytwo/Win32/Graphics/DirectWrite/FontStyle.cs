// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  The font style enumeration describes the slope style of a font face, such as Normal, Italic or Oblique.
///  [<see cref="DWRITE_FONT_STYLE"/>]
/// </summary>
public enum FontStyle : uint
{
    /// <inheritdoc cref="DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL"/>
    Normal = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL,

    /// <inheritdoc cref="DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_OBLIQUE"/>
    Oblique = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_OBLIQUE,

    /// <inheritdoc cref="DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC"/>
    Italic = DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC
}