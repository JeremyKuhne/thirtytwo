// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;

namespace Bezier;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 5-16, Pages 156-159.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Bezier("Bezier Splines"));
}

internal class Bezier : MainWindow
{
    private readonly Point[] _apt = new Point[4];

    public Bezier(string title) : base(title)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Size:
                int cxClient = lParam.LOWORD;
                int cyClient = lParam.HIWORD;

                _apt[0].X = cxClient / 4;
                _apt[0].Y = cyClient / 2;
                _apt[1].X = cxClient / 2;
                _apt[1].Y = cyClient / 4;
                _apt[2].X = cxClient / 2;
                _apt[2].Y = 3 * cyClient / 4;
                _apt[3].X = 3 * cxClient / 4;
                _apt[3].Y = cyClient / 2;

                return (LRESULT)0;

            case MessageType.LeftButtonDown:
            case MessageType.RightButtonDown:
            case MessageType.MouseMove:
                MouseKey mk = (MouseKey)wParam.LOWORD;
                if ((mk & (MouseKey.LeftButton | MouseKey.RightButton)) != 0)
                {
                    using DeviceContext dc = window.GetDeviceContext();
                    dc.SelectObject(StockPen.White);
                    DrawBezier(dc, _apt);

                    if ((mk & MouseKey.LeftButton) != 0)
                    {
                        _apt[1].X = lParam.LOWORD;
                        _apt[1].Y = lParam.HIWORD;
                    }

                    if ((mk & MouseKey.RightButton) != 0)
                    {
                        _apt[2].X = lParam.LOWORD;
                        _apt[2].Y = lParam.HIWORD;
                    }

                    dc.SelectObject(StockPen.Black);
                    DrawBezier(dc, _apt);
                }

                return (LRESULT)0;

            case MessageType.Paint:
                window.Invalidate(true);
                using (DeviceContext dc = window.BeginPaint())
                {
                    DrawBezier(dc, _apt);
                }

                return (LRESULT)0;
        }

        static void DrawBezier(DeviceContext dc, Point[] apt)
        {
            dc.PolyBezier(apt);
            dc.MoveTo(apt[0]);
            dc.LineTo(apt[1]);
            dc.MoveTo(apt[2]);
            dc.LineTo(apt[3]);
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
