// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.Graphics.Imaging;

namespace Windows.Win32.Graphics.Direct2D;

public static unsafe class RenderTargetExtensions
{
    public static void Resize<T>(this T target, Size size)
        where T : IPointer<ID2D1HwndRenderTarget>
    {
        target.Pointer->Resize((D2D_SIZE_U)size).ThrowOnFailure();
        GC.KeepAlive(target);
    }

    public static void BeginDraw<T>(this T target) where T : IPointer<ID2D1RenderTarget>
    {
        target.Pointer->BeginDraw();
        GC.KeepAlive(target);
    }

    public static void EndDraw<T>(this T target, out bool recreateTarget)
        where T : IPointer<ID2D1RenderTarget>
    {
        HRESULT result = target.Pointer->EndDraw(null, null);
        if (result == HRESULT.D2DERR_RECREATE_TARGET)
        {
            recreateTarget = true;
        }
        else
        {
            result.ThrowOnFailure();
            recreateTarget = false;
        }

        GC.KeepAlive(target);
    }

    public static SolidColorBrush CreateSolidColorBrush<T>(this T target, Color color)
        where T : IPointer<ID2D1RenderTarget>
    {
        ID2D1SolidColorBrush* solidColorBrush;
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        target.Pointer->CreateSolidColorBrush(&colorf, null, &solidColorBrush).ThrowOnFailure();
        GC.KeepAlive(target);
        return new SolidColorBrush(solidColorBrush);
    }

    public static void SetTransform<T>(this T target, Matrix3x2 transform) where T : IPointer<ID2D1RenderTarget>
    {
        target.Pointer->SetTransform((D2D_MATRIX_3X2_F*)&transform);
        GC.KeepAlive(target);
    }

    public static void Clear<T>(this T target, Color color) where T : IPointer<ID2D1RenderTarget>
    {
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        target.Pointer->Clear(&colorf);
        GC.KeepAlive(target);
    }

    public static void DrawLine<T>(this T target, PointF point0, PointF point1, Brush brush, float strokeWidth = 1.0f)
        where T : IPointer<ID2D1RenderTarget>
    {
        target.Pointer->DrawLine(*(D2D_POINT_2F*)&point0, *(D2D_POINT_2F*)&point1, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(target);
    }

    public static void FillRectangle<T>(this T target, RectangleF rect, Brush brush)
        where T : IPointer<ID2D1RenderTarget>
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        target.Pointer->FillRectangle(&rectf, brush.Pointer);
        GC.KeepAlive(target);
    }

    public static void DrawRectangle<T>(this T target, RectangleF rect, Brush brush, float strokeWidth = 1.0f)
        where T : IPointer<ID2D1RenderTarget>
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        target.Pointer->DrawRectangle(&rectf, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(target);
    }

    public static SizeF Size<T>(this T target) where T : IPointer<ID2D1RenderTarget>
    {
        D2D_SIZE_F size = target.Pointer->GetSizeHack();
        GC.KeepAlive(target);
        return *(SizeF*)&size;
    }

    /// <inheritdoc cref="ID2D1RenderTarget.DrawTextLayout(D2D_POINT_2F, IDWriteTextLayout*, ID2D1Brush*, D2D1_DRAW_TEXT_OPTIONS)"/>
    public static void DrawTextLayout<TTarget, TLayout, TBrush>(
        this TTarget target,
        PointF origin,
        TLayout textLayout,
        TBrush defaultFillBrush,
        DrawTextOptions options = DrawTextOptions.None)
        where TTarget : IPointer<ID2D1RenderTarget>
        where TLayout : IPointer<IDWriteTextLayout>
        where TBrush : IPointer<ID2D1Brush>
    {
        target.Pointer->DrawTextLayout(
            origin,
            textLayout.Pointer,
            defaultFillBrush.Pointer,
            (D2D1_DRAW_TEXT_OPTIONS)options);

        GC.KeepAlive(target);
        GC.KeepAlive(textLayout);
        GC.KeepAlive(defaultFillBrush);
    }

    /// <inheritdoc cref="ID2D1RenderTarget.CreateBitmapFromWicBitmap(IWICBitmapSource*, D2D1_BITMAP_PROPERTIES*, ID2D1Bitmap**)"/>
    public static Bitmap CreateBitmapFromWicBitmap<TRenderTarget, TBitmapSource>(
        this TRenderTarget target,
        TBitmapSource wicBitmap)
        where TRenderTarget : IPointer<ID2D1RenderTarget>
        where TBitmapSource : IPointer<IWICBitmapSource>
    {
        ID2D1Bitmap* d2dBitmap;
        target.Pointer->CreateBitmapFromWicBitmap(
            wicBitmap.Pointer,
            bitmapProperties: (D2D1_BITMAP_PROPERTIES*)null,
            &d2dBitmap).ThrowOnFailure();

        Bitmap bitmap = new(d2dBitmap);
        GC.KeepAlive(target);
        GC.KeepAlive(wicBitmap);
        return bitmap;
    }

    public static void DrawBitmap<TRenderTarget, TBitmap>(
        this TRenderTarget target,
        TBitmap bitmap,
        RectangleF destinationRectangle = default,
        float opacity = 1.0f,
        BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.Linear)
        where TRenderTarget : IPointer<ID2D1RenderTarget>
        where TBitmap : IPointer<ID2D1Bitmap>
    {
        D2D_RECT_F destination = (D2D_RECT_F)destinationRectangle;
        if (destinationRectangle.IsEmpty)
        {
            D2D_SIZE_F size = target.Pointer->GetSizeHack();
            destination = new D2D_RECT_F { left = 0, top = 0, right = size.width, bottom = size.height };
        }
        else
        {
            destination = (D2D_RECT_F)destinationRectangle;
        }

        target.Pointer->DrawBitmap(
            bitmap.Pointer,
            &destination,
            opacity,
            (D2D1_BITMAP_INTERPOLATION_MODE)interpolationMode,
            null);

        GC.KeepAlive(target);
        GC.KeepAlive(bitmap);
    }
}