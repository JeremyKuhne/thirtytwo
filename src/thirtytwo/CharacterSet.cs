﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum CharacterSet : byte
{
    Ansi = FONT_CHARSET.ANSI_CHARSET,
    Default = FONT_CHARSET.DEFAULT_CHARSET,
    Symbol = FONT_CHARSET.SYMBOL_CHARSET,
    Mac = FONT_CHARSET.MAC_CHARSET,
    ShiftJis = FONT_CHARSET.SHIFTJIS_CHARSET,
    Hangul = FONT_CHARSET.HANGUL_CHARSET,
    GB2312 = FONT_CHARSET.GB2312_CHARSET,
    ChineseBig5 = FONT_CHARSET.CHINESEBIG5_CHARSET,
    Oem = FONT_CHARSET.OEM_CHARSET,
    Johab = FONT_CHARSET.JOHAB_CHARSET,
    Hebrew = FONT_CHARSET.HEBREW_CHARSET,
    Arabic = FONT_CHARSET.ARABIC_CHARSET,
    Greek = FONT_CHARSET.GREEK_CHARSET,
    Turkish = FONT_CHARSET.TURKISH_CHARSET,
    Vietnamese = FONT_CHARSET.VIETNAMESE_CHARSET,
    Baltic = FONT_CHARSET.BALTIC_CHARSET,
    Thai = FONT_CHARSET.THAI_CHARSET,
    EasternEurope = FONT_CHARSET.EASTEUROPE_CHARSET,
    Russian = FONT_CHARSET.RUSSIAN_CHARSET
}