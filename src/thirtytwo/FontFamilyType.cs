// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Used to describe the desired font in a general way.
/// </summary>
public enum FontFamilyType : byte
{
    // FontFamily collides with System.Drawing.FontFamily

    /// <summary>
    ///  Use the default font.
    /// </summary>
    DoNotCare = FONT_FAMILY.FF_DONTCARE,

    /// <summary>
    ///  Proportional with serifs.
    /// </summary>
    Roman = FONT_FAMILY.FF_ROMAN,

    /// <summary>
    ///  Proportional without serifs.
    /// </summary>
    Swiss = FONT_FAMILY.FF_SWISS,

    /// <summary>
    ///  Fixed width.
    /// </summary>
    Modern = FONT_FAMILY.FF_MODERN,

    /// <summary>
    ///  Handwriting style.
    /// </summary>
    Script = FONT_FAMILY.FF_SCRIPT,

    /// <summary>
    ///  Novelty (such as "Old English").
    /// </summary>
    Decorative = FONT_FAMILY.FF_DECORATIVE
}
