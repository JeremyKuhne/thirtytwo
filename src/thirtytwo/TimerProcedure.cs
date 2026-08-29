// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Represents a callback function that processes timer events raised by USER32.
/// </summary>
/// <param name="hwnd">
///  The associated window handle, or <see cref="HWND.Null"/> when the timer was created without a window.
/// </param>
/// <param name="uMsg">The message identifier, typically <see cref="MessageType.Timer"/>.</param>
/// <param name="idEvent">The timer identifier returned by timer creation.</param>
/// <param name="dwTime">The system tick count, in milliseconds, when USER32 dispatched the callback.</param>
public delegate void TimerProcedure(
    HWND hwnd,
    MessageType uMsg,
    nuint idEvent,
    uint dwTime);