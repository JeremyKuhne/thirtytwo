// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Blokout2;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 7-11, Pages 314-317.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Blockout2("Mouse Button & Capture Demo"));
}

internal class Blockout2 : MainWindow
{
    private bool _fBlocking, _fValidBox;
    private Point _ptBeg, _ptEnd, _ptBoxBeg, _ptBoxEnd;

    public Blockout2(string title) : base(title: title)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.LeftButtonDown:
                _ptBeg.X = _ptEnd.X = lParam.LOWORD;
                _ptBeg.Y = _ptEnd.Y = lParam.HIWORD;
                DrawBoxOutline(window, _ptBeg, _ptEnd);
                Interop.SetCapture(window);
                CursorId.Cross.SetCursor();
                _fBlocking = true;
                return (LRESULT)0;
            case MessageType.MouseMove:
                if (_fBlocking)
                {
                    CursorId.Cross.SetCursor();
                    DrawBoxOutline(window, _ptBeg, _ptEnd);
                    _ptEnd.X = lParam.LOWORD;
                    _ptEnd.Y = lParam.HIWORD;
                    DrawBoxOutline(window, _ptBeg, _ptEnd);
                }

                return (LRESULT)0;
            case MessageType.LeftButtonUp:
                if (_fBlocking)
                {
                    DrawBoxOutline(window, _ptBeg, _ptEnd);
                    _ptBoxBeg = _ptBeg;
                    _ptBoxEnd.X = lParam.LOWORD;
                    _ptBoxEnd.Y = lParam.HIWORD;
                    Interop.ReleaseCapture();
                    CursorId.Arrow.SetCursor();
                    _fBlocking = false;
                    _fValidBox = true;
                    window.Invalidate(true);
                }

                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    if (_fValidBox)
                    {
                        dc.SelectObject(StockBrush.Black);
                        dc.Rectangle(_ptBoxBeg.X, _ptBoxBeg.Y, _ptBoxEnd.X, _ptBoxEnd.Y);
                    }
                    if (_fBlocking)
                    {
                        dc.SetRasterOperation(PenMixMode.Not);
                        dc.SelectObject(StockBrush.Null);
                        dc.Rectangle(_ptBeg.X, _ptBeg.Y, _ptEnd.X, _ptEnd.Y);
                    }
                }

                return (LRESULT)0;
        }

        static void DrawBoxOutline(HWND window, Point ptBeg, Point ptEnd)
        {
            using DeviceContext dc = window.GetDeviceContext();
            dc.SetRasterOperation(PenMixMode.Not);
            dc.SelectObject(StockBrush.Null);
            dc.Rectangle(Rectangle.FromLTRB(ptBeg.X, ptBeg.Y, ptEnd.X, ptEnd.Y));
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
