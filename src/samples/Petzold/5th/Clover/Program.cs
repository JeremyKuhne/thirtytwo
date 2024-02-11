// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Clover;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 5-27, Pages 205-208.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Clover());
}

internal class Clover : MainWindow
{
    private int _cxClient, _cyClient;
    private HRGN _hrgnClip;
    private const double TWO_PI = Math.PI * 2;

    public Clover() : base(title: "Draw a Clover", backgroundColor: Color.White) { }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Size:
                {
                    _cxClient = lParam.LOWORD;
                    _cyClient = lParam.HIWORD;

                    using WaitCursorScope cursorScope = new();

                    _hrgnClip.Dispose();

                    using HRGN region0 = HRGN.FromEllipse(0, _cyClient / 3, _cxClient / 2, 2 * _cyClient / 3);
                    using HRGN region1 = HRGN.FromEllipse(_cxClient / 2, _cyClient / 3, _cxClient, 2 * _cyClient / 3);
                    using HRGN region2 = HRGN.FromEllipse(_cxClient / 3, 0, 2 * _cxClient / 3, _cyClient / 2);
                    using HRGN region3 = HRGN.FromEllipse(_cxClient / 3, _cyClient / 2, 2 * _cxClient / 3, _cyClient);
                    using HRGN region4 = HRGN.CombineRegion(region0, region1, RegionCombineMode.Or);
                    using HRGN region5 = HRGN.CombineRegion(region2, region3, RegionCombineMode.Or);
                    _hrgnClip = HRGN.CombineRegion(region4, region5, RegionCombineMode.Xor);

                    return (LRESULT)0;
                }
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    dc.SetViewportOrigin(new(_cxClient / 2, _cyClient / 2));
                    dc.SelectClippingRegion(_hrgnClip);

                    double fRadius = Hypotenuse(_cxClient / 2.0, _cyClient / 2.0);

                    for (double fAngle = 0.0; fAngle < TWO_PI; fAngle += TWO_PI / 360)
                    {
                        dc.MoveTo(default);
                        dc.LineTo(
                            (int)(fRadius * Math.Cos(fAngle) + 0.5),
                            (int)(-fRadius * Math.Sin(fAngle) + 0.5));
                    }
                }

                return (LRESULT)0;
            case MessageType.Destroy:
                _hrgnClip.Dispose();
                break;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    private static double Hypotenuse(double x, double y) => Math.Sqrt(x * x + y * y);
}
