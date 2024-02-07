// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectWrite;
using FontWeight = Windows.Win32.Graphics.DirectWrite.FontWeight;

namespace DirectWriteDemo;

internal class Program
{
    [STAThread]
    private static void Main() => Application.Run(new DirectWriteDemo());

    private class DirectWriteDemo : MainWindow
    {
        private const string Message = "Hello World From ... DirectWrite!";

        protected TextFormat _textFormat;
        protected TextLayout? _textLayout;
        protected Typography _typography;

        protected SolidColorBrush? _blackBrush;

        public DirectWriteDemo() : base(title: "Simple DirectWrite Application", features: Features.EnableDirect2d)
        {
            _textFormat = new("Gabriola", fontSize: 64)
            {
                TextAlignment = TextAlignment.Center,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            _typography = new();
            _typography.AddFontFeature(FontFeatureTag.StylisticSet7);
        }

        protected override void RenderTargetCreated()
        {
            _blackBrush?.Dispose();
            _blackBrush = RenderTarget.CreateSolidColorBrush(Color.Black);
            base.RenderTargetCreated();
        }

        protected override void OnPaint()
        {
            RenderTarget.Clear(Color.CornflowerBlue);
            RenderTarget.DrawTextLayout(default, _textLayout!, _blackBrush!);
        }

        protected override void OnSize(Size size)
        {
            if (_textLayout is not null)
            {
                _textLayout.MaxHeight = size.Height;
                _textLayout.MaxWidth = size.Width;
                return;
            }

            _textLayout = new(Message, _textFormat, RenderTarget.Size());

            // (21, 12) is the range around "DirectWrite!"
            _textLayout.SetFontSize(100, (21, 12));
            _textLayout.SetTypography(_typography, (0, Message.Length));
            _textLayout.SetUnderline(true, (21, 12));
            _textLayout.SetFontWeight(FontWeight.Bold, (21, 12));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _textFormat.Dispose();
                _textLayout?.Dispose();
                _blackBrush?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
