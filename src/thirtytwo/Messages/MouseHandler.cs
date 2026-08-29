// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Messages;

/// <summary>
///  Routes mouse-related window messages to events and overridable callbacks.
/// </summary>
public class MouseHandler : IMouseMessageHandler
{
    private readonly Window _attachedWindow;

    /// <summary>
    ///  Raised when a mouse button release message is received.
    /// </summary>
    public event MouseMessageEvent? MouseUp;

    /// <summary>
    ///  Raised when a mouse move message is received.
    /// </summary>
    public event MouseMessageEvent? MouseMove;

    /// <summary>
    ///  Raised when a mouse button press message is received.
    /// </summary>
    public event MouseMessageEvent? MouseDown;

    /// <summary>
    ///  Subscribes a mouse handler to the specified window.
    /// </summary>
    /// <param name="window">The window whose message stream is observed.</param>
    public MouseHandler(Window window)
    {
        window.MessageHandler += WindowMessageHandler;
        _attachedWindow = window;
    }

    private unsafe LRESULT? WindowMessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.MouseMove:
                OnMouseMove(*(POINTS*)&lParam, (MouseKey)(uint)wParam);
                break;
            case MessageType.LeftButtonUp:
                OnButtonUp(*(POINTS*)&lParam, MouseButton.Left, (MouseKey)(uint)wParam);
                break;
            case MessageType.RightButtonUp:
                OnButtonUp(*(POINTS*)&lParam, MouseButton.Right, (MouseKey)(uint)wParam);
                break;
            case MessageType.MiddleButtonUp:
                OnButtonUp(*(POINTS*)&lParam, MouseButton.Middle, (MouseKey)(uint)wParam);
                break;
            case MessageType.ExtraButtonUp:
                OnButtonUp(*(POINTS*)&lParam, MouseButton.X1, (MouseKey)(uint)wParam);
                break;
            case MessageType.LeftButtonDown:
                OnButtonDown(*(POINTS*)&lParam, MouseButton.Left, (MouseKey)(uint)wParam);
                break;
            case MessageType.RightButtonDown:
                OnButtonDown(*(POINTS*)&lParam, MouseButton.Right, (MouseKey)(uint)wParam);
                break;
            case MessageType.MiddleButtonDown:
                OnButtonDown(*(POINTS*)&lParam, MouseButton.Middle, (MouseKey)(uint)wParam);
                break;
            case MessageType.ExtraButtonDown:
                OnButtonDown(*(POINTS*)&lParam, MouseButton.X1, (MouseKey)(uint)wParam);
                break;
        }

        return null;
    }

    /// <inheritdoc/>
    public virtual void OnMouseMove(Point position, MouseKey mouseState)
        => MouseMove?.Invoke(_attachedWindow, position, 0, mouseState);

    /// <inheritdoc/>
    public virtual void OnButtonDown(Point position, MouseButton button, MouseKey mouseState)
        => MouseDown?.Invoke(_attachedWindow, position, 0, mouseState);

    /// <inheritdoc/>
    public virtual void OnButtonUp(Point position, MouseButton button, MouseKey mouseState)
        => MouseUp?.Invoke(_attachedWindow,  position, button, mouseState);
}