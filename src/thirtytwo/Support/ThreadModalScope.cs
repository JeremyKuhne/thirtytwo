// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public readonly ref struct ThreadModalScope
{
    private readonly List<HWND> _windows;
    private readonly HWND _focusedWindow;
    private readonly HWND _activeWindow;

    public ThreadModalScope()
    {
        _focusedWindow = Interop.GetFocus();
        _activeWindow = Interop.GetActiveWindow();

        List<HWND> windows = new();

        Application.EnumerateThreadWindows((HWND hwnd) =>
        {
            if (Interop.IsWindowVisible(hwnd) && Interop.IsWindowEnabled(hwnd))
            {
                Interop.EnableWindow(hwnd, false);
                windows.Add(hwnd);
            }

            return true;
        });

        _windows = windows;
    }

    public void Dispose()
    {
        foreach (HWND hwnd in _windows)
        {
            Interop.EnableWindow(hwnd, true);
        }

        if (!_activeWindow.IsNull)
        {
            Interop.SetActiveWindow(_activeWindow);
        }

        if (!_focusedWindow.IsNull)
        {
            Interop.SetFocus(_focusedWindow);
        }
    }
}