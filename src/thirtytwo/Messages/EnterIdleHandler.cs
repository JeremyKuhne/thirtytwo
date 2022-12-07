// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Messages;

public class EnterIdleHandler
{
    public event EnterIdleEvent? IdleEntered;

    public EnterIdleHandler(Window window)
    {
        window.MessageHandler += Window_MessageHandler;
    }

    private unsafe LRESULT? Window_MessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.EnterIdle:
                IdleEntered?.Invoke(wParam == Interop.MSGF_DIALOGBOX, (HWND)lParam);
                break;
        }

        return null;
    }

    /// <summary>
    ///  Delegate for processing idle events.
    /// </summary>
    /// <param name="isDialog"><see langword="true"/> if dialog is displayed, otherwise a menu is displayed.</param>
    /// <param name="handle">Dialog handle if is <see langword="true"/>, or parent window handle.</param>
    public delegate void EnterIdleEvent(bool isDialog, HWND handle);

    public static void Attach(Window window, EnterIdleEvent eventHandler)
    {
        EnterIdleHandler handler = new(window);
        handler.IdleEntered += eventHandler;
    }
}