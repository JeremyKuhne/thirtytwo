// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Delegate for handling Windows messages. Used to forward recieved messages for a window.
/// </summary>
/// <param name="sender">The class invoking the delegate.</param>
/// <returns>
///  The result of processing the message. Return null to indicate that the message has not been handled.
/// </returns>
public delegate LRESULT? WindowsMessageEvent(
    object sender,
    HWND window,
    MessageType message,
    WPARAM wParam,
    LPARAM lParam);