// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Specifies the standard command buttons shown by a task dialog.
/// </summary>
[Flags]
public enum TaskDialogButtons
{
    /// <summary>
    ///  Shows the OK button.
    /// </summary>
    Ok = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_OK_BUTTON,

    /// <summary>
    ///  Shows the Yes button.
    /// </summary>
    Yes = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_YES_BUTTON,

    /// <summary>
    ///  Shows the No button.
    /// </summary>
    No = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_NO_BUTTON,

    /// <summary>
    ///  Shows the Cancel button.
    /// </summary>
    Cancel = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CANCEL_BUTTON,

    /// <summary>
    ///  Shows the Retry button.
    /// </summary>
    Retry = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_RETRY_BUTTON,

    /// <summary>
    ///  Shows the Close button.
    /// </summary>
    Close = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CLOSE_BUTTON
}