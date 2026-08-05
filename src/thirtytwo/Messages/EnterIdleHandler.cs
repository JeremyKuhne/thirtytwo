// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Messages;

public partial class EnterIdleHandler
{
    public event EnterIdleEvent? IdleEntered;

    public EnterIdleHandler(Window window)
    {
        window.MessageHandler += Window_MessageHandler;
    }

    private LRESULT? Window_MessageHandler(
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

    public static void Attach(Window window, EnterIdleEvent eventHandler)
    {
        EnterIdleHandler handler = new(window);
        handler.IdleEntered += eventHandler;
    }
}