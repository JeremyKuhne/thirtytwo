// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;

namespace Windows;

public static unsafe partial class DeviceContextExtensions
{
    /// <inheritdoc cref="Interop.GetGraphicsMode(HDC)"/>
    public static GRAPHICS_MODE GetGraphicsMode<T>(this T context)
        where T : IHandle<HDC>
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

    public static bool Polygon<T>(this T context, params Point[] points)
        where T: IHandle<HDC> => Polygon(context, points.AsSpan());

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

    public static unsafe (int Height, uint LengthDrawn, Rectangle Bounds) DrawText<T>(
        this T context,
        ReadOnlySpan<char> text,
        Rectangle bounds,
        DrawTextFormat format)
        where T : IHandle<HDC>
    {
        RECT rect = bounds;

        DRAWTEXTPARAMS* dtp = null;
        DRAWTEXTPARAMS dt = default;

        if (format.HasFlag(DrawTextFormat.TabStop))
        {
            dt.cbSize = (uint)sizeof(DRAWTEXTPARAMS);
            dt.iTabLength = (int)(((uint)format & 0xFF00) >> 8);
            format = (DrawTextFormat)((uint)format & 0xFFFF00FF);
            dtp = &dt;
        }

        return DrawTextHelper(context, text, &rect, format, dtp);
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
                int result = Interop.DrawTextEx(context.Handle, (PWSTR)c, text.Length, bounds, (DRAW_TEXT_FORMAT)format, dtp);
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
        if (!Interop.DrawIconEx(context.Handle, location.X, location.Y, icon.Handle, size.Width, size.Height, 0, default, flags))
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

    public static Bitmap CreateCompatibleBitmap<T>(this T context, Size size)
        where T : IHandle<HDC>
    {
        HBITMAP hbitmap = Interop.CreateCompatibleBitmap(context.Handle, size.Width, size.Height);
        if (hbitmap.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(context.Wrapper);
        return Bitmap.Create(hbitmap, ownsHandle: true);
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

    public static bool MoveTo<T>(this T context, Point point)
        where T : IHandle<HDC>
        => context.MoveTo(point.X, point.Y);

    public static bool MoveTo<T>(this T context, int x, int y)
        where T : IHandle<HDC>
    {
        bool result = Interop.MoveToEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    public static bool LineTo<T>(this T context, Point point)
        where T : IHandle<HDC>
        => context.LineTo(point.X, point.Y);

    public static bool LineTo<T>(this T context, int x, int y)
        where T : IHandle<HDC>
    {
        bool success = Interop.LineTo(context.Handle, x, y);
        GC.KeepAlive(context.Wrapper);
        return success;
    }
}