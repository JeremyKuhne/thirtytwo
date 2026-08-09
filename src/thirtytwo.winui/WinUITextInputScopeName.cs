// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies a keyboard or input-method hint for a text editor.</summary>
public enum WinUITextInputScopeName
{
    /// <summary>Uses the default input scope.</summary>
    Default = 0,
    /// <summary>Accepts a URL.</summary>
    Url = 1,
    /// <summary>Accepts an SMTP email address.</summary>
    EmailSmtpAddress = 5,
    /// <summary>Accepts a person's full name.</summary>
    PersonalFullName = 7,
    /// <summary>Accepts a currency amount and symbol.</summary>
    CurrencyAmountAndSymbol = 20,
    /// <summary>Accepts a currency amount.</summary>
    CurrencyAmount = 21,
    /// <summary>Accepts a month number.</summary>
    DateMonthNumber = 23,
    /// <summary>Accepts a day number.</summary>
    DateDayNumber = 24,
    /// <summary>Accepts a year.</summary>
    DateYear = 25,
    /// <summary>Accepts decimal digits.</summary>
    Digits = 28,
    /// <summary>Accepts a number.</summary>
    Number = 29,
    /// <summary>Accepts a password.</summary>
    Password = 31,
    /// <summary>Accepts a telephone number.</summary>
    TelephoneNumber = 32,
    /// <summary>Accepts a telephone country code.</summary>
    TelephoneCountryCode = 33,
    /// <summary>Accepts a telephone area code.</summary>
    TelephoneAreaCode = 34,
    /// <summary>Accepts a local telephone number.</summary>
    TelephoneLocalNumber = 35,
    /// <summary>Accepts an hour value.</summary>
    TimeHour = 37,
    /// <summary>Accepts minutes or seconds.</summary>
    TimeMinutesOrSeconds = 38,
    /// <summary>Accepts full-width numbers.</summary>
    NumberFullWidth = 39,
    /// <summary>Accepts half-width alphanumeric text.</summary>
    AlphanumericHalfWidth = 40,
    /// <summary>Accepts full-width alphanumeric text.</summary>
    AlphanumericFullWidth = 41,
    /// <summary>Accepts Hiragana text.</summary>
    Hiragana = 44,
    /// <summary>Accepts half-width Katakana text.</summary>
    KatakanaHalfWidth = 45,
    /// <summary>Accepts full-width Katakana text.</summary>
    KatakanaFullWidth = 46,
    /// <summary>Accepts Hanja text.</summary>
    Hanja = 47,
    /// <summary>Accepts half-width Hangul text.</summary>
    HangulHalfWidth = 48,
    /// <summary>Accepts full-width Hangul text.</summary>
    HangulFullWidth = 49,
    /// <summary>Accepts a search query.</summary>
    Search = 50,
    /// <summary>Accepts a formula.</summary>
    Formula = 51,
    /// <summary>Accepts incremental-search text.</summary>
    SearchIncremental = 52,
    /// <summary>Accepts half-width Chinese text.</summary>
    ChineseHalfWidth = 53,
    /// <summary>Accepts full-width Chinese text.</summary>
    ChineseFullWidth = 54,
    /// <summary>Accepts native-script text.</summary>
    NativeScript = 55,
    /// <summary>Accepts general text.</summary>
    Text = 57,
    /// <summary>Accepts chat text.</summary>
    Chat = 58,
    /// <summary>Accepts a name or telephone number.</summary>
    NameOrPhoneNumber = 59,
    /// <summary>Accepts an email name or address.</summary>
    EmailNameOrAddress = 60,
    /// <summary>Accepts map-search text.</summary>
    Maps = 62,
    /// <summary>Accepts a numeric password.</summary>
    NumericPassword = 63,
    /// <summary>Accepts a numeric PIN.</summary>
    NumericPin = 64,
    /// <summary>Accepts an alphanumeric PIN.</summary>
    AlphanumericPin = 65,
    /// <summary>Accepts a formula number.</summary>
    FormulaNumber = 67,
    /// <summary>Accepts chat text without emoji suggestions.</summary>
    ChatWithoutEmoji = 68
}