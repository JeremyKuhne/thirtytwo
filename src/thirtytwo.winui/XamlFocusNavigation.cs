// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI;

/// <summary>
///  Coordinates tab direction and native sibling traversal at a XAML island boundary.
/// </summary>
/// <remarks>
///  <para>
///   Direction state is stored per thread to bridge focus transitions that leave XAML and continue through native
///   dialog-tab navigation.
///  </para>
/// </remarks>
internal static class XamlFocusNavigation
{
    [ThreadStatic]
    private static bool? t_pendingForward;

    /// <summary>
    ///  Gets the pending direction for entering XAML focus, defaulting to keyboard state when no direction is pending.
    /// </summary>
    /// <returns><see langword="true"/> when focus should move forward; otherwise, <see langword="false"/>.</returns>
    internal static bool EntryIsForward
        => t_pendingForward ?? !IsShiftPressed();

    /// <summary>
    ///  Determines whether the Shift key is currently pressed.
    /// </summary>
    /// <returns><see langword="true"/> when Shift is pressed; otherwise, <see langword="false"/>.</returns>
    internal static bool IsShiftPressed()
        => PInvoke.GetKeyState((int)VirtualKey.Shift) < 0;

    /// <summary>
    ///  Attempts to move focus to the next native dialog-tab sibling of <paramref name="current"/>.
    /// </summary>
    /// <param name="current">The current island or host window handle.</param>
    /// <param name="forward">
    ///  <see langword="true"/> to move to the next sibling; <see langword="false"/> to move to the previous sibling.
    /// </param>
    /// <returns>
    ///  <see langword="true"/> when focus changed to a different native window; otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool TryMoveFocus(HWND current, bool forward)
    {
        Window? currentWindow = Window.FromHandle(current, walkParents: true);
        if (currentWindow is null || currentWindow.Handle.IsNull)
        {
            return false;
        }

        HWND parent = PInvoke.GetParent(currentWindow.Handle);
        if (parent.IsNull)
        {
            return false;
        }

        HWND next = PInvoke.GetNextDlgTabItem(parent, currentWindow.Handle, !forward);
        if (next.IsNull || next == currentWindow.Handle)
        {
            return false;
        }

        HWND previousFocus = PInvoke.GetFocus();
        bool? previousPendingForward = t_pendingForward;
        t_pendingForward = forward;
        try
        {
            next.SetFocus();
            return PInvoke.GetFocus() != previousFocus;
        }
        finally
        {
            t_pendingForward = previousPendingForward;
        }
    }
}