// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <remarks>
///  <inheritdoc cref="Interop.MessageBoxEx(HWND, PCWSTR, PCWSTR, MESSAGEBOX_STYLE, ushort)"/>
/// </remarks>
[Flags]
public enum MessageBoxStyle : uint
{
    /// <summary>
    ///  One OK button, default.
    /// </summary>
    Ok = MESSAGEBOX_STYLE.MB_OK,

    /// <summary>
    ///  OK and Cancel buttons.
    /// </summary>
    OkCancel = MESSAGEBOX_STYLE.MB_OKCANCEL,

    /// <summary>
    ///  Abort, Retry, and Ignore buttons.
    /// </summary>
    AbortRetryIgnore = MESSAGEBOX_STYLE.MB_ABORTRETRYIGNORE,

    /// <summary>
    ///  Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel = MESSAGEBOX_STYLE.MB_YESNOCANCEL,

    /// <summary>
    ///  Yes and No buttons.
    /// </summary>
    YesNo = MESSAGEBOX_STYLE.MB_YESNO,

    /// <summary>
    ///  Retry and Cancel buttons.
    /// </summary>
    RetryCancel = MESSAGEBOX_STYLE.MB_RETRYCANCEL,

    /// <summary>
    ///  Cancel, Try, and Continue buttons.
    /// </summary>
    CancelTryContinue = MESSAGEBOX_STYLE.MB_CANCELTRYCONTINUE,

    /// <summary>
    ///  Stop sign icon.
    /// </summary>
    IconHand = MESSAGEBOX_STYLE.MB_ICONHAND,

    /// <summary>
    ///  Question mark icon. Not recommended.
    /// </summary>
    IconQuestion = MESSAGEBOX_STYLE.MB_ICONQUESTION,

    /// <summary>
    ///  Exclamation point icon.
    /// </summary>
    IconExclamation = MESSAGEBOX_STYLE.MB_ICONEXCLAMATION,

    /// <summary>
    ///  Info icon.
    /// </summary>
    IconAsterisk = MESSAGEBOX_STYLE.MB_ICONASTERISK,

    /// <summary>
    ///  Use the specified user icon.
    /// </summary>
    UserIcon = MESSAGEBOX_STYLE.MB_USERICON,

    /// <summary>
    ///  Exclamation point icon.
    /// </summary>
    IconWarning = MESSAGEBOX_STYLE.MB_ICONWARNING,

    IconError = MESSAGEBOX_STYLE.MB_ICONERROR,

    /// <summary>
    ///  Info icon.
    /// </summary>
    IconInformation = MESSAGEBOX_STYLE.MB_ICONINFORMATION,

    /// <summary>
    ///  Stop sign icon
    /// </summary>
    IconStop = MESSAGEBOX_STYLE.MB_ICONSTOP,

    // MB_DEFBUTTON1              = 0x00000000,

    /// <summary>
    ///  2nd button is the default.
    /// </summary>
    DefaultButton2 = MESSAGEBOX_STYLE.MB_DEFBUTTON2,

    /// <summary>
    ///  3rd button is the default.
    /// </summary>
    DefaultButton3 = MESSAGEBOX_STYLE.MB_DEFBUTTON3,

    /// <summary>
    ///  4th button is the default.
    /// </summary>
    DefaultButton4 = MESSAGEBOX_STYLE.MB_DEFBUTTON4,

    // MB_APPLMODAL               = 0x00000000,

    /// <summary>
    ///  Puts the dialog topmost.
    /// </summary>
    SystemModal = MESSAGEBOX_STYLE.MB_SYSTEMMODAL,

    /// <summary>
    ///  Disable all threads top-level windows if no window handle is specified.
    /// </summary>
    TaskModal = MESSAGEBOX_STYLE.MB_TASKMODAL,

    /// <summary>
    ///  Adds a Help button. When clicked or F1 is pressed, help message is sent to the owner.
    /// </summary>
    Help = MESSAGEBOX_STYLE.MB_HELP,

    /// <summary>
    ///  Ensures no button initially has focus.
    /// </summary>
    NoFocus = MESSAGEBOX_STYLE.MB_NOFOCUS,

    /// <summary>
    ///  Message box is set to the foreground.
    /// </summary>
    SetForeground = MESSAGEBOX_STYLE.MB_SETFOREGROUND,

    /// <summary>
    ///  If the current input desktop is not the default desktop, MessageBox does not return until the user
    ///  switches to the default desktop.
    /// </summary>
    DefaultDesktopOnly = MESSAGEBOX_STYLE.MB_DEFAULT_DESKTOP_ONLY,

    /// <summary>
    ///  Messagebox is created with the topmost style.
    /// </summary>
    TopMost = MESSAGEBOX_STYLE.MB_TOPMOST,

    /// <summary>
    ///  Text is right justified.
    /// </summary>
    Right = MESSAGEBOX_STYLE.MB_RIGHT,

    /// <summary>
    ///  Display right-to-left.
    /// </summary>
    RightToLeftReading = MESSAGEBOX_STYLE.MB_RTLREADING,

    /// <summary>
    ///  Used for services to send notifications.
    /// </summary>
    ServiceNotification = MESSAGEBOX_STYLE.MB_SERVICE_NOTIFICATION,
}
