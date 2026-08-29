// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Specifies character set values for a logical font.
/// </summary>
public enum CharacterSet : byte
{
    /// <summary>
    ///  Windows ANSI character set.
    /// </summary>
    Ansi = FONT_CHARSET.ANSI_CHARSET,

    /// <summary>
    ///  Character set selected from the system's default Windows ANSI code page.
    /// </summary>
    Default = FONT_CHARSET.DEFAULT_CHARSET,

    /// <summary>
    ///  Windows symbol character set.
    /// </summary>
    Symbol = FONT_CHARSET.SYMBOL_CHARSET,

    /// <summary>
    ///  Character set selected from the current system Macintosh code page.
    /// </summary>
    Mac = FONT_CHARSET.MAC_CHARSET,

    /// <summary>
    ///  Shift-JIS Japanese character set.
    /// </summary>
    ShiftJis = FONT_CHARSET.SHIFTJIS_CHARSET,

    /// <summary>
    ///  Korean Hangul character set.
    /// </summary>
    Hangul = FONT_CHARSET.HANGUL_CHARSET,

    /// <summary>
    ///  Simplified Chinese GB2312 character set.
    /// </summary>
    GB2312 = FONT_CHARSET.GB2312_CHARSET,

    /// <summary>
    ///  Traditional Chinese Big5 character set.
    /// </summary>
    ChineseBig5 = FONT_CHARSET.CHINESEBIG5_CHARSET,

    /// <summary>
    ///  System-dependent original equipment manufacturer (OEM) character set.
    /// </summary>
    Oem = FONT_CHARSET.OEM_CHARSET,

    /// <summary>
    ///  Korean Johab character set.
    /// </summary>
    Johab = FONT_CHARSET.JOHAB_CHARSET,

    /// <summary>
    ///  Hebrew character set.
    /// </summary>
    Hebrew = FONT_CHARSET.HEBREW_CHARSET,

    /// <summary>
    ///  Arabic character set.
    /// </summary>
    Arabic = FONT_CHARSET.ARABIC_CHARSET,

    /// <summary>
    ///  Greek character set.
    /// </summary>
    Greek = FONT_CHARSET.GREEK_CHARSET,

    /// <summary>
    ///  Turkish character set.
    /// </summary>
    Turkish = FONT_CHARSET.TURKISH_CHARSET,

    /// <summary>
    ///  Vietnamese character set.
    /// </summary>
    Vietnamese = FONT_CHARSET.VIETNAMESE_CHARSET,

    /// <summary>
    ///  Baltic character set.
    /// </summary>
    Baltic = FONT_CHARSET.BALTIC_CHARSET,

    /// <summary>
    ///  Thai character set.
    /// </summary>
    Thai = FONT_CHARSET.THAI_CHARSET,

    /// <summary>
    ///  Eastern European character set.
    /// </summary>
    EasternEurope = FONT_CHARSET.EASTEUROPE_CHARSET,

    /// <summary>
    ///  Cyrillic character set.
    /// </summary>
    Russian = FONT_CHARSET.RUSSIAN_CHARSET
}