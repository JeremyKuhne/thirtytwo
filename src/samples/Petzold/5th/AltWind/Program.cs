// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;

namespace AltWind;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 5-21, Pages 171-173.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.Run(new AltWind(), "Alternate and Winding Fill Modes");
    }
}

public class AltWind : WindowClass
{
    private readonly Point[] _aptFigure =
    [
        new(10, 70),
        new(50, 70),
        new(50, 10),
        new(90, 10),
        new(90, 50),
        new(30, 50),
        new(30, 90),
        new(70, 90),
        new(70, 30),
        new(10, 30)
    ];

    private int _cxClient, _cyClient;

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Size:
                _cxClient = lParam.LOWORD;
                _cyClient = lParam.HIWORD;
                return (LRESULT)0;
            case MessageType.Paint:
                Span<Point> apt = stackalloc Point[10];
                using (DeviceContext dc = window.BeginPaint())
                {
                    dc.SelectObject(StockBrush.Gray);
                    for (int i = 0; i < 10; i++)
                    {
                        apt[i].X = _cxClient * _aptFigure[i].X / 200;
                        apt[i].Y = _cyClient * _aptFigure[i].Y / 100;
                    }

                    dc.SetPolyFillMode(PolyFillMode.Alternate);
                    dc.Polygon(apt);

                    for (int i = 0; i < 10; i++)
                    {
                        apt[i].X += _cxClient / 2;
                    }
                    dc.SetPolyFillMode(PolyFillMode.Winding);
                    dc.Polygon(apt);
                }

                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
