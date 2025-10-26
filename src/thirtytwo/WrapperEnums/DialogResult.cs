// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <remarks>
///  <inheritdoc cref="Interop.MessageBoxEx(HWND, PCWSTR, PCWSTR, MESSAGEBOX_STYLE, ushort)"/>
///  <inheritdoc cref="Interop.TaskDialogIndirect(TASKDIALOGCONFIG*, int*, int*, BOOL*)"/>
/// </remarks>
public enum DialogResult : int
{
    Ok = MESSAGEBOX_RESULT.IDOK,
    Cancel = MESSAGEBOX_RESULT.IDCANCEL,
    Abort = MESSAGEBOX_RESULT.IDABORT,
    Retry = MESSAGEBOX_RESULT.IDRETRY,
    Ignore = MESSAGEBOX_RESULT.IDIGNORE,
    Yes = MESSAGEBOX_RESULT.IDYES,
    No = MESSAGEBOX_RESULT.IDNO,
    Close = MESSAGEBOX_RESULT.IDCLOSE,
    Help = MESSAGEBOX_RESULT.IDHELP,
    TryAgain = MESSAGEBOX_RESULT.IDTRYAGAIN,
    Continue = MESSAGEBOX_RESULT.IDCONTINUE,
    Timeout = MESSAGEBOX_RESULT.IDTIMEOUT
}