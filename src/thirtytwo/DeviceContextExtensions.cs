// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;

namespace Windows;

/// <summary>
///  Provides extension methods for common Win32 device-context operations.
/// </summary>
public static unsafe partial class DeviceContextExtensions
{
    /// <inheritdoc cref="Interop.GetGraphicsMode(HDC)"/>
    public static GRAPHICS_MODE GetGraphicsMode<T>(this T context) where T : IHandle<HDC>
    {
        GRAPHICS_MODE mode = (GRAPHICS_MODE)PInvoke.GetGraphicsMode(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.SetGraphicsMode(HDC, GRAPHICS_MODE)"/>
    public static GRAPHICS_MODE SetGraphicsMode<T>(this T context, GRAPHICS_MODE mode)
        where T : IHandle<HDC>
    {
        mode = (GRAPHICS_MODE)PInvoke.SetGraphicsMode(context.Handle, mode);
        GC.KeepAlive(context.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.GetBkColor(HDC)"/>
    public static Color GetBackgroundColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF result = PInvoke.GetBkColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.SetBkColor(HDC, COLORREF)"/>
    public static Color SetBackgroundColor<T>(this T context, Color color) where T : IHandle<HDC>
    {
        COLORREF result = PInvoke.SetBkColor(context.Handle, (COLORREF)color);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.GetWorldTransform(HDC, XFORM*)"/>
    public static bool GetWorldTransform<T>(this T context, ref Matrix3x2 transform)
        where T : IHandle<HDC>
    {
        fixed (Matrix3x2* t = &transform)
        {
            bool result = PInvoke.GetWorldTransform(context.Handle, (XFORM*)t);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="Interop.SetWorldTransform(HDC, XFORM*)"/>
    public static bool SetWorldTransform<T>(this T context, ref Matrix3x2 transform)
        where T : IHandle<HDC>
    {
        fixed (Matrix3x2* t = &transform)
        {
            bool result = PInvoke.SetWorldTransform(context.Handle, (XFORM*)t);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="Interop.GetDeviceCaps(HDC, GET_DEVICE_CAPS_INDEX)"/>
    public static int GetDeviceCaps<T>(this T context, GET_DEVICE_CAPS_INDEX index)
       where T : IHandle<HDC>
    {
        int result = PInvoke.GetDeviceCaps(context.Handle, index);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Converts the requested point size to height based on the DPI of the given device context.
    /// </summary>
    /// <param name="context">The device context whose vertical DPI is used for conversion.</param>
    /// <param name="pointSize">The font size in points.</param>
    /// <returns>The computed font height in logical units for the device context.</returns>
    public static int FontPointSizeToHeight<T>(this T context, int pointSize)
        where T : IHandle<HDC>
    {
        Application.EnsureDpiAwareness();
        int result = PInvoke.MulDiv(
           pointSize,
           PInvoke.GetDeviceCaps(context.Handle, GET_DEVICE_CAPS_INDEX.LOGPIXELSY),
           72);

        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Selects a GDI object into the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="object">The GDI object handle to select.</param>
    /// <returns>
    ///  A scope that restores the previously selected object when disposed, or <see langword="default"/>
    ///  when selection fails or when selecting a region (which has different restore semantics).
    /// </returns>
    public static ObjectScope<T> SelectObject<T>(this T context, HGDIOBJ @object)
        where T : IHandle<HDC>
    {
        HGDIOBJ handle = PInvoke.SelectObject(context.Handle, @object);
        if (handle.IsNull)
        {
            return default;
        }

        OBJ_TYPE type = (OBJ_TYPE)PInvoke.GetObjectType(@object);
        return type == OBJ_TYPE.OBJ_REGION ? default : new(handle, context);
    }

    /// <summary>
    ///  Sets how filled polygons are rendered for subsequent drawing operations.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="mode">The new polygon fill mode.</param>
    /// <returns>The previous polygon fill mode.</returns>
    public static PolyFillMode SetPolyFillMode<T>(this T context, PolyFillMode mode)
        where T : IHandle<HDC>
    {
        PolyFillMode result = (PolyFillMode)PInvoke.SetPolyFillMode(context.Handle, (CREATE_POLYGON_RGN_MODE)mode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Polygon{T}(T, ReadOnlySpan{Point})"/>
    public static bool Polygon<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        Polygon(context, points.AsSpan());

    /// <summary>
    ///  Draws a closed polygon that connects each point and closes back to the first point.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="points">The polygon vertices, in logical units of the device context.</param>
    /// <returns><see langword="true"/> if the polygon is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool Polygon<T>(this T context, ReadOnlySpan<Point> points)
        where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool result = PInvoke.Polygon(context.Handle, p, points.Length);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="Polyline{T}(T, ReadOnlySpan{Point})"/>
    public static bool Polyline<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        Polyline(context, points.AsSpan());

    /// <summary>
    ///  Draws a series of connected line segments through the provided points.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="points">The polyline points, in logical units of the device context.</param>
    /// <returns><see langword="true"/> if the polyline is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool Polyline<T>(this T context, ReadOnlySpan<Point> points)
        where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool result = PInvoke.Polyline(context.Handle, p, points.Length);
            GC.KeepAlive(context.Wrapper);
            return result;
        }
    }

    /// <inheritdoc cref="DrawText{TDeviceContext, HFONT}(TDeviceContext, ReadOnlySpan{char}, Rectangle, DrawTextFormat, HFONT, Color, Color)"/>
    public static (int Height, uint LengthDrawn, Rectangle Bounds) DrawText<TDeviceContext>(
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
    /// <param name="context">The device context to draw into.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="bounds">The layout rectangle, in logical units.</param>
    /// <param name="format">The <see cref="DrawTextFormat"/> flags that control layout and rendering.</param>
    /// <param name="hfont">
    ///  The font to select for drawing, or <see langword="null"/> to use the currently selected font.
    /// </param>
    /// <param name="foreColor">The foreground color for glyph rendering. If empty, black is used.</param>
    /// <param name="backColor">
    ///  The background color. If empty or transparent, text is drawn with a transparent background.
    /// </param>
    /// <returns>
    ///  A tuple containing the rendered text height, the number of characters processed by DrawTextEx,
    ///  and the final bounds rectangle after layout.
    /// </returns>
    public static (int Height, uint LengthDrawn, Rectangle Bounds) DrawText<TDeviceContext, TFont>(
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
        int state = PInvoke.SaveDC(context.Handle);
        Debug.Assert(state != 0);

        BACKGROUND_MODE newBackGroundMode = (backColor.IsEmpty || backColor == Color.Transparent)
            ? BACKGROUND_MODE.TRANSPARENT
            : BACKGROUND_MODE.OPAQUE;

        int priorBkMode = PInvoke.SetBkMode(context.Handle, newBackGroundMode);
        Debug.Assert(priorBkMode != 0);

        if (newBackGroundMode == BACKGROUND_MODE.OPAQUE)
        {
            PInvoke.SetBkColor(context.Handle, (COLORREF)backColor);
        }

        if (foreColor.IsEmpty)
        {
            foreColor = Color.Black;
        }

        PInvoke.SetTextColor(context.Handle, (COLORREF)foreColor);

        if (hfont is not null && !hfont.Handle.IsNull)
        {
            PInvoke.SelectObject(context.Handle, hfont.Handle);
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
            bool success = PInvoke.RestoreDC(context.Handle, state);
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
                int result = PInvoke.DrawTextEx(
                    context.Handle,
                    (PWSTR)c,
                    text.Length,
                    bounds,
                    (DRAW_TEXT_FORMAT)format,
                    dtp);

                if (result == 0)
                {
                    WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
                }

                GC.KeepAlive(context.Wrapper);
                return (result, dtp is null ? 0 : dtp->uiLengthDrawn, *bounds);
            }
        }

        // DrawTextEx can append up to four characters when DT_MODIFYSTRING is set.
        using BufferScope<char> buffer = new(checked(text.Length + 4));
        text.CopyTo(buffer);
        buffer[text.Length..].Clear();
        fixed (char* c = buffer)
        {
            int result = PInvoke.DrawTextEx(context.Handle, (PWSTR)c, text.Length, bounds, (DRAW_TEXT_FORMAT)format, dtp);
            if (result == 0)
            {
                Error.GetLastError().ThrowThirtyTwoException();
            }

            GC.KeepAlive(context.Wrapper);
            return (result, dtp is null ? 0 : dtp->uiLengthDrawn, *bounds);
        }
    }

    /// <summary>
    ///  Draws an icon at the specified location and size.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="icon">The icon handle to draw.</param>
    /// <param name="location">The upper-left destination point, in logical units.</param>
    /// <param name="size">The icon size, in pixels. If empty, the icon's resource size is used.</param>
    /// <param name="flags">Drawing flags passed to DrawIconEx.</param>
    public static void DrawIcon<TDeviceContext, TIcon>(
        this TDeviceContext context,
        TIcon icon,
        Point location,
        Size size = default,
        DI_FLAGS flags = DI_FLAGS.DI_NORMAL)
            where TDeviceContext : IHandle<HDC>
            where TIcon : IHandle<HICON>
    {
        if (!PInvoke.DrawIconEx(
            context.Handle,
            location.X, location.Y,
            icon.Handle,
            size.Width, size.Height,
            0,
            HBRUSH.Null,
            flags))
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(context.Wrapper);
        GC.KeepAlive(icon.Wrapper);
    }

    /// <summary>
    ///  Creates a memory device context compatible with the specified source device context.
    /// </summary>
    /// <param name="context">The source device context that defines compatibility.</param>
    /// <returns>A new compatible memory device context that owns its native handle.</returns>
    public static DeviceContext CreateCompatibleDeviceContext<TDeviceContext>(this TDeviceContext context)
        where TDeviceContext : IHandle<HDC>
    {
        HDC hdc = PInvoke.CreateCompatibleDC(context.Handle);
        if (hdc.IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(context.Wrapper);
        return DeviceContext.Create(hdc, ownsHandle: true);
    }

    /// <summary>
    ///  Creates a bitmap compatible with the specified device context.
    /// </summary>
    /// <param name="context">The source device context that defines pixel format compatibility.</param>
    /// <param name="size">The bitmap dimensions, in pixels.</param>
    /// <returns>A bitmap that owns its native handle.</returns>
    public static Bitmap CreateCompatibleBitmap<T>(this T context, Size size) where T : IHandle<HDC>
    {
        HBITMAP hbitmap = PInvoke.CreateCompatibleBitmap(context.Handle, size.Width, size.Height);
        if (hbitmap.IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(context.Wrapper);
        return Bitmap.Create(hbitmap, ownsHandle: true);
    }

    /// <summary>
    ///  Offsets the logical window origin of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="x">The horizontal offset in logical units.</param>
    /// <param name="y">The vertical offset in logical units.</param>
    /// <returns><see langword="true"/> if the origin is updated; otherwise, <see langword="false"/>.</returns>
    public static bool OffsetWindowOrigin<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = PInvoke.OffsetWindowOrgEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Offsets the viewport origin of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="x">The horizontal offset in logical units.</param>
    /// <param name="y">The vertical offset in logical units.</param>
    /// <returns><see langword="true"/> if the origin is updated; otherwise, <see langword="false"/>.</returns>
    public static bool OffsetViewportOrigin<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = PInvoke.OffsetViewportOrgEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Gets the current logical window extents of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="size">Receives the logical window extents.</param>
    /// <returns><see langword="true"/> if extents are retrieved; otherwise, <see langword="false"/>.</returns>
    public static bool GetWindowExtents<T>(this T context, out Size size) where T : IHandle<HDC>
    {
        fixed (Size* s = &size)
        {
            bool success = PInvoke.GetWindowExtEx(context.Handle, (SIZE*)s);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    /// <summary>
    ///  Sets the logical ("window") dimensions of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="size">The new logical window extents.</param>
    /// <returns><see langword="true"/> if extents are updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetWindowExtents<T>(this T context, Size size) where T : IHandle<HDC>
    {
        bool success = PInvoke.SetWindowExtEx(context.Handle, size.Width, size.Height, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Gets the current viewport extents of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="size">Receives the viewport extents in device units.</param>
    /// <returns><see langword="true"/> if extents are retrieved; otherwise, <see langword="false"/>.</returns>
    public static bool GetViewportExtents<T>(this T context, out Size size) where T : IHandle<HDC>
    {
        fixed (Size* s = &size)
        {
            bool success = PInvoke.GetViewportExtEx(context.Handle, (SIZE*)s);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    /// <summary>
    ///  Sets the viewport extents of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="size">The new viewport extents in device units.</param>
    /// <returns><see langword="true"/> if extents are updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetViewportExtents<T>(this T context, Size size) where T : IHandle<HDC>
    {
        bool success = PInvoke.SetViewportExtEx(context.Handle, size.Width, size.Height, null);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Gets the current mapping mode for coordinate conversion.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <returns>The current mapping mode.</returns>
    public static MappingMode GetMappingMode<T>(this T context) where T : IHandle<HDC>
    {
        MappingMode result = (MappingMode)PInvoke.GetMapMode(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Sets the mapping mode that controls logical-to-device coordinate conversion.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="mapMode">The mapping mode to apply.</param>
    /// <returns>The previous mapping mode.</returns>
    public static MappingMode SetMappingMode<T>(this T context, MappingMode mapMode) where T : IHandle<HDC>
    {
        MappingMode result = (MappingMode)PInvoke.SetMapMode(context.Handle, (HDC_MAP_MODE)mapMode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Gets the current viewport origin of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="success">
    ///  Receives <see langword="true"/> if the origin was retrieved; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The viewport origin point in logical units.</returns>
    public static Point GetViewportOrigin<T>(this T context, out bool success)
        where T : IHandle<HDC>
    {
        Point point;
        success = PInvoke.GetViewportOrgEx(context.Handle, &point);
        GC.KeepAlive(context.Wrapper);
        return point;
    }

    /// <summary>
    ///  Sets the viewport origin of the device context.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="point">The new viewport origin in logical units.</param>
    /// <returns><see langword="true"/> if the origin is updated; otherwise, <see langword="false"/>.</returns>
    public static bool SetViewportOrigin<T>(this T context, Point point)
        where T : IHandle<HDC>
    {
        bool result = PInvoke.SetViewportOrgEx(context.Handle, point.X, point.Y, null);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Selects the clipping region for subsequent drawing operations.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="region">The clipping region to select, or null to clear clipping.</param>
    /// <returns>The resulting region complexity.</returns>
    public static RegionType SelectClippingRegion<T>(this T context, HRGN region)
        where T : IHandle<HDC>
    {
        RegionType type = (RegionType)PInvoke.SelectClipRgn(context.Handle, region);
        GC.KeepAlive(context.Wrapper);
        return type;
    }

    /// <inheritdoc cref="MoveTo{T}(T, int, int)"/>
    public static bool MoveTo<T>(this T context, Point point) where T : IHandle<HDC> =>
        context.MoveTo(point.X, point.Y);

    /// <summary>
    ///  Moves the current drawing position to the specified logical point.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="x">The x-coordinate in logical units.</param>
    /// <param name="y">The y-coordinate in logical units.</param>
    /// <returns><see langword="true"/> if the position is updated; otherwise, <see langword="false"/>.</returns>
    public static bool MoveTo<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool result = PInvoke.MoveToEx(context.Handle, x, y, null);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <inheritdoc cref="LineTo{T}(T, int, int)"/>
    public static bool LineTo<T>(this T context, Point point) where T : IHandle<HDC> =>
        context.LineTo(point.X, point.Y);

    /// <summary>
    ///  Draws a line from the current position to the specified logical point.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="x">The destination x-coordinate in logical units.</param>
    /// <param name="y">The destination y-coordinate in logical units.</param>
    /// <returns><see langword="true"/> if the line is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool LineTo<T>(this T context, int x, int y) where T : IHandle<HDC>
    {
        bool success = PInvoke.LineTo(context.Handle, x, y);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <inheritdoc cref="Ellipse{T}(T, int, int, int, int)"/>
    public static bool Ellipse<T>(this T context, Rectangle rectangle) where T : IHandle<HDC> =>
        context.Ellipse(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    /// <summary>
    ///  Draws an ellipse inside the bounding rectangle.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="left">The left edge of the bounding rectangle in logical units.</param>
    /// <param name="top">The top edge of the bounding rectangle in logical units.</param>
    /// <param name="right">The right edge of the bounding rectangle in logical units.</param>
    /// <param name="bottom">The bottom edge of the bounding rectangle in logical units.</param>
    /// <returns><see langword="true"/> if the ellipse is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool Ellipse<T>(this T context, int left, int top, int right, int bottom) where T : IHandle<HDC>
    {
        bool success = PInvoke.Ellipse(context.Handle, left, top, right, bottom);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <inheritdoc cref="PolyBezier{T}(T, ReadOnlySpan{Point})"/>
    public static bool PolyBezier<T>(this T context, params Point[] points) where T : IHandle<HDC> =>
        PolyBezier(context, points.AsSpan());

    /// <summary>
    ///  Draws one or more cubic Bezier splines defined by control points.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="points">The control points in logical units.</param>
    /// <returns><see langword="true"/> if the splines are drawn; otherwise, <see langword="false"/>.</returns>
    public static bool PolyBezier<T>(this T context, ReadOnlySpan<Point> points) where T : IHandle<HDC>
    {
        fixed (Point* p = points)
        {
            bool success = PInvoke.PolyBezier(context.Handle, p, (uint)points.Length);
            GC.KeepAlive(context.Wrapper);
            return success;
        }
    }

    /// <inheritdoc cref="Rectangle{T}(T, int, int, int, int)"/>
    public static bool Rectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC> =>
        context.Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

    /// <summary>
    ///  Draws a rectangle using the current pen and brush.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="left">The left edge in logical units.</param>
    /// <param name="top">The top edge in logical units.</param>
    /// <param name="right">The right edge in logical units.</param>
    /// <param name="bottom">The bottom edge in logical units.</param>
    /// <returns><see langword="true"/> if the rectangle is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool Rectangle<T>(this T context, int left, int top, int right, int bottom)
        where T : IHandle<HDC>
    {
        bool success = PInvoke.Rectangle(context.Handle, left, top, right, bottom);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <inheritdoc cref="RoundRectangle{T}(T, int, int, int, int, int, int)"/>
    public static bool RoundRectangle<T>(this T context, Rectangle rectangle, Size corner) where T : IHandle<HDC> =>
        context.RoundRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, corner.Width, corner.Height);

    /// <summary>
    ///  Draws a rectangle with rounded corners.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="left">The left edge in logical units.</param>
    /// <param name="top">The top edge in logical units.</param>
    /// <param name="right">The right edge in logical units.</param>
    /// <param name="bottom">The bottom edge in logical units.</param>
    /// <param name="width">The ellipse width used to round corners, in logical units.</param>
    /// <param name="height">The ellipse height used to round corners, in logical units.</param>
    /// <returns><see langword="true"/> if the shape is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool RoundRectangle<T>(this T context, int left, int top, int right, int bottom, int width, int height)
        where T : IHandle<HDC>
    {
        bool success = PInvoke.RoundRect(context.Handle, left, top, right, bottom, width, height);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Fills the specified rectangle with the given brush.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="rectangle">The rectangle to fill, in logical units.</param>
    /// <param name="hbrush">The brush handle used for filling.</param>
    /// <returns><see langword="true"/> if the rectangle is filled; otherwise, <see langword="false"/>.</returns>
    public static bool FillRectangle<T>(this T context, Rectangle rectangle, HBRUSH hbrush) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = (BOOL)PInvoke.FillRect(context.Handle, &rect, hbrush);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Draws the border of the specified rectangle using the given brush.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="rectangle">The rectangle whose frame is drawn, in logical units.</param>
    /// <param name="brush">The brush handle used for the frame.</param>
    /// <returns><see langword="true"/> if the frame is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool FrameRectangle<T>(this T context, Rectangle rectangle, HBRUSH brush) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = (BOOL)PInvoke.FrameRect(context.Handle, &rect, brush);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Inverts the colors in the specified rectangle.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="rectangle">The rectangle to invert, in logical units.</param>
    /// <returns><see langword="true"/> if the rectangle is inverted; otherwise, <see langword="false"/>.</returns>
    public static bool InvertRectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = PInvoke.InvertRect(context.Handle, &rect);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Draws a focus rectangle using the current display focus style.
    /// </summary>
    /// <param name="context">The device context to draw into.</param>
    /// <param name="rectangle">The focus rectangle bounds in logical units.</param>
    /// <returns><see langword="true"/> if the focus rectangle is drawn; otherwise, <see langword="false"/>.</returns>
    public static bool DrawFocusRectangle<T>(this T context, Rectangle rectangle) where T : IHandle<HDC>
    {
        RECT rect = rectangle;
        bool success = PInvoke.DrawFocusRect(context.Handle, &rect);
        GC.KeepAlive(context.Wrapper);
        return success;
    }

    /// <summary>
    ///  Sets the foreground raster-operation mix mode used by line and pen output.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <param name="foregroundMixMode">The new raster-operation mode.</param>
    /// <returns>The previous raster-operation mode.</returns>
    public static PenMixMode SetRasterOperation<T>(this T context, PenMixMode foregroundMixMode)
        where T : IHandle<HDC>
    {
        PenMixMode result = (PenMixMode)PInvoke.SetROP2(context.Handle, (R2_MODE)foregroundMixMode);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Gets the current foreground raster-operation mix mode.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <returns>The current raster-operation mode.</returns>
    public static PenMixMode GetRasterOperation<T>(this T context) where T : IHandle<HDC>
    {
        PenMixMode result = (PenMixMode)PInvoke.GetROP2(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return result;
    }

    /// <summary>
    ///  Gets the current DC brush color.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <returns>The current brush color as a <see cref="Color"/>.</returns>
    public static Color GetBrushColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF color = PInvoke.GetDCBrushColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return color;
    }

    /// <summary>
    ///  Gets the current text color.
    /// </summary>
    /// <param name="context">The target device context.</param>
    /// <returns>The current text color as a <see cref="Color"/>.</returns>
    public static Color GetTextColor<T>(this T context) where T : IHandle<HDC>
    {
        COLORREF color = PInvoke.GetTextColor(context.Handle);
        GC.KeepAlive(context.Wrapper);
        return color;
    }

    /// <inheritdoc cref="PInvoke.SetTextColor(HDC, COLORREF)"/>
    public static Color SetTextColor<T>(this T context, Color color) where T : IHandle<HDC>
    {
        COLORREF result = PInvoke.SetTextColor(context.Handle, (COLORREF)color);
        GC.KeepAlive(context.Wrapper);
        return result;
    }
}