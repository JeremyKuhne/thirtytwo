// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.Graphics.Imaging;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class RenderTarget : Resource, IPointer<ID2D1RenderTarget>
{
    public unsafe new ID2D1RenderTarget* Pointer => (ID2D1RenderTarget*)base.Pointer;

    public RenderTarget(ID2D1RenderTarget* renderTarget) : base((ID2D1Resource*)renderTarget)
    {
    }

    public void BeginDraw()
    {
        Pointer->BeginDraw();
        GC.KeepAlive(this);
    }

    public void EndDraw(out bool recreateTarget)
    {
        HRESULT result = Pointer->EndDraw(null, null);
        if (result == HRESULT.D2DERR_RECREATE_TARGET)
        {
            recreateTarget = true;
        }
        else
        {
            result.ThrowOnFailure();
            recreateTarget = false;
        }

        GC.KeepAlive(this);
    }

    public SolidColorBrush CreateSolidColorBrush(Color color)
    {
        ID2D1SolidColorBrush* solidColorBrush;
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        Pointer->CreateSolidColorBrush(&colorf, null, &solidColorBrush).ThrowOnFailure();
        GC.KeepAlive(this);
        return new SolidColorBrush(solidColorBrush);
    }

    public void SetTransform(Matrix3x2 transform)
    {
        Pointer->SetTransform((D2D_MATRIX_3X2_F*)&transform);
        GC.KeepAlive(this);
    }

    public void Clear(Color color)
    {
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        Pointer->Clear(&colorf);
        GC.KeepAlive(this);
    }

    public void DrawLine(PointF point0, PointF point1, Brush brush, float strokeWidth = 1.0f)
    {
        Pointer->DrawLine(*(D2D_POINT_2F*)&point0, *(D2D_POINT_2F*)&point1, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(this);
    }

    public void FillRectangle(RectangleF rect, Brush brush)
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        Pointer->FillRectangle(&rectf, brush.Pointer);
        GC.KeepAlive(this);
    }

    public void DrawRectangle(RectangleF rect, Brush brush, float strokeWidth = 1.0f)
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        Pointer->DrawRectangle(&rectf, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(this);
    }

    public SizeF Size()
    {
        D2D_SIZE_F size = Pointer->GetSizeHack();
        GC.KeepAlive(this);
        return *(SizeF*)&size;
    }

    /// <inheritdoc cref="ID2D1RenderTarget.DrawTextLayout(D2D_POINT_2F, IDWriteTextLayout*, ID2D1Brush*, D2D1_DRAW_TEXT_OPTIONS)"/>
    public void DrawTextLayout(
        PointF origin,
        TextLayout textLayout,
        Brush defaultFillBrush,
        DrawTextOptions options = DrawTextOptions.None)
    {
        Pointer->DrawTextLayout(
            origin,
            textLayout.Pointer,
            defaultFillBrush.Pointer,
            (D2D1_DRAW_TEXT_OPTIONS)options);

        GC.KeepAlive(this);
        GC.KeepAlive(textLayout);
        GC.KeepAlive(defaultFillBrush);
    }

    public void DrawBitmap(
        Bitmap bitmap,
        RectangleF destinationRectangle = default,
        float opacity = 1.0f,
        BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.Linear)
    {
        D2D_RECT_F destination = (D2D_RECT_F)destinationRectangle;
        if (destinationRectangle.IsEmpty)
        {
            D2D_SIZE_F size = Pointer->GetSizeHack();
            destination = new D2D_RECT_F { left = 0, top = 0, right = size.width, bottom = size.height };
        }
        else
        {
            destination = (D2D_RECT_F)destinationRectangle;
        }

        Pointer->DrawBitmap(
            bitmap.Pointer,
            &destination,
            opacity,
            (D2D1_BITMAP_INTERPOLATION_MODE)interpolationMode,
            null);

        GC.KeepAlive(this);
        GC.KeepAlive(bitmap);
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