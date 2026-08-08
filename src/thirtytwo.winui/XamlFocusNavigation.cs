// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.WinUI;

/// <summary>
///  Coordinates tab direction and native sibling traversal at a XAML island boundary.
/// </summary>
internal static class XamlFocusNavigation
{
    [ThreadStatic]
    private static bool? t_pendingForward;

    internal static bool EntryIsForward
        => t_pendingForward ?? !IsShiftPressed();

    internal static bool IsShiftPressed()
        => PInvoke.GetKeyState((int)VirtualKey.Shift) < 0;

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