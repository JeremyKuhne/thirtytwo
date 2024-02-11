// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Windows;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Imaging;
using Bitmap = Windows.Win32.Graphics.Direct2D.Bitmap;

namespace ImagingDemo;

internal class Program
{
    [STAThread]
    private static void Main() => Application.Run(new ImagingDemo());

    private unsafe class ImagingDemo : MainWindow
    {
        private FormatConverter? _converter;
        private Bitmap? _bitmap;


        public ImagingDemo() : base(
            title: "Simple Windows Imaging Application",
            backgroundColor: Color.Black,
            features: Features.EnableDirect2d)
        {
        }

        [MemberNotNull(nameof(_converter))]
        private void CreateBitmapFromFile(string fileName)
        {
            _converter?.Dispose();
            _bitmap?.Dispose();

            using BitmapDecoder decoder = new(fileName);
            using BitmapFrameDecode frame = decoder.GetFrame(0);
            _converter = new(frame);
        }

        protected override void RenderTargetCreated()
        {
            if (_converter is null)
            {
                CreateBitmapFromFile("Blue Marble 2012 Original.jpg");
            }

            _bitmap?.Dispose();
            _bitmap = RenderTarget.CreateBitmapFromWicBitmap(_converter);
            base.RenderTargetCreated();
        }

        protected override void OnPaint()
        {
            if (_bitmap is not null)
            {
                RenderTarget.DrawBitmap(_bitmap);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _converter?.Dispose();
                _bitmap?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
