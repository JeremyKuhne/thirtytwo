// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Identifies the result returned from a message box or task dialog.
/// </summary>
/// <remarks>
///  Values correspond to the standard button identifiers returned by <c>MessageBoxEx</c> and
///  <c>TaskDialogIndirect</c>.
/// </remarks>
public enum DialogResult : int
{
    /// <summary>
    ///  The OK button was selected.
    /// </summary>
    Ok = MESSAGEBOX_RESULT.IDOK,

    /// <summary>
    ///  The Cancel button was selected.
    /// </summary>
    Cancel = MESSAGEBOX_RESULT.IDCANCEL,

    /// <summary>
    ///  The Abort button was selected.
    /// </summary>
    Abort = MESSAGEBOX_RESULT.IDABORT,

    /// <summary>
    ///  The Retry button was selected.
    /// </summary>
    Retry = MESSAGEBOX_RESULT.IDRETRY,

    /// <summary>
    ///  The Ignore button was selected.
    /// </summary>
    Ignore = MESSAGEBOX_RESULT.IDIGNORE,

    /// <summary>
    ///  The Yes button was selected.
    /// </summary>
    Yes = MESSAGEBOX_RESULT.IDYES,

    /// <summary>
    ///  The No button was selected.
    /// </summary>
    No = MESSAGEBOX_RESULT.IDNO,

    /// <summary>
    ///  The Close button was selected.
    /// </summary>
    Close = MESSAGEBOX_RESULT.IDCLOSE,

    /// <summary>
    ///  The Help button was selected.
    /// </summary>
    Help = MESSAGEBOX_RESULT.IDHELP,

    /// <summary>
    ///  The Try Again button was selected.
    /// </summary>
    TryAgain = MESSAGEBOX_RESULT.IDTRYAGAIN,

    /// <summary>
    ///  The Continue button was selected.
    /// </summary>
    Continue = MESSAGEBOX_RESULT.IDCONTINUE,

    /// <summary>
    ///  The dialog timed out before a button was selected.
    /// </summary>
    Timeout = MESSAGEBOX_RESULT.IDTIMEOUT
}