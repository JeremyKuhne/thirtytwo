// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// The GDI+ variant works, but is far from optimized. Creating and disposing a Graphics object every second
// is clearly not optimal. Keeping things aligned with the sample to show the direct mapping.
//
#define GDIPLUS

#if GDIPLUS
using Windows.Win32.Graphics.GdiPlus;
using GdiPlusPen = Windows.Win32.Graphics.GdiPlus.Pen;
using GdiPlusBrush = Windows.Win32.Graphics.GdiPlus.Brush;
#endif

using Windows;
using System.Drawing;
using Windows.Win32.Foundation;
using Windows.Win32;
using Point = System.Drawing.Point;

namespace Clock;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 8-5, Pages 346-350.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Clock("Analog Clock"));
}

internal class Clock : MainWindow
{
#if GDIPLUS
    private readonly GdiPlusPen _blackPen = new(Color.Black);
    private static readonly GdiPlusBrush s_blackBrush = new SolidBrush(Color.Black);
    private readonly GdiPlusBrush _whiteBrush = new SolidBrush(Color.White);
#endif

    public Clock(string title) : base(title) { }

    private void SetIsotropic(DeviceContext hdc)
    {
        hdc.SetMappingMode(MappingMode.Isotropic);
        hdc.SetWindowExtents(new Size(1000, 1000));
        hdc.SetViewportExtents(new Size(_clientSize.Width / 2, -_clientSize.Height / 2));
        hdc.SetViewportOrigin(new Point(_clientSize.Width / 2, _clientSize.Height / 2));
    }

    private static void RotatePoint(Point[] pt, int iNum, int iAngle)
    {
        for (int i = 0; i < iNum; i++)
        {
            pt[i] = new Point
            (
                (int)(pt[i].X * Math.Cos(TWOPI * iAngle / 360) + pt[i].Y * Math.Sin(TWOPI * iAngle / 360)),
                (int)(pt[i].Y * Math.Cos(TWOPI * iAngle / 360) - pt[i].X * Math.Sin(TWOPI * iAngle / 360))
            );
        }
    }

    private static void DrawClock(DeviceContext dc)
    {
        int iAngle;
        Point[] pt = new Point[3];

#if GDIPLUS
        using var graphics = new Graphics(dc);
        graphics.SetSmoothingMode(SmoothingMode.SmoothingModeHighQuality);
#else
        dc.SelectObject(StockBrush.Black);
#endif
        for (iAngle = 0; iAngle < 360; iAngle += 6)
        {
            pt[0].X = 0;
            pt[0].Y = 900;
            RotatePoint(pt, 1, iAngle);
            pt[2].X = pt[2].Y = iAngle % 5 != 0 ? 33 : 100;
            pt[0].X -= pt[2].X / 2;
            pt[0].Y -= pt[2].Y / 2;
            pt[1].X = pt[0].X + pt[2].X;
            pt[1].Y = pt[0].Y + pt[2].Y;
#if GDIPLUS
            graphics.FillEllipse(s_blackBrush, pt[0].X, pt[0].Y, pt[1].X - pt[0].X, pt[1].Y - pt[0].Y);
#else
            dc.Ellipse(Rectangle.FromLTRB(pt[0].X, pt[0].Y, pt[1].X, pt[1].Y));
#endif
        }
    }

    private void DrawHands(DeviceContext dc, SYSTEMTIME time, bool erase = false, bool drawHourAndMinuteHands = true)
    {
        int[] handAngles =
        [
            (time.wHour * 30) % 360 + time.wMinute / 2,
            time.wMinute * (360 / 60),
            time.wSecond * (360 / 60)
        ];

        Point[][] handPoints =
        [
            [new(0, -150), new(100, 0), new(0, 600), new(-100, 0), new(0, -150)],
            [new(0, -200), new(50, 0), new(0, 800), new(-50, 0), new(0, -200)],
            [new(0, 0), new(0, 0), new(0, 0), new(0, 0), new(0, 800)]
        ];

#if GDIPLUS
        using var graphics = new Graphics(dc);
        if (erase)
        {
            graphics.FillEllipse(_whiteBrush, -830, -830, 1660, 1660);
            return;
        }

        graphics.SetSmoothingMode(SmoothingMode.SmoothingModeHighQuality);
#else
        dc.SelectObject(erase ? StockPen.White : StockPen.Black);
#endif

        for (int i = drawHourAndMinuteHands ? 0 : 2; i < 3; i++)
        {
            RotatePoint(handPoints[i], 5, handAngles[i]);

#if GDIPLUS
            graphics.DrawLines(_blackPen, handPoints[i]);
#else
            dc.Polyline(handPoints[i]);
#endif
        }
    }

    private const int ID_TIMER = 1;
    private const double TWOPI = Math.PI * 2;
    private Size _clientSize;
    private SYSTEMTIME _previousTime;

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Create:
                window.SetTimer(ID_TIMER, 1000);
                Interop.GetLocalTime(out _previousTime);
                return (LRESULT)0;
            case MessageType.Size:
                _clientSize = new Message.Size(wParam, lParam).NewSize;
                return (LRESULT)0;
            case MessageType.Timer:
                Interop.GetLocalTime(out SYSTEMTIME time);
                bool drawAllHands = time.wHour != _previousTime.wHour || time.wMinute != _previousTime.wMinute;
                using (DeviceContext dc = window.GetDeviceContext())
                {
                    SetIsotropic(dc);
                    DrawHands(dc, _previousTime, erase: true, drawAllHands);
                    DrawHands(dc, time);
                }
                _previousTime = time;
                return (LRESULT)0;
            case MessageType.Paint:
                using (DeviceContext dc = window.BeginPaint())
                {
                    SetIsotropic(dc);
                    DrawClock(dc);
                    DrawHands(dc, _previousTime);
                }
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}
