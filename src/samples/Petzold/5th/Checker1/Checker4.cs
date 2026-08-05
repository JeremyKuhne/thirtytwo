// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
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
                PInvoke.MessageBeep(MESSAGEBOX_STYLE.MB_OK);
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
