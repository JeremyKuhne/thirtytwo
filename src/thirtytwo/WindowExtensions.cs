// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;

public static unsafe partial class WindowExtensions
{
    /// <inheritdoc cref="Interop.GetWindowText(HWND, PWSTR, int)"/>
    public static string GetWindowText<T>(this T window)
        where T : IHandle<HWND>
    {
        char[]? buffer = null;
        int length = Interop.GetWindowTextLength(window.Handle);
        if (length == 0)
        {
            return string.Empty;
        }

        do
        {
            if (buffer is not null)
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            buffer = ArrayPool<char>.Shared.Rent(length * 2);

            fixed (char* b = buffer)
            {
                length = Interop.GetWindowText(window.Handle, b, buffer.Length);
            }

            if (length == 0)
            {
                Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
            }
            else if (length == buffer.Length - 1)
            {
                length = Math.Min(length * 2, ushort.MaxValue);
                continue;
            }

            break;
        } while (true);

        string text = new(buffer, 0, length);
        ArrayPool<char>.Shared.Return(buffer);
        return text;
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.SetWindowText(HWND, PCWSTR)"/>
    public static void SetWindowText<T>(this T window, string text) where T : IHandle<HWND>
    {
        Error.ThrowLastErrorIfFalse(Interop.SetWindowText(window.Handle, text));
        GC.KeepAlive(window.Wrapper);
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.GetDpiForWindow(HWND)"/>
    public static uint GetDpiForWindow<T>(this T window) where T : IHandle<HWND>
    {
        uint dpi = Interop.GetDpiForWindow(window.Handle);
        GC.KeepAlive(window.Wrapper);
        return dpi;
    }

    /// <remarks>
    ///  <para>
    ///   Use in a <see langword="using"/> block to ensure the HDC is released.
    ///  </para>
    ///  <inheritdoc cref="Interop.GetDC(HWND)"/>
    /// </remarks>
    /// <inheritdoc cref="Interop.GetDC(HWND)"/>
    public static DeviceContext GetDeviceContext<T>(this T window) where T : IHandle<HWND>
    {
        DeviceContext context = DeviceContext.Create(Interop.GetDC(window.Handle), window.Handle);
        GC.KeepAlive(window.Wrapper);
        return context;
    }

    /// <inheritdoc cref="Interop.SendMessage(HWND, uint, WPARAM, LPARAM)"/>
    public static LRESULT SendMessage<T>(
        this T window,
        MessageType message,
        WPARAM wParam = default,
        LPARAM lParam = default) where T : IHandle<HWND>
    {
        LRESULT result = Interop.SendMessage(window.Handle, (uint)message, wParam, lParam);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Gets the font currently set for the window, if any.
    /// </summary>
    public static HFONT GetFontHandle<T>(this T window)
        where T : IHandle<HWND>
    {
        HFONT font = new(window.SendMessage(MessageType.GetFont));
        GC.KeepAlive(window.Wrapper);
        return font;
    }

    /// <summary>
    ///  Converts the requested point size to height based on the DPI of the given window.
    /// </summary>
    public static int FontPointSizeToHeight<T>(this T window, int pointSize) where T : IHandle<HWND>
    {
        int result = Interop.MulDiv(
            pointSize,
            (int)window.GetDpiForWindow(),
            72);

        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.GetClassLong(HWND, GET_CLASS_LONG_INDEX)"/>
    public static nuint GetClassLong<T>(this T window, GET_CLASS_LONG_INDEX index) where T : IHandle<HWND>
    {
        nuint result = Environment.Is64BitProcess
            ? Interop.GetClassLongPtr(window.Handle, index)
            : Interop.GetClassLong(window.Handle, index);

        if (result == 0)
        {
            Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
        }

        return result;
    }

    /// <inheritdoc cref="Interop.SetWindowLong(HWND, WINDOW_LONG_PTR_INDEX, int)" />
    public static nint SetWindowLong<T>(this T window, WINDOW_LONG_PTR_INDEX index, nint value)
        where T : IHandle<HWND>
    {
        nint result = Environment.Is64BitProcess
            ? Interop.SetWindowLongPtr(window.Handle, index, value)
            : Interop.SetWindowLong(window.Handle, index, (int)value);

        if (result == 0)
        {
            Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
        }

        return result;
    }

    /// <summary>
    ///  Wrapper to SetWindowLong for changing the window procedure. Returns the old
    ///  window procedure handle- use CallWindowProcedure to call the old method.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Make sure to root <paramref name="newCallback"/> to ensure that it doesn't get
    ///   collected.
    ///  </para>
    /// </remarks>
    public static WNDPROC SetWindowProcedure<T>(this T window, WindowProcedure newCallback)
        where T : IHandle<HWND>
    {
        // It is possible that the returned window procedure will not be a direct handle.
        return (WNDPROC)SetWindowLong(
            window,
            WINDOW_LONG_PTR_INDEX.GWL_WNDPROC,
            Marshal.GetFunctionPointerForDelegate(newCallback));
    }

    /// <summary>
    ///  Set the specified font for the window.
    /// </summary>
    public static unsafe LRESULT SetFont<T>(this T window, HFONT font)
        where T : IHandle<HWND>
    {
        return window.SendMessage(
            MessageType.SetFont,
            (WPARAM)font,
            (LPARAM)(BOOL)true);          // True to force a redraw
    }

    /// <inheritdoc cref="Interop.ScreenToClient(HWND, ref Point)"/>
    public static unsafe bool ScreenToClient<T>(this T window, ref Point point) where T : IHandle<HWND>
    {
        bool result = Interop.ScreenToClient(window.Handle, ref point);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Repositions the <paramref name="rectangle"/> location from screen to client coordinates.
    /// </summary>
    /// <inheritdoc cref="Interop.ScreenToClient(HWND, ref Point)"/>
    public static unsafe bool ScreenToClient<T>(this T window, ref Rectangle rectangle) where T : IHandle<HWND>
    {
        Point location = rectangle.Location;
        bool result = Interop.ScreenToClient(window.Handle, &location);
        rectangle.Location = location;
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.ClientToScreen(HWND, ref Point)"/>
    public static unsafe bool ClientToScreen<T>(this T window, ref Point point) where T : IHandle<HWND>
    {
        bool result = Interop.ClientToScreen(window.Handle, ref point);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.GetClientRect(HWND, out RECT)"/>
    public static unsafe Rectangle GetClientRectangle<T>(this T window)
        where T : IHandle<HWND>
    {
        Unsafe.SkipInit(out RECT rect);
        Error.ThrowLastErrorIfFalse(Interop.GetClientRect(window.Handle, &rect));
        GC.KeepAlive(window.Wrapper);
        return rect;
    }

    /// <summary>
    ///  Enumerates child windows for the given <paramref name="parent"/>.
    /// </summary>
    /// <param name="callback">
    ///  The provided function will be passed child window handles. Return true to continue enumeration.
    /// </param>
    public static void EnumerateChildWindows<T>(
        this T parent,
        Func<HWND, bool> callback) where T : IHandle<HWND>
    {
        using var enumerator = new ChildWindowEnumerator(parent.Handle, callback);
        GC.KeepAlive(parent.Wrapper);
    }

    public static bool ShowWindow<T>(this T window, ShowWindowCommand command)
        where T : IHandle<HWND>
    {
        bool result = Interop.ShowWindow(window.Handle, (SHOW_WINDOW_CMD)command);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Moves the window to the requested location. For main windows this is in screen coordinates. For child
    ///  windows this is relative to the client area of the parent window.
    /// </summary>
    public static void MoveWindow<T>(this T window, Rectangle position, bool repaint)
        where T : IHandle<HWND>
    {
        Error.ThrowLastErrorIfFalse(
            Interop.MoveWindow(window.Handle, position.X, position.Y, position.Width, position.Height, repaint));
        GC.KeepAlive(window.Wrapper);
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.UpdateWindow(HWND)"/>
    public static void UpdateWindow<T>(this T window) where T : IHandle<HWND>
    {
        Error.ThrowLastErrorIfFalse(Interop.UpdateWindow(window.Handle));
        GC.KeepAlive(window.Wrapper);
    }

    public static unsafe MessageBoxResult MessageBox<T>(
        this T owner,
        string text,
        string caption,
        MessageBoxStyle style = MessageBoxStyle.Ok)
        where T : IHandle<HWND>
    {
        MessageBoxResult result = (MessageBoxResult)Interop.MessageBoxEx(
            owner.Handle,
            text,
            caption,
            (MESSAGEBOX_STYLE)style,
            wLanguageId: 0);

        if (result == 0)
        {
            Error.ThrowLastError();
        }

        GC.KeepAlive(owner.Wrapper);
        return result;
    }

    public static unsafe DeviceContext BeginPaint<T>(this T window)
        where T : IHandle<HWND>
    {
        PAINTSTRUCT paintStruct = default;
        Interop.BeginPaint(window.Handle, &paintStruct);
        DeviceContext context = DeviceContext.Create(ref paintStruct, window);
        GC.KeepAlive(window.Wrapper);
        return context;
    }
}
