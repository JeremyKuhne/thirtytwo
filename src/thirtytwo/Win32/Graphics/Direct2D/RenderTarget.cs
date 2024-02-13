// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.Graphics.Imaging;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class RenderTarget : Resource, IPointer<ID2D1RenderTarget>
{
    public unsafe new ID2D1RenderTarget* Pointer => (ID2D1RenderTarget*)base.Pointer;

    public RenderTarget(ID2D1RenderTarget* renderTarget) : base((ID2D1Resource*)renderTarget)
    {
    }

    /// <inheritdoc cref="ID2D1RenderTarget.CreateBitmapFromWicBitmap(IWICBitmapSource*, D2D1_BITMAP_PROPERTIES*, ID2D1Bitmap**)"/>
    public Bitmap CreateBitmapFromWicBitmap<TBitmapSource>(
        TBitmapSource wicBitmap)
        where TBitmapSource : IPointer<IWICBitmapSource>
    {
        ID2D1Bitmap* d2dBitmap;
        Pointer->CreateBitmapFromWicBitmap(
            wicBitmap.Pointer,
            bitmapProperties: (D2D1_BITMAP_PROPERTIES*)null,
            &d2dBitmap).ThrowOnFailure();

        Bitmap bitmap = new(d2dBitmap);
        GC.KeepAlive(this);
        GC.KeepAlive(wicBitmap);
        return bitmap;
    }

    public Bitmap CreateBitmapFromGdiPlusBitmap(GdiPlus.Bitmap bitmap)
    {
        GdiPlus.PixelFormat pixelFormat = bitmap.PixelFormat;
        RectangleF bounds = bitmap.Bounds;

        const int BytesPerPixel = 4;

        // We could let GDI+ do the buffer allocation, but for illustrative purposes I've done it here.
        // Note that GDI+ always copies the data, even if it internally is in the desired format.
        using BufferScope<byte> buffer = new((int)bounds.Width * (int)bounds.Height * BytesPerPixel);

        fixed (byte* b = buffer)
        {
            GdiPlus.BitmapData bitmapData = new()
            {
                Width = (uint)bounds.Width,
                Height = (uint)bounds.Height,
                Stride = (int)bounds.Width * BytesPerPixel,
                PixelFormat = (int)GdiPlus.PixelFormat.Format32bppArgb,
                Scan0 = b
            };

            bitmap.LockBits(
                new((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height),
                GdiPlus.ImageLockMode.ImageLockModeUserInputBuf | GdiPlus.ImageLockMode.ImageLockModeRead,
                GdiPlus.PixelFormat.Format32bppArgb,
                ref bitmapData);

            D2D1_BITMAP_PROPERTIES bitmapProperties = new()
            {
                pixelFormat = new D2D1_PIXEL_FORMAT
                {
                    format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                    alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_IGNORE
                },
                dpiX = 96,
                dpiY = 96
            };

            ID2D1Bitmap* newBitmap;
            HRESULT result = Pointer->CreateBitmap(
                new((uint)bounds.Width, (uint)bounds.Height),
                b,
                (uint)bitmapData.Stride,
                &bitmapProperties,
                &newBitmap);

            bitmap.UnlockBits(ref bitmapData);
            result.ThrowOnFailure();

            GC.KeepAlive(this);
            return new Bitmap(newBitmap);
        }
    }

    public static implicit operator ID2D1RenderTarget*(RenderTarget renderTarget) => renderTarget.Pointer;
}