// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public class LayoutBinder
{
    private readonly ILayoutHandler _handler;

    public LayoutBinder(Window window, ILayoutHandler handler)
    {
        _handler = handler;
        window.MessageHandler += Window_MessageHandler;
    }

    private LRESULT? Window_MessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        if (message != MessageType.WindowPositionChanged)
        {
            return null;
        }

        _handler.Layout(window.GetClientRectangle());
        return null;
    }
}