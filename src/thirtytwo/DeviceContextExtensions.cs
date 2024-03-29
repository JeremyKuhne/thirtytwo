﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;

namespace Windows;

public static unsafe partial class DeviceContextExtensions
{
    /// <inheritdoc cref="Interop.GetGraphicsMode(HDC)"/>
    public static GRAPHICS_MODE GetGraphicsMode<T>(this T context) where T : IHandle<HDC>
    {
        GRAPHICS_MODE mode = (GRAPHICS_MODE)Interop.GetGraphicsMode(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.SetGraphicsMode(HDC, GRAPHICS_MODE)"/>
    public static GRAPHICS_MODE SetGraphicsMode<T>(this T context, GRAPHICS_MODE mode)
        where T : IHandle<HDC>
    {
        mode = (GRAPHICS_MODE)Interop.SetGraphicsMode(context.Handle, mode);
        GC.KeepAlive(context.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.GetBkColor(HDC)"/>
    public static Color GetBackgroundColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF result = Interop.GetBkColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.SetBkColor(HDC, COLORREF)"/>
    public static Color SetBackgroundColor<T>(this T context, Color color) where T : IHandle<HDC>
    {
        COLORREF result = Interop.SetBkColor(context.Handle, (COLORREF)color);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.GetWorldTransform(HDC, XFORM*)"/>
    public static unsafe bool GetWorldTransform<T>(this T context, ref Matrix3x2 transform)
        where T : IHandle<HDC>
    {
        fixed (Matrix3x2* t = &transform)
        {
            bool result = Interop.GetWorldTransform(context.Handle, (XFORM*)t);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="Interop.SetWorldTransform(HDC, XFORM*)"/>
    public static unsafe bool SetWorldTransform<T>(this T context, ref Matrix3x2 transform)
        where T : IHandle<HDC>
    {
        fixed (Matrix3x2* t = &transform)
        {
            bool result = Interop.SetWorldTransform(context.Handle, (XFORM*)t);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="Interop.GetDeviceCaps(HDC, GET_DEVICE_CAPS_INDEX)"/>
    public static int GetDeviceCaps<T>(this T context, GET_DEVICE_CAPS_INDEX index)
       where T : IHandle<HDC>
    {
        int result = Interop.GetDeviceCaps(context.Handle, index);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Converts the requested point size to height based on the DPI of the given device context.
    /// </summary>
    public static int FontPointSizeToHeight<T>(this T context, int pointSize)
        where T : IHandle<HDC>
    {
        Application.EnsureDpiAwareness();
        int result = Interop.MulDiv(
           pointSize,
           Interop.GetDeviceCaps(context.Handle, GET_DEVICE_CAPS_INDEX.LOGPIXELSY),
           72);

        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static ObjectScope<T> SelectObject<T>(this T context, HGDIOBJ @object)
        where T : IHandle<HDC>
    {
        HGDIOBJ handle = Interop.SelectObject(context.Handle, @object);
        if (handle.IsNull)
        {
            return default;
        }

        OBJ_TYPE type = (OBJ_TYPE)Interop.GetObjectType(@object);
        return type == OBJ_TYPE.OBJ_REGION ? default : new(handle, context);
    }

    public static PolyFillMode SetPolyFillMode<T>(this T context, PolyFillMode mode)
        where T : IHandle<HDC>
    {
        PolyFillMode result = (PolyFillMode)Interop.SetPolyFillMode(context.Handle, (CREATE_POLYGON_RGN_MODE)mode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static bool Polygon<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        Polygon(context, points.AsSpan());

    public static bool Polygon<T>(this T context, ReadOnlySpan<Point> points)
        where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool result = Interop.Polygon(context.Handle, p, points.Length);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    public static bool Polyline<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        Polyline(context, points.AsSpan());

    public static bool Polyline<T>(this T context, ReadOnlySpan<Point> points)
        where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool result = Interop.Polyline(context.Handle, p, points.Length);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="DrawText{TDeviceContext, HFONT}(TDeviceContext, ReadOnlySpan{char}, Rectangle, DrawTextFormat, HFONT, Color, Color)"/>
    public static unsafe (int Height, uint LengthDrawn, Rectangle Bounds) DrawText<TDeviceContext>(
        this TDeviceContext context,
        ReadOnlySpan<char> text,
        Rectangle bounds,
        DrawTextFormat format,
        Color foreColor = default,
        Color backColor = default)
        where TDeviceContext : IHandle<HDC> =>
        DrawText<TDeviceContext, HFONT>(context, text, bounds, format, default, foreColor, backColor);

    /// <summary>
    ///  Draws text using the given font and format.
    /// </summary>
    /// <param name="text">Text to draw.</param>
    /// <param name="format">Format flags.</param>
    /// <param name="bounds">The bounds to render in.</param>
    /// <param name="foreColor">The foreground color for the text, or black by default.</param>
    /// <param name="backColor">The background color to use, or paint transparently.</param>
    public static unsafe (int Height, uint LengthDrawn, Rectangle Bounds) DrawText<TDeviceContext, TFont>(
        this TDeviceContext context,
        ReadOnlySpan<char> text,
        Rectangle bounds,
        DrawTextFormat format,
        TFont? hfont = default,
        Color foreColor = default,
        Color backColor = default)
        where TDeviceContext : IHandle<HDC>
        where TFont : IHandle<HFONT>
    {
        int state = Interop.SaveDC(context.Handle);
        Debug.Assert(state != 0);

        BACKGROUND_MODE newBackGroundMode = (backColor.IsEmpty || backColor == Color.Transparent)
            ? BACKGROUND_MODE.TRANSPARENT
            : BACKGROUND_MODE.OPAQUE;

        int priorBkMode = Interop.SetBkMode(context.Handle, newBackGroundMode);
        Debug.Assert(priorBkMode != 0);

        if (newBackGroundMode == BACKGROUND_MODE.OPAQUE)
        {
            Interop.SetBkColor(context.Handle, (COLORREF)backColor);
        }

        if (foreColor.IsEmpty)
        {
            foreColor = Color.Black;
        }

        Interop.SetTextColor(context.Handle, (COLORREF)foreColor);

        if (hfont is not null && !hfont.Handle.IsNull)
        {
            Interop.SelectObject(context.Handle, hfont.Handle);
        }

        DRAWTEXTPARAMS* dtp = null;
        DRAWTEXTPARAMS dt = default;

        if (format.HasFlag(DrawTextFormat.TabStop))
        {
            // Populate the tab stops.
            dt.cbSize = (uint)sizeof(DRAWTEXTPARAMS);
            dt.iTabLength = (int)(((uint)format & 0xFF00) >> 8);
            format = (DrawTextFormat)((uint)format & 0xFFFF00FF);
            dtp = &dt;
        }

        RECT rect = bounds;

        try
        {
            return DrawTextHelper(context, text, &rect, format, dtp);
        }
        finally
        {
            bool success = Interop.RestoreDC(context.Handle, state);
            Debug.Assert(success);
            GC.KeepAlive(context.Wrapper);
            GC.KeepAlive(hfont?.Wrapper);
        }
    }

    private static (int Height, uint LengthDrawn, Rectangle Bounds) DrawTextHelper<T>(
        this T context,
        ReadOnlySpan<char> text,
        RECT* bounds,
        DrawTextFormat format,
        DRAWTEXTPARAMS* dtp)
        where T : IHandle<HDC>
    {
        if (!format.HasFlag(DrawTextFormat.ModifyString))
        {
            // The string won't be changed, we can just pin
            fixed (char* c = text)
            {
                int result = Interop.DrawTextEx(
                    context.Handle,
                    (PWSTR)c,
                    text.Length,
                    bounds,
                    (DRAW_TEXT_FORMAT)format,
                    dtp);

                if (result == 0)
                {
                    Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
                }

                GC.KeepAlive(context.Wrapper);
                return (result, dtp is null ? 0 : dtp->uiLengthDrawn, *bounds);
            }
        }

        using BufferScope<char> buffer = new(text.Length);
        text.CopyTo(buffer);
        fixed (char* c = buffer)
        {
            int result = Interop.DrawTextEx(context.Handle, (PWSTR)c, text.Length, bounds, (DRAW_TEXT_FORMAT)format, dtp);
            if (result == 0)
            {
                Error.ThrowLastError();
            }

            GC.KeepAlive(context.Wrapper);
            return (result, dtp is null ? 0 : dtp->uiLengthDrawn, *bounds);
        }
    }

    public static void DrawIcon<TDeviceContext, TIcon>(
        this TDeviceContext context,
        TIcon icon,
        Point location,
        Size size = default,
        DI_FLAGS flags = DI_FLAGS.DI_NORMAL)
            where TDeviceContext : IHandle<HDC>
            where TIcon : IHandle<HICON>
    {
        if (!Interop.DrawIconEx(
            context.Handle,
            location.X, location.Y,
            icon.Handle,
            size.Width, size.Height,
            0,
            HBRUSH.Null,
            flags))
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(context.Wrapper);
        GC.KeepAlive(icon.Wrapper);
    }

    public static DeviceContext CreateCompatibleDeviceContext<TDeviceContext>(this TDeviceContext context)
        where TDeviceContext : IHandle<HDC>
    {
        HDC hdc = Interop.CreateCompatibleDC(context.Handle);
        if (hdc.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(context.Wrapper);
        return DeviceContext.Create(hdc, ownsHandle: true);
    }

    public static Bitmap CreateCompatibleBitmap<T>(this T context, Size size) where T : IHandle<HDC>
    {
        HBITMAP hbitmap = Interop.CreateCompatibleBitmap(context.Handle, size.Width, size.Height);
        if (hbitmap.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(context.Wrapper);
        return Bitmap.Create(hbitmap, ownsHandle: true);
    }

    public static unsafe bool OffsetWindowOrigin<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = Interop.OffsetWindowOrgEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static unsafe bool OffsetViewportOrigin<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = Interop.OffsetViewportOrgEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static unsafe bool GetWindowExtents<T>(this T context, out Size size) where T : IHandle<HDC>
    {
        fixed (Size* s = &size)
        {
            bool success = Interop.GetWindowExtEx(context.Handle, (SIZE*)s);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    /// <summary>
    ///  Sets the logical ("window") dimensions of the device context.
    /// </summary>
    public static unsafe bool SetWindowExtents<T>(this T context, Size size) where T : IHandle<HDC>
    {
        bool success = Interop.SetWindowExtEx(context.Handle, size.Width, size.Height, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static unsafe bool GetViewportExtents<T>(this T context, out Size size) where T : IHandle<HDC>
    {
        fixed (Size* s = &size)
        {
            bool success = Interop.GetViewportExtEx(context.Handle, (SIZE*)s);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    public static unsafe bool SetViewportExtents<T>(this T context, Size size) where T : IHandle<HDC>
    {
        bool success = Interop.SetViewportExtEx(context.Handle, size.Width, size.Height, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static MappingMode GetMappingMode<T>(this T context) where T : IHandle<HDC>
    {
        MappingMode result = (MappingMode)Interop.GetMapMode(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static MappingMode SetMappingMode<T>(this T context, MappingMode mapMode) where T : IHandle<HDC>
    {
        MappingMode result = (MappingMode)Interop.SetMapMode(context.Handle, (HDC_MAP_MODE)mapMode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static unsafe Point GetViewportOrigin<T>(this T context, out bool success)
        where T : IHandle<HDC>
    {
        Point point;
        success = Interop.GetViewportOrgEx(context.Handle, &point);
        GC.KeepAlive(context.Wrapper);
        return point;
    }

    public static unsafe bool SetViewportOrigin<T>(this T context, Point point)
        where T : IHandle<HDC>
    {
        bool result = Interop.SetViewportOrgEx(context.Handle, point.X, point.Y, null);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static RegionType SelectClippingRegion<T>(this T context, HRGN region)
        where T : IHandle<HDC>
    {
        RegionType type = (RegionType)Interop.SelectClipRgn(context.Handle, region);
        GC.KeepAlive(context.Wrapper);
        return type;
    }

    public static bool MoveTo<T>(this T context, Point point) where T : IHandle<HDC> =>
        context.MoveTo(point.X, point.Y);

    public static bool MoveTo<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool result = Interop.MoveToEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static bool LineTo<T>(this T context, Point point) where T : IHandle<HDC> =>
        context.LineTo(point.X, point.Y);

    public static bool LineTo<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = Interop.LineTo(context.Handle, x, y);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool Ellipse<T>(this T context, Rectangle rectangle) where T : IHandle<HDC> =>
        context.Ellipse(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    public static bool Ellipse<T>(this T context, int left, int top, int right, int bottom) where T : IHandle<HDC>
    {
        bool success = Interop.Ellipse(context.Handle, left, top, right, bottom);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool PolyBezier<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        PolyBezier(context, points.AsSpan());

    public static bool PolyBezier<T>(this T context, ReadOnlySpan<Point> points) where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool success = Interop.PolyBezier(context.Handle, p, (uint)points.Length);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    public static bool Rectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC> =>
        context.Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    public static bool Rectangle<T>(this T context, int left, int top, int right, int bottom)
        where T : IHandle<HDC>
    {
        bool success = Interop.Rectangle(context.Handle, left, top, right, bottom);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool RoundRectangle<T>(this T context, Rectangle rectangle, Size corner) where T : IHandle<HDC> =>
        context.RoundRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, corner.Width, corner.Height);

    public static bool RoundRectangle<T>(this T context, int left, int top, int right, int bottom, int width, int height)
        where T : IHandle<HDC>
    {
        bool success = Interop.RoundRect(context.Handle, left, top, right, bottom, width, height);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool FillRectangle<T>(this T context, Rectangle rectangle, HBRUSH hbrush) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = (BOOL)Interop.FillRect(context.Handle, &rect, hbrush);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool FrameRectangle<T>(this T context, Rectangle rectangle, HBRUSH brush) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = (BOOL)Interop.FrameRect(context.Handle, &rect, brush);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool InvertRectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = Interop.InvertRect(context.Handle, &rect);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static bool DrawFocusRectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = Interop.DrawFocusRect(context.Handle, &rect);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    public static PenMixMode SetRasterOperation<T>(this T context, PenMixMode foregroundMixMode)
        where T : IHandle<HDC>
    {
        PenMixMode result = (PenMixMode)Interop.SetROP2(context.Handle, (R2_MODE)foregroundMixMode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static PenMixMode GetRasterOperation<T>(this T context) where T : IHandle<HDC>
    {
        PenMixMode result = (PenMixMode)Interop.GetROP2(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static Color GetBrushColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF color = Interop.GetDCBrushColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return color;
    }

    public static Color GetTextColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF color = Interop.GetTextColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return color;
    }
}