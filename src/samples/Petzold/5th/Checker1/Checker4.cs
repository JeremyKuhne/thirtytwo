// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Checker;

internal class Checker4 : Checker3
{
    private const int DIVISIONS = 5;
    private readonly HWND[,] _hwndChild = new HWND[DIVISIONS, DIVISIONS];
    private int _cxBlock, _cyBlock;
    public static int IdFocus = 0;
    private readonly Checker4Child _childClass = (Checker4Child)(new Checker4Child().Register());

    public Checker4(string title) : base(title)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        int x, y;

        switch (message)
        {
            case MessageType.Create:
                for (x = 0; x < DIVISIONS; x++)
                    for (y = 0; y < DIVISIONS; y++)
                        _hwndChild[x, y] = _childClass.CreateWindow(
                            style: WindowStyles.ChildWindow | WindowStyles.Visible,
                            parentWindow: window,
                            menuHandle: (HMENU)(y << 8 | x));
                return (LRESULT)0;
            case MessageType.Size:
                _cxBlock = lParam.LOWORD / DIVISIONS;
                _cyBlock = lParam.HIWORD / DIVISIONS;
                for (x = 0; x < DIVISIONS; x++)
                    for (y = 0; y < DIVISIONS; y++)
                        _hwndChild[x, y].MoveWindow(
                            new Rectangle(x * _cxBlock, y * _cyBlock, _cxBlock, _cyBlock),
                            repaint: true);
                return (LRESULT)0;
            case MessageType.LeftButtonDown:
                Interop.MessageBeep(MESSAGEBOX_STYLE.MB_OK);
                return (LRESULT)0;
            // On set-focus message, set focus to child window
            case MessageType.SetFocus:
                window.GetDialogItem(IdFocus).SetFocus();
                return (LRESULT)0;
            // On key-down message, possibly change the focus window
            case MessageType.KeyDown:
                x = IdFocus & 0xFF;
                y = IdFocus >> 8;
                switch ((VIRTUAL_KEY)(ushort)(uint)wParam)
                {
                    case VIRTUAL_KEY.VK_UP: y--; break;
                    case VIRTUAL_KEY.VK_DOWN: y++; break;
                    case VIRTUAL_KEY.VK_LEFT: x--; break;
                    case VIRTUAL_KEY.VK_RIGHT: x++; break;
                    case VIRTUAL_KEY.VK_HOME: x = y = 0; break;
                    case VIRTUAL_KEY.VK_END: x = y = DIVISIONS - 1; break;
                    default: return (LRESULT)0;
                }
                x = (x + DIVISIONS) % DIVISIONS;
                y = (y + DIVISIONS) % DIVISIONS;
                IdFocus = y << 8 | x;
                window.GetDialogItem(IdFocus).SetFocus();
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}

internal unsafe class Checker4Child : WindowClass
{
    public Checker4Child() : base(windowExtraBytes: sizeof(void*))
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                window.SetWindowLong(0, 0); // on/off flag
                return (LRESULT)0;
            case MessageType.KeyDown:
                // Send most key presses to the parent window
                if ((VIRTUAL_KEY)(ushort)(uint)wParam != VIRTUAL_KEY.VK_RETURN
                    && (VIRTUAL_KEY)(ushort)(uint)wParam != VIRTUAL_KEY.VK_SPACE)
                {
                    window.GetParent().SendMessage(message, wParam, lParam);
                    return (LRESULT)0;
                }

                // For Return and Space, fall through to toggle the square
                goto case MessageType.LeftButtonDown;
            case MessageType.LeftButtonDown:
                window.SetWindowLong(0, 1 ^ (int)window.GetWindowLong(0));
                window.SetFocus();
                window.Invalidate(false);
                return (LRESULT)0;
            // For focus messages, invalidate the window for repaint
            case MessageType.SetFocus:
                Checker4.IdFocus = (int)window.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_ID);
                // Fall through
                goto case MessageType.KillFocus;
            case MessageType.KillFocus:
                window.Invalidate();
                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    Rectangle rect = window.GetClientRectangle();
                    dc.Rectangle(rect);

                    if (window.GetWindowLong(0) != IntPtr.Zero)
                    {
                        dc.MoveTo(new Point(0, 0));
                        dc.LineTo(new Point(rect.Right, rect.Bottom));
                        dc.MoveTo(new Point(0, rect.Bottom));
                        dc.LineTo(new Point(rect.Right, 0));
                    }

                    // Draw the "focus" rectangle
                    if (window == Interop.GetFocus())
                    {
                        rect.Inflate(rect.Width / -10, rect.Height / -10);

                        dc.SelectObject(StockBrush.Null);
                        using HPEN pen = Interop.CreatePen(PEN_STYLE.PS_DASH, 0, default);
                        dc.SelectObject(pen);
                        dc.Rectangle(rect);
                        dc.SelectObject(StockPen.Black);
                    }
                }

                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
