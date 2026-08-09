// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Controls which position, size, activation, and redraw changes <c>SetWindowPos</c> applies.</summary>
[Flags]
public enum WindowPositionFlags : uint
{
    /// <summary>Applies the requested bounds, z-order, activation, and redraw changes.</summary>
    None = 0,

    /// <summary>Retains the current size and ignores the requested width and height.</summary>
    NoSize = SET_WINDOW_POS_FLAGS.SWP_NOSIZE,

    /// <summary>Retains the current position and ignores the requested X and Y coordinates.</summary>
    NoMove = SET_WINDOW_POS_FLAGS.SWP_NOMOVE,

    /// <summary>Retains the current z-order and ignores the requested insertion position.</summary>
    NoZOrder = SET_WINDOW_POS_FLAGS.SWP_NOZORDER,

    /// <summary>Suppresses repainting caused by the position change.</summary>
    NoRedraw = SET_WINDOW_POS_FLAGS.SWP_NOREDRAW,

    /// <summary>Does not activate the window.</summary>
    NoActivate = SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE,

    /// <summary>Recalculates the nonclient area even when the window size does not change.</summary>
    FrameChanged = SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED,

    /// <summary>Shows the window.</summary>
    ShowWindow = SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW,

    /// <summary>Hides the window.</summary>
    HideWindow = SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW,

    /// <summary>Discards the current client-area contents instead of preserving them after the change.</summary>
    NoCopyBits = SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS,

    /// <summary>Does not change the owner window's z-order position.</summary>
    NoOwnerZOrder = SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER,

    /// <summary>Prevents the window from receiving <c>WM_WINDOWPOSCHANGING</c>.</summary>
    NoSendChanging = SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING,

    /// <summary>Prevents generation of <c>WM_SYNCPAINT</c>.</summary>
    DeferErase = SET_WINDOW_POS_FLAGS.SWP_DEFERERASE,

    /// <summary>
    ///  Posts the request when the calling thread and window thread use different input queues. The target HWND and
    ///  any sibling HWND supplied to <c>SetWindowPosition</c> must remain valid until the owning thread applies it.
    /// </summary>
    AsyncWindowPosition = SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS
}