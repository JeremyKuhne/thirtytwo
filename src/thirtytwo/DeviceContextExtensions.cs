// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Numerics;
using Windows.Support;

namespace Windows;

public static unsafe partial class DeviceContextExtensions
{
    /// <inheritdoc cref="Interop.GetGraphicsMode(HDC)"/>
    public static GRAPHICS_MODE GetGraphicsMode<T>(this T deviceContext)
        where T : IHandle<HDC>
    {
        GRAPHICS_MODE mode = (GRAPHICS_MODE)Interop.GetGraphicsMode(deviceContext.Handle);
        GC.KeepAlive(deviceContext.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.SetGraphicsMode(HDC, GRAPHICS_MODE)"/>
    public static GRAPHICS_MODE SetGraphicsMode<T>(this T deviceContext, GRAPHICS_MODE mode)
        where T : IHandle<HDC>
    {
        mode = (GRAPHICS_MODE)Interop.SetGraphicsMode(deviceContext.Handle, mode);
        GC.KeepAlive(deviceContext.Wrapper);
        return mode;
    }

    /// <inheritdoc cref="Interop.SetWorldTransform(HDC, XFORM*)"/>
    public static unsafe bool SetWorldTransform<T>(this T deviceContext, ref Matrix3x2 transform)
        where T : IHandle<HDC>
    {
        fixed (Matrix3x2* t = &transform)
        {
            bool result = Interop.SetWorldTransform(deviceContext.Handle, (XFORM*)t);
            GC.KeepAlive(deviceContext.Wrapper);
            return result;
        }
    }

    /// <summary>
    ///  Converts the requested point size to height based on the DPI of the given device context.
    /// </summary>
    public static int FontPointSizeToHeight<T>(this T deviceContext, int pointSize)
        where T : IHandle<HDC>
    {
        Application.EnsureDpiAwareness();
        int result = Interop.MulDiv(
           pointSize,
           Interop.GetDeviceCaps(deviceContext.Handle, GET_DEVICE_CAPS_INDEX.LOGPIXELSY),
           72);

        GC.KeepAlive(deviceContext.Wrapper);
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
        this TDeviceContext deviceContext,
        TIcon icon,
        Point location,
        Size size = default,
        DI_FLAGS flags = DI_FLAGS.DI_NORMAL)
            where TDeviceContext : IHandle<HDC>
            where TIcon : IHandle<HICON>
    {
        if (!Interop.DrawIconEx(deviceContext.Handle, location.X, location.Y, icon.Handle, size.Width, size.Height, 0, default, flags))
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(deviceContext.Wrapper);
        GC.KeepAlive(icon.Wrapper);
    }

    public static DeviceContext CreateCompatibleDeviceContext<TDeviceContext>(this TDeviceContext deviceContext)
        where TDeviceContext : IHandle<HDC>
    {
        HDC hdc = Interop.CreateCompatibleDC(deviceContext.Handle);
        if (hdc.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(deviceContext.Wrapper);
        return DeviceContext.Create(hdc, ownsHandle: true);
    }

    public static Bitmap CreateCompatibleBitmap<TDeviceContext>(this TDeviceContext deviceContext, Size size)
        where TDeviceContext : IHandle<HDC>
    {
        HBITMAP hbitmap = Interop.CreateCompatibleBitmap(deviceContext.Handle, size.Width, size.Height);
        if (hbitmap.IsNull)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(deviceContext.Wrapper);
        return Bitmap.Create(hbitmap, ownsHandle: true);
    }
}