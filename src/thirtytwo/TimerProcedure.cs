// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <docs>https://learn.microsoft.com/windows/win32/api/winuser/nc-winuser-timerproc</docs>
public delegate void TimerProcedure(
    HWND hwnd,
    MessageType uMsg,
    nuint idEvent,
    uint dwTime);