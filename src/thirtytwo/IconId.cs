// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum IconId : uint
{
    // Unfortunately the metadata defines most of these as PCWSTR, which has CsWin32 defining them as statics.

    /// <summary>
    ///  Application icon. (IDI_APPLICATION)
    /// </summary>
    Application = 32512,

    /// <summary>
    ///  Error icon (hand). (IDI_HAND)
    /// </summary>
    Hand = 32513,

    /// <summary>
    ///  Question mark icon. (IDI_QUESTION)
    /// </summary>
    Question = 32514,

    /// <summary>
    ///  Warning icon (exclamation point). (IDI_EXCLAMATION)
    /// </summary>
    Exclamation = 32515,

    /// <summary>
    ///  Information icon (asterisk). (IDI_ASTERISK)
    /// </summary>
    Asterisk = 32516,

    /// <summary>
    ///  Application icon (Windows logo on Windows 2000). (IDI_WINLOGO)
    /// </summary>
    WindowsLogo = 32517,

    /// <summary>
    ///  Security shield icon. (IDI_SHIELD)
    /// </summary>
    Shield = 32518,

    /// <summary>
    ///  Warning icon (exclamation point). (IDI_WARNING)
    /// </summary>
    Warning = Interop.IDI_WARNING,

    /// <summary>
    ///  Error icon (hand). (IDI_ERROR)
    /// </summary>
    Error = Interop.IDI_ERROR,

    /// <summary>
    ///  Information icon (asterisk). (IDI_INFORMATION)
    /// </summary>
    Information = Interop.IDI_INFORMATION
}
