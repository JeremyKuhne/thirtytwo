// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Binds a <see cref="Window"/> to an <see cref="ILayoutHandler"/> and listens for window position changes
///  to trigger layout updates.
/// </summary>
public class LayoutBinder
{
    private readonly ILayoutHandler _handler;

    /// <summary>
    ///  Initializes a new instance of the <see cref="LayoutBinder"/> class and attaches the layout handler
    ///  to the specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The window to bind to layout changes.</param>
    /// <param name="handler">The layout handler to invoke on layout events.</param>
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
        if (message == MessageType.WindowPositionChanged)
        {
            _handler.Layout(window.GetClientRectangle());
        }

        // Return null to indicate that the message was not handled.
        return null;
    }
}