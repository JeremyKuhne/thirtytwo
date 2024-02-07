// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Alignment of paragraph text along the reading direction axis relative to
///  the leading and trailing edge of the layout box. [<see cref="DWRITE_TEXT_ALIGNMENT"/>]
/// </summary>
public enum TextAlignment : uint
{
    /// <inheritdoc cref="DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING"/>
    Leading = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING,

    /// <inheritdoc cref="DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING"/>
    Trailing = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING,

    /// <inheritdoc cref="DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER"/>
    Center = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER,

    /// <inheritdoc cref="DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED"/>
    Justified = DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_JUSTIFIED
}