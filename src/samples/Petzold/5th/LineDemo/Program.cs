// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Foundation;

namespace LineDemo;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 5-14, Pages 153-155.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main() => Application.Run(new LineDemo("LineDemo"));

    private class LineDemo : MainWindow
    {
        private static int s_cxClient, s_cyClient;

        public LineDemo(string title) : base(title) { }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Size:
                    s_cxClient = lParam.LOWORD;
                    s_cyClient = lParam.HIWORD;
                    return (LRESULT)0;
                case MessageType.Paint:
                    using (DeviceContext dc = window.BeginPaint())
                    {
                        dc.Rectangle(s_cxClient / 8, s_cyClient / 8, 7 * s_cxClient / 8, 7 * s_cyClient / 8);
                        dc.MoveTo(0, 0);
                        dc.LineTo(s_cxClient, s_cyClient);
                        dc.MoveTo(0, s_cyClient);
                        dc.LineTo(s_cxClient, 0);
                        dc.Ellipse(s_cxClient / 8, s_cyClient / 8, 7 * s_cxClient / 8, 7 * s_cyClient / 8);
                        dc.RoundRectangle(
                            Rectangle.FromLTRB(s_cxClient / 4, s_cyClient / 4, 3 * s_cxClient / 4, 3 * s_cyClient / 4),
                            new Size(s_cxClient / 4, s_cyClient / 4));
                    }

                    return (LRESULT)0;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
