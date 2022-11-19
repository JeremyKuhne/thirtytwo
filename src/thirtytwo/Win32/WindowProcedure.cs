// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32;

/// <summary>
///  Callback that processes messages sent to a window. [WindowProc]
/// </summary>
/// <docs>https://learn.microsoft.com/windows/win32/api/winuser/nc-winuser-wndproc</docs>
public delegate LRESULT WindowProcedure(
    HWND hwnd,
    uint uMsg,
    WPARAM wParam,
    LPARAM lParam);
