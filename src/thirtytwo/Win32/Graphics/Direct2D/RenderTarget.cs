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

/// <summary>
///  Managed wrapper for <see cref="ID2D1RenderTarget"/>.
/// </summary>
/// <remarks>Provides drawing operations against a Direct2D render target.</remarks>
public unsafe class RenderTarget : Resource, IPointer<ID2D1RenderTarget>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1RenderTarget"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1RenderTarget* Pointer => (ID2D1RenderTarget*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1RenderTarget"/> pointer.
    /// </summary>
    /// <param name="renderTarget">The native render target pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public RenderTarget(ID2D1RenderTarget* renderTarget) : base((ID2D1Resource*)renderTarget)
    {
    }

    /// <summary>
    ///  Begins a drawing batch on the render target.
    /// </summary>
    public void BeginDraw()
    {
        Pointer->BeginDraw();
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Ends a drawing batch and reports whether the target must be recreated.
    /// </summary>
    /// <param name="recreateTarget">
    ///  Set to <see langword="true"/> when Direct2D reports <c>D2DERR_RECREATE_TARGET</c>; otherwise <see langword="false"/>.
    /// </param>
    /// <remarks>
    ///  Any failure code other than <c>D2DERR_RECREATE_TARGET</c> results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
    public void EndDraw(out bool recreateTarget)
    {
        HRESULT result = Pointer->EndDraw(null, null);
        if (result == PInvoke.D2DERR_RECREATE_TARGET)
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

    /// <summary>
    ///  Creates a solid-color brush for this render target.
    /// </summary>
    /// <param name="color">The brush color.</param>
    /// <returns>A wrapper for the created <see cref="ID2D1SolidColorBrush"/>.</returns>
    /// <remarks>A failed native call results in exception flow from <c>ThrowOnFailure</c>.</remarks>
    public SolidColorBrush CreateSolidColorBrush(Color color)
    {
        ID2D1SolidColorBrush* solidColorBrush;
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        Pointer->CreateSolidColorBrush(&colorf, null, &solidColorBrush).ThrowOnFailure();
        GC.KeepAlive(this);
        return new SolidColorBrush(solidColorBrush);
    }

    /// <summary>
    ///  Sets the world transform applied to subsequent drawing operations.
    /// </summary>
    /// <param name="transform">The 3x2 affine transform matrix.</param>
    public void SetTransform(Matrix3x2 transform)
    {
        Pointer->SetTransform((D2D_MATRIX_3X2_F*)&transform);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Clears the render target with a solid color.
    /// </summary>
    /// <param name="color">The clear color.</param>
    public void Clear(Color color)
    {
        D2D1_COLOR_F colorf = (D2D1_COLOR_F)color;
        Pointer->Clear(&colorf);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Draws a line segment between two points.
    /// </summary>
    /// <param name="point0">The starting point, in device-independent pixels (DIPs).</param>
    /// <param name="point1">The ending point, in device-independent pixels (DIPs).</param>
    /// <param name="brush">The brush used to draw the line.</param>
    /// <param name="strokeWidth">The stroke width, in DIPs.</param>
    public void DrawLine(PointF point0, PointF point1, Brush brush, float strokeWidth = 1.0f)
    {
        Pointer->DrawLine(*(D2D_POINT_2F*)&point0, *(D2D_POINT_2F*)&point1, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Fills a rectangle with the specified brush.
    /// </summary>
    /// <param name="rect">The rectangle to fill, in DIPs.</param>
    /// <param name="brush">The brush used to fill the rectangle.</param>
    public void FillRectangle(RectangleF rect, Brush brush)
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        Pointer->FillRectangle(&rectf, brush.Pointer);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Draws the outline of a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to draw, in DIPs.</param>
    /// <param name="brush">The brush used to draw the rectangle outline.</param>
    /// <param name="strokeWidth">The stroke width, in DIPs.</param>
    public void DrawRectangle(RectangleF rect, Brush brush, float strokeWidth = 1.0f)
    {
        D2D_RECT_F rectf = (D2D_RECT_F)rect;
        Pointer->DrawRectangle(&rectf, brush.Pointer, strokeWidth, null);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Gets the render target size.
    /// </summary>
    /// <returns>The render target dimensions in device-independent pixels (DIPs).</returns>
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

    /// <summary>
    ///  Draws a bitmap to the render target.
    /// </summary>
    /// <param name="bitmap">The bitmap to draw.</param>
    /// <param name="destinationRectangle">
    ///  The destination rectangle, in DIPs. If empty, the full render target bounds are used.
    /// </param>
    /// <param name="opacity">The opacity multiplier applied while drawing.</param>
    /// <param name="interpolationMode">The interpolation mode used when scaling is required.</param>
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

    /// <summary>
    ///  Creates a Direct2D bitmap from a WIC bitmap source.
    /// </summary>
    /// <param name="wicBitmap">The source WIC bitmap interface.</param>
    /// <returns>A wrapper for the created <see cref="ID2D1Bitmap"/>.</returns>
    /// <remarks>
    ///  Equivalent to calling <see cref="ID2D1RenderTarget.CreateBitmapFromWicBitmap(IWICBitmapSource*, D2D1_BITMAP_PROPERTIES*, ID2D1Bitmap**)"/>
    ///  with default bitmap properties.
    ///  A failed native call results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
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

    /// <summary>
    ///  Creates a Direct2D bitmap by copying pixel data from a GDI+ bitmap.
    /// </summary>
    /// <param name="bitmap">The source GDI+ bitmap.</param>
    /// <returns>A wrapper for the created <see cref="ID2D1Bitmap"/>.</returns>
    /// <remarks>
    ///  Pixel data is copied into an intermediate 32bpp ARGB buffer and then into a Direct2D bitmap using
    ///  <c>DXGI_FORMAT_B8G8R8A8_UNORM</c> with <c>D2D1_ALPHA_MODE_IGNORE</c> and 96 DPI.
    ///  A failed native call results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
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

    /// <summary>
    ///  Converts a <see cref="RenderTarget"/> wrapper to its underlying <see cref="ID2D1RenderTarget"/> pointer.
    /// </summary>
    /// <param name="renderTarget">The wrapper instance.</param>
    /// <returns>The wrapped native render target pointer.</returns>
    public static implicit operator ID2D1RenderTarget*(RenderTarget renderTarget) => renderTarget.Pointer;
}