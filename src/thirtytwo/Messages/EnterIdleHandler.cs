// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Messages;

/// <summary>
///  Routes <see cref="MessageType.EnterIdle"/> notifications from a window message stream.
/// </summary>
public partial class EnterIdleHandler
{
    /// <summary>
    ///  Raised when the owning window receives <see cref="MessageType.EnterIdle"/>.
    /// </summary>
    public event EnterIdleEvent? IdleEntered;

    /// <summary>
    ///  Subscribes a new idle handler to <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The window whose message stream is observed.</param>
    public EnterIdleHandler(Window window)
    {
        window.MessageHandler += WindowMessageHandler;
    }

    private LRESULT? WindowMessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.EnterIdle:
                IdleEntered?.Invoke(wParam == PInvoke.MSGF_DIALOGBOX, (HWND)lParam);
                break;
        }

        return null;
    }

    /// <summary>
    ///  Creates and attaches an <see cref="EnterIdleHandler"/> that invokes <paramref name="eventHandler"/>.
    /// </summary>
    /// <param name="window">The window whose message stream is observed.</param>
    /// <param name="eventHandler">The callback invoked for each <see cref="MessageType.EnterIdle"/> message.</param>
    public static void Attach(Window window, EnterIdleEvent eventHandler)
    {
        EnterIdleHandler handler = new(window);
        handler.IdleEntered += eventHandler;
    }
}