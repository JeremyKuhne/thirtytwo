// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;

namespace Direct2dDemo;

internal class Program
{
    [STAThread]
    private static void Main() => Application.Run(new Direct2dDemo());

    private class Direct2dDemo : MainWindow
    {
        private SolidColorBrush? _lightSlateGrayBrush;
        private SolidColorBrush? _cornflowerBlueBrush;

        public Direct2dDemo() : base(title: "Simple Direct2D Application", features: Features.EnableDirect2d)
        {
        }

        protected override void RenderTargetCreated(HwndRenderTarget renderTarget)
        {
            _lightSlateGrayBrush?.Dispose();
            _cornflowerBlueBrush?.Dispose();
            _lightSlateGrayBrush = renderTarget.CreateSolidColorBrush(Color.LightSlateGray);
            _cornflowerBlueBrush = renderTarget.CreateSolidColorBrush(Color.CornflowerBlue);
            base.RenderTargetCreated(renderTarget);
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Paint:
                    if (IsDirect2dEnabled(out var renderTarget))
                    {
                        renderTarget.SetTransform(Matrix3x2.Identity);
                        renderTarget.Clear(Color.White);

                        SizeF size = renderTarget.Size();

                        for (int x = 0; x < size.Width; x += 10)
                        {
                            renderTarget.DrawLine(
                                new(x, 0), new(x, size.Height),
                                _lightSlateGrayBrush!,
                                0.5f);
                        }

                        for (int y = 0; y < size.Height; y += 10)
                        {
                            renderTarget.DrawLine(
                                new(0, y), new(size.Width, y),
                                _lightSlateGrayBrush!,
                                0.5f);
                        }

                        RectangleF rectangle1 = RectangleF.FromLTRB(
                            size.Width / 2 - 50,
                            size.Height / 2 - 50,
                            size.Width / 2 + 50,
                            size.Height / 2 + 50);

                        RectangleF rectangle2 = RectangleF.FromLTRB(
                            size.Width / 2 - 100,
                            size.Height / 2 - 100,
                            size.Width / 2 + 100,
                            size.Height / 2 + 100);

                        renderTarget.FillRectangle(rectangle1, _lightSlateGrayBrush!);
                        renderTarget.DrawRectangle(rectangle2, _cornflowerBlueBrush!);
                    }
                    return (LRESULT)0;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
