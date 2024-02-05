// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.SystemServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Checker;

internal class Checker2 : MainWindow
{
    private const int DIVISIONS = 5;
    private readonly bool[,] _state = new bool[DIVISIONS, DIVISIONS];
    private int _cxBlock, _cyBlock;

    public Checker2(string title) : base(title: title)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Size:
                _cxBlock = lParam.LOWORD / DIVISIONS;
                _cyBlock = lParam.HIWORD / DIVISIONS;
                return (LRESULT)0;
            case MessageType.SetFocus:
                _ = Interop.ShowCursor(true);
                return (LRESULT)0;
            case MessageType.KillFocus:
                _ = Interop.ShowCursor(false);
                return (LRESULT)0;
            case MessageType.KeyDown:
                Interop.GetCursorPos(out Point point);
                window.ScreenToClient(ref point);
                int x = Math.Max(0, Math.Min(DIVISIONS - 1, point.X / _cxBlock));
                int y = Math.Max(0, Math.Min(DIVISIONS - 1, point.Y / _cyBlock));
                switch ((VIRTUAL_KEY)(ushort)(uint)wParam)
                {
                    case VIRTUAL_KEY.VK_UP:
                        y--;
                        break;
                    case VIRTUAL_KEY.VK_DOWN:
                        y++;
                        break;
                    case VIRTUAL_KEY.VK_LEFT:
                        x--;
                        break;
                    case VIRTUAL_KEY.VK_RIGHT:
                        x++;
                        break;
                    case VIRTUAL_KEY.VK_HOME:
                        x = y = 0;
                        break;
                    case VIRTUAL_KEY.VK_END:
                        x = y = DIVISIONS - 1;
                        break;
                    case VIRTUAL_KEY.VK_RETURN:
                    case VIRTUAL_KEY.VK_SPACE:
                        window.SendMessage(
                            MessageType.LeftButtonDown,
                            (WPARAM)(uint)MODIFIERKEYS_FLAGS.MK_LBUTTON,
                            LPARAM.MAKELPARAM(y * _cyBlock, x * _cxBlock));
                        break;
                }
                x = (x + DIVISIONS) % DIVISIONS;
                y = (y + DIVISIONS) % DIVISIONS;

                point = new Point(x * _cxBlock + _cxBlock / 2, y * _cyBlock + _cyBlock / 2);
                window.ClientToScreen(ref point);
                Interop.SetCursorPos(point.X, point.Y);
                return (LRESULT)0;
            case MessageType.LeftButtonDown:
                x = lParam.LOWORD / _cxBlock;
                y = lParam.HIWORD / _cyBlock;
                if (x < DIVISIONS && y < DIVISIONS)
                {
                    _state[x, y] ^= true;
                    Rectangle rect = Rectangle.FromLTRB
                    (
                        x * _cxBlock,
                        y * _cyBlock,
                        (x + 1) * _cxBlock,
                        (y + 1) * _cyBlock
                    );
                    window.InvalidateRectangle(rect, false);
                }
                else
                {
                    Interop.MessageBeep(0);
                }

                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    for (x = 0; x < DIVISIONS; x++)
                        for (y = 0; y < DIVISIONS; y++)
                        {
                            dc.Rectangle(new Rectangle(x * _cxBlock, y * _cyBlock, (x + 1) * _cxBlock, (y + 1) * _cyBlock));
                            if (_state[x, y])
                            {
                                dc.MoveTo(new Point(x * _cxBlock, y * _cyBlock));
                                dc.LineTo(new Point((x + 1) * _cxBlock, (y + 1) * _cyBlock));
                                dc.MoveTo(new Point(x * _cxBlock, (y + 1) * _cyBlock));
                                dc.LineTo(new Point((x + 1) * _cxBlock, y * _cyBlock));
                            }
                        }
                }

                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
