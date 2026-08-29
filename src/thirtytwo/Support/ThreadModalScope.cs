// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Temporarily disables visible, enabled windows on the current UI thread to emulate modal behavior.
/// </summary>
/// <remarks>
///  Disposing the scope re-enables all disabled windows and attempts to restore the previously active and focused windows.
/// </remarks>
public readonly ref struct ThreadModalScope
{
    private readonly List<HWND> _windows;
    private readonly HWND _focusedWindow;
    private readonly HWND _activeWindow;

    /// <summary>
    ///  Captures current focus and active windows, then disables eligible thread windows.
    /// </summary>
    public ThreadModalScope()
    {
        _focusedWindow = PInvoke.GetFocus();
        _activeWindow = PInvoke.GetActiveWindow();

        List<HWND> windows = [];

        Application.EnumerateThreadWindows((HWND hwnd) =>
        {
            if (PInvoke.IsWindowVisible(hwnd) && PInvoke.IsWindowEnabled(hwnd))
            {
                PInvoke.EnableWindow(hwnd, false);
                windows.Add(hwnd);
            }

            return true;
        });

        _windows = windows;
    }

    /// <summary>
    ///  Re-enables windows disabled by this scope and restores active and focused windows when available.
    /// </summary>
    public void Dispose()
    {
        foreach (HWND hwnd in _windows)
        {
            PInvoke.EnableWindow(hwnd, true);
        }

        if (!_activeWindow.IsNull)
        {
            PInvoke.SetActiveWindow(_activeWindow);
        }

        if (!_focusedWindow.IsNull)
        {
            PInvoke.SetFocus(_focusedWindow);
        }
    }
}