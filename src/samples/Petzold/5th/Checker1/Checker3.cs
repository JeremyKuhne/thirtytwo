// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Checker;

internal class Checker3 : MainWindow
{
    private const int DIVISIONS = 5;
    private readonly HWND[,] _hwndChild = new HWND[DIVISIONS, DIVISIONS];
    private int _cxBlock, _cyBlock;
    private readonly Checker3Child _childClass = (Checker3Child)(new Checker3Child().Register());

    public Checker3(string title) : base(title: title)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                for (int x = 0; x < DIVISIONS; x++)
                    for (int y = 0; y < DIVISIONS; y++)
                        _hwndChild[x, y] = _childClass.CreateWindow(
                            style: WindowStyles.ChildWindow | WindowStyles.Visible,
                            parentWindow: window);
                return (LRESULT)0;
            case MessageType.Size:
                _cxBlock = lParam.LOWORD / DIVISIONS;
                _cyBlock = lParam.HIWORD / DIVISIONS;
                for (int x = 0; x < DIVISIONS; x++)
                    for (int y = 0; y < DIVISIONS; y++)
                        _hwndChild[x, y].MoveWindow(
                            new Rectangle(x * _cxBlock, y * _cyBlock, _cxBlock, _cyBlock),
                            repaint: true);
                return (LRESULT)0;
            case MessageType.LeftButtonDown:
                PInvoke.MessageBeep(MESSAGEBOX_STYLE.MB_OK);
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
