// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum ShowWindowCommand : uint
{
    /// <summary>
    ///  Hides the window, activating another window. (SW_HIDE)
    /// </summary>
    Hide = SHOW_WINDOW_CMD.SW_HIDE,

    /// <summary>
    ///  Activates and displays a window, restoring it to its original size and position.
    ///  Use this when displaying a window for the first time. (SW_SHOWNORMAL)
    /// </summary>
    Normal = SHOW_WINDOW_CMD.SW_SHOWNORMAL,

    /// <summary>
    ///  Activates and displays minimized. (SW_SHOWMINIMIZED)
    /// </summary>
    Minimized = SHOW_WINDOW_CMD.SW_SHOWMINIMIZED,

    /// <summary>
    ///  Activates and displays maximized. (SW_SHOWMAXIMIZED)
    /// </summary>
    Maximized = SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED,

    /// <summary>
    ///  Same as Normal, but doesn't activate. (SW_SHOWNOACTIVATE)
    /// </summary>
    NormalNoActivate = SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE,

    /// <summary>
    ///  Activates and shows in its current size position. (SW_SHOW)
    /// </summary>
    Show = SHOW_WINDOW_CMD.SW_SHOW,

    /// <summary>
    ///  Minimizes and activates the next window in the Z order. (SW_MINIMIZE)
    /// </summary>
    Minimize = SHOW_WINDOW_CMD.SW_MINIMIZE,

    /// <summary>
    ///  Same as Minimize, but doesn't activate. (SW_SHOWMINNOACTIVE)
    /// </summary>
    MinimizeNoActivate = SHOW_WINDOW_CMD.SW_SHOWMINNOACTIVE,

    /// <summary>
    ///  Same as Show, but doesn't activate. (SW_SHOWNA)
    /// </summary>
    NoActivate = SHOW_WINDOW_CMD.SW_SHOWNA,

    /// <summary>
    ///  Activates and displays, restoring size and position if minimized or maximized (SW_RESTORE).
    /// </summary>
    Restore = SHOW_WINDOW_CMD.SW_RESTORE,

    /// <summary>
    ///  Shows according to the default value used to create the process (in STARTUPINFO). (SW_SHOWDEFAULT)
    /// </summary>
    Default = SHOW_WINDOW_CMD.SW_SHOWDEFAULT,

    /// <summary>
    ///  Minimizes a window, even if the thread is not responding. (SW_FORCEMINIMIZE)
    /// </summary>
    ForceMinimize = SHOW_WINDOW_CMD.SW_FORCEMINIMIZE,
}