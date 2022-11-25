// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Messages;

public class MouseHandler : IMouseMessageHandler
{
    private readonly Window _attachedWindow;
    public event MouseMessageEvent? MouseUp;
    public event MouseMessageEvent? MouseMove;
    public event MouseMessageEvent? MouseDown;

    public MouseHandler(Window window)
    {
        window.MessageHandler += Window_MessageHandler;
        _attachedWindow = window;
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

    public virtual void OnMouseMove(Point position, MouseKey mouseState)
        => MouseMove?.Invoke(_attachedWindow, position, 0, mouseState);

    public virtual void OnButtonDown(Point position, MouseButton button, MouseKey mouseState)
        => MouseDown?.Invoke(_attachedWindow, position, 0, mouseState);

    public virtual void OnButtonUp(Point position, MouseButton button, MouseKey mouseState)
        => MouseUp?.Invoke(_attachedWindow,  position, button, mouseState);
}