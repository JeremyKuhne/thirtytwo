// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;

/// <summary>
///  Provides Win32 window helper extension methods for handle-wrapper types.
/// </summary>
public static unsafe partial class WindowExtensions
{
    /// <inheritdoc cref="Interop.GetWindowText(HWND, PWSTR, int)"/>
    public static string GetWindowText<T>(this T window) where T : IHandle<HWND>
    {
        int length = PInvoke.GetWindowTextLength(window.Handle);
        if (length == 0)
        {
            WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
            return string.Empty;
        }

        using BufferScope<char> buffer = new(stackalloc char[128]);

        do
        {
            buffer.EnsureCapacity(length);

            fixed (char* b = buffer)
            {
                length = PInvoke.GetWindowText(window.Handle, b, buffer.Length);
            }

            if (length == 0)
            {
                WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
            }
            else if (length == buffer.Length - 1)
            {
                length = Math.Min(length * 2, ushort.MaxValue);
                continue;
            }

            return buffer[..length].ToString();
        } while (true);
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.SetWindowText(HWND, PCWSTR)"/>
    public static void SetWindowText<T>(this T window, string text) where T : IHandle<HWND>
    {
        PInvoke.SetWindowText(window.Handle, text).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.GetDpiForWindow(HWND)"/>
    public static uint GetDpi<T>(this T window) where T : IHandle<HWND>
    {
        uint dpi = PInvoke.GetDpiForWindow(window.Handle);
        GC.KeepAlive(window.Wrapper);
        return dpi;
    }

    /// <summary>
    ///  Gets the scale factor for the window based on its DPI.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The scale factor where 1.0 corresponds to 96 DPI.</returns>
    public static float GetScale<T>(this T window) where T : IHandle<HWND>
    {
        // Scale is the DPI divided by 96.
        uint dpi = window.GetDpi();
        GC.KeepAlive(window.Wrapper);

        if (dpi <= 0)
        {
            Debug.Fail("Window handle is invalid.");
            return 1.0f;
        }

        // Optimize for common values
        return dpi switch
        {
            Interop.USER_DEFAULT_SCREEN_DPI => 1.0f,
            (int)(Interop.USER_DEFAULT_SCREEN_DPI * 1.25f) => 1.25f,
            (int)(Interop.USER_DEFAULT_SCREEN_DPI * 1.5f) => 1.5f,
            (int)(Interop.USER_DEFAULT_SCREEN_DPI * 1.75f) => 1.75f,
            Interop.USER_DEFAULT_SCREEN_DPI * 2 => 2.0f,
            _ => (float)(dpi / (float)PInvoke.USER_DEFAULT_SCREEN_DPI)
        };
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
        DeviceContext context = DeviceContext.Create(PInvoke.GetDC(window.Handle), window.Handle);
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
        LRESULT result = PInvoke.SendMessage(window.Handle, (uint)message, wParam, lParam);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.PostMessage(HWND, uint, WPARAM, LPARAM)"/>
    public static void PostMessage<T>(
        this T window,
        MessageType message,
        WPARAM wParam = default,
        LPARAM lParam = default) where T : IHandle<HWND>
    {
        PInvoke.PostMessage(window.Handle, (uint)message, wParam, lParam).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }

    /// <summary>
    ///  Gets the font currently set for the window, if any.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The currently associated font handle, or <see cref="HFONT.Null"/>.</returns>
    public static HFONT GetFontHandle<T>(this T window) where T : IHandle<HWND>
    {
        HFONT font = new(window.SendMessage(MessageType.GetFont));
        GC.KeepAlive(window.Wrapper);
        return font;
    }

    /// <summary>
    ///  Converts the requested point size to height based on the DPI of the given window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="pointSize">The point size to convert.</param>
    /// <returns>The corresponding logical font height.</returns>
    public static int FontPointSizeToHeight<T>(this T window, int pointSize) where T : IHandle<HWND>
    {
        int result = PInvoke.MulDiv(
            pointSize,
            (int)window.GetDpi(),
            72);

        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.GetClassLong(HWND, GET_CLASS_LONG_INDEX)"/>
    public static nuint GetClassLong<T>(this T window, GET_CLASS_LONG_INDEX index) where T : IHandle<HWND>
    {
        nuint result = Environment.Is64BitProcess
            ? PInvoke.GetClassLongPtr(window.Handle, index)
            : PInvoke.GetClassLong(window.Handle, index);

        if (result == 0)
        {
            WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
        }

        return result;
    }

    /// <inheritdoc cref="Interop.SetClassLong(HWND, GET_CLASS_LONG_INDEX, int)"/>
    public static nuint SetClassLong<T>(this T window, GET_CLASS_LONG_INDEX index, nint value) where T : IHandle<HWND>
    {
        nuint result = Environment.Is64BitProcess
            ? PInvoke.SetClassLongPtr(window.Handle, index, value)
            : PInvoke.SetClassLong(window.Handle, index, (int)value);

        if (result == 0)
        {
            WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
        }

        return result;
    }

    /// <inheritdoc cref="GetWindowLong{T}(T, WINDOW_LONG_PTR_INDEX)"/>
    public static nint GetWindowLong<T>(this T window, int index) where T : IHandle<HWND> =>
        GetWindowLong(window, (WINDOW_LONG_PTR_INDEX)index);

    /// <inheritdoc cref="Interop.GetWindowLong(HWND, WINDOW_LONG_PTR_INDEX)" />
    public static nint GetWindowLong<T>(this T window, WINDOW_LONG_PTR_INDEX index)
        where T : IHandle<HWND>
    {
        nint result = Environment.Is64BitProcess
            ? PInvoke.GetWindowLongPtr(window.Handle, index)
            : PInvoke.GetWindowLong(window.Handle, index);

        if (result == 0)
        {
            WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
        }

        return result;
    }

    /// <inheritdoc cref="Interop.SetWindowLong(HWND, WINDOW_LONG_PTR_INDEX, int)" />
    public static nint SetWindowLong<T>(this T window, WINDOW_LONG_PTR_INDEX index, nint value)
        where T : IHandle<HWND>
    {
        nint result = Environment.Is64BitProcess
            ? PInvoke.SetWindowLongPtr(window.Handle, index, value)
            : PInvoke.SetWindowLong(window.Handle, index, (int)value);

        if (result == 0)
        {
            WIN32_ERROR.ERROR_SUCCESS.ThrowIfLastErrorNot();
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
    /// <param name="window">The target window.</param>
    /// <param name="newCallback">The replacement managed window procedure delegate.</param>
    /// <returns>The previously installed native window procedure pointer.</returns>
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
    /// <param name="window">The target window.</param>
    /// <param name="font">The font handle to assign.</param>
    /// <returns>The message result from <see cref="MessageType.SetFont"/> processing.</returns>
    public static LRESULT SetFontHandle<T>(this T window, HFONT font)
        where T : IHandle<HWND>
    {
        return window.SendMessage(
            MessageType.SetFont,
            (WPARAM)font,
            (LPARAM)(BOOL)true);          // True to force a redraw
    }

    /// <inheritdoc cref="Interop.ScreenToClient(HWND, ref Point)"/>
    public static bool ScreenToClient<T>(this T window, ref Point point) where T : IHandle<HWND>
    {
        bool result = PInvoke.ScreenToClient(window.Handle, ref point);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Repositions the <paramref name="rectangle"/> location from screen to client coordinates.
    /// </summary>
    /// <inheritdoc cref="Interop.ScreenToClient(HWND, ref Point)"/>
    public static bool ScreenToClient<T>(this T window, ref Rectangle rectangle) where T : IHandle<HWND>
    {
        Point location = rectangle.Location;
        bool result = PInvoke.ScreenToClient(window.Handle, &location);
        rectangle.Location = location;
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <inheritdoc cref="Interop.ClientToScreen(HWND, ref Point)"/>
    public static bool ClientToScreen<T>(this T window, ref Point point) where T : IHandle<HWND>
    {
        bool result = PInvoke.ClientToScreen(window.Handle, ref point);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Repositions the <paramref name="rectangle"/> location from client to screen coordinates.
    /// </summary>
    /// <inheritdoc cref="Interop.ClientToScreen(HWND, ref Point)"/>
    public static bool ClientToScreen<T>(this T window, ref Rectangle rectangle) where T : IHandle<HWND>
    {
        Point location = rectangle.Location;
        bool result = PInvoke.ClientToScreen(window.Handle, ref location);
        rectangle.Location = location;
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.GetClientRect(HWND, out RECT)"/>
    public static Rectangle GetClientRectangle<T>(this T window)
        where T : IHandle<HWND>
    {
        Unsafe.SkipInit(out RECT rect);
        PInvoke.GetClientRect(window.Handle, &rect).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
        return rect;
    }

    /// <summary>
    ///  Dimensions of the bounding rectangle of the specified <paramref name="window"/>
    ///  in screen coordinates relative to the upper-left corner.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The window rectangle in screen coordinates.</returns>
    public static Rectangle GetWindowRectangle<T>(this T window) where T : IHandle<HWND>
    {
        Unsafe.SkipInit(out RECT rect);
        PInvoke.GetWindowRect(window.Handle, &rect).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
        return rect;
    }

    /// <summary>
    ///  Converts (maps) a set of points from a coordinate space relative to one window to a coordinate space
    ///  relative to another window.
    /// </summary>
    /// <param name="from">The source coordinate-space window.</param>
    /// <param name="to">The destination coordinate-space window.</param>
    /// <param name="rectangle">The rectangle to map.</param>
    /// <returns>The mapped rectangle.</returns>
    public static Rectangle MapTo<TFrom, TTo>(this TFrom from, TTo to, Rectangle rectangle)
        where TFrom : IHandle<HWND> where TTo : IHandle<HWND>
    {
        RECT rect = rectangle;
        _ = PInvoke.MapWindowPoints(from.Handle, to.Handle, (Point*)&rect, 2);
        GC.KeepAlive(to.Wrapper);
        GC.KeepAlive(from.Wrapper);
        return rect;
    }

    /// <summary>
    ///  Converts (maps) a set of points from a coordinate space relative to one window to a coordinate space
    ///  relative to another window.
    /// </summary>
    /// <param name="to">The destination coordinate-space window.</param>
    /// <param name="from">The source coordinate-space window.</param>
    /// <param name="rectangle">The rectangle to map.</param>
    /// <returns>The mapped rectangle.</returns>
    public static Rectangle MapFrom<TFrom, TTo>(this TTo to, TFrom from, Rectangle rectangle)
        where TFrom : IHandle<HWND> where TTo : IHandle<HWND>
    {
        RECT rect = rectangle;
        _ = PInvoke.MapWindowPoints(from.Handle, to.Handle, (Point*)&rect, 2);
        GC.KeepAlive(to.Wrapper);
        GC.KeepAlive(from.Wrapper);
        return rect;
    }

    /// <summary>
    ///  Gets the native parent window handle.
    /// </summary>
    /// <param name="child">The child window.</param>
    /// <returns>The parent window handle, or <see cref="HWND.Null"/>.</returns>
    public static HWND GetParent<T>(this T child) where T : IHandle<HWND>
    {
        HWND parent = PInvoke.GetParent(child.Handle);
        GC.KeepAlive(child.Wrapper);
        return parent;
    }

    /// <summary>
    ///  Enumerates child windows for the given <paramref name="parent"/>.
    /// </summary>
    /// <param name="parent">The parent whose child windows are enumerated.</param>
    /// <param name="callback">
    ///  The provided function will be passed child window handles. Return <see langword="true"/> to continue enumeration.
    /// </param>
    public static void EnumerateChildWindows<T>(
        this T parent,
        Func<HWND, bool> callback) where T : IHandle<HWND>
    {
        using var enumerator = new ChildWindowEnumerator(parent.Handle, callback);
        GC.KeepAlive(parent.Wrapper);
    }

    /// <summary>
    ///  Shows, hides, minimizes, or restores a window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="command">The show-state command to apply.</param>
    /// <returns>The previous visibility state as reported by USER32.</returns>
    public static bool ShowWindow<T>(this T window, ShowWindowCommand command = ShowWindowCommand.Show)
        where T : IHandle<HWND>
    {
        bool result = PInvoke.ShowWindow(window.Handle, (SHOW_WINDOW_CMD)command);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Moves the window to the requested location. For main windows this is in screen coordinates. For child
    ///  windows this is relative to the client area of the parent window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="position">The requested bounds.</param>
    /// <param name="repaint"><see langword="true"/> to repaint after moving.</param>
    public static void MoveWindow<T>(this T window, Rectangle position, bool repaint)
        where T : IHandle<HWND>
    {
        PInvoke.MoveWindow(
            window.Handle,
            position.X,
            position.Y,
            position.Width,
            position.Height,
            repaint).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }

    /// <summary>
    ///  Changes a window's bounds and special z-order position.
    /// </summary>
    /// <param name="window">The window to position.</param>
    /// <param name="zOrder">The special z-order position.</param>
    /// <param name="position">
    ///  The requested bounds in screen coordinates for top-level windows or parent-client coordinates for child
    ///  windows. Individual values are ignored when the corresponding flags are set.
    /// </param>
    /// <param name="flags">Controls which position, size, activation, and redraw changes are applied.</param>
    /// <remarks>
    ///  When <paramref name="flags"/> includes <see cref="WindowPositionFlags.AsyncWindowPosition"/>, the caller is
    ///  responsible for keeping the target HWND valid until its owning thread applies the request.
    /// </remarks>
    public static void SetWindowPosition<T>(
        this T window,
        WindowZOrder zOrder,
        Rectangle position,
        WindowPositionFlags flags = WindowPositionFlags.None)
        where T : IHandle<HWND>
    {
        HWND insertAfter = zOrder switch
        {
            WindowZOrder.Top => HWND.HWND_TOP,
            WindowZOrder.Bottom => HWND.HWND_BOTTOM,
            WindowZOrder.TopMost => HWND.HWND_TOPMOST,
            WindowZOrder.NotTopMost => HWND.HWND_NOTOPMOST,
            _ => throw new ArgumentOutOfRangeException(nameof(zOrder), zOrder, "Unknown z-order position.")
        };

        SetWindowPosition(window, insertAfter, position, flags);
    }

    /// <summary>
    ///  Changes a window's bounds and positions it behind a sibling in native z-order.
    /// </summary>
    /// <param name="window">The window to position.</param>
    /// <param name="insertAfter">The sibling that should immediately precede <paramref name="window"/>.</param>
    /// <param name="position">
    ///  The requested bounds in screen coordinates for top-level windows or parent-client coordinates for child
    ///  windows. Individual values are ignored when the corresponding flags are set.
    /// </param>
    /// <param name="flags">Controls which position, size, activation, and redraw changes are applied.</param>
    /// <remarks>
    ///  When <paramref name="flags"/> includes <see cref="WindowPositionFlags.AsyncWindowPosition"/>, the caller is
    ///  responsible for keeping both HWNDs valid until the target's owning thread applies the request.
    /// </remarks>
    public static void SetWindowPosition<T, TInsertAfter>(
        this T window,
        TInsertAfter insertAfter,
        Rectangle position,
        WindowPositionFlags flags = WindowPositionFlags.None)
        where T : IHandle<HWND>
        where TInsertAfter : IHandle<HWND>
    {
        ArgumentNullException.ThrowIfNull(insertAfter);
        SetWindowPosition(window, insertAfter.Handle, position, flags);
        GC.KeepAlive(insertAfter.Wrapper);
    }

    /// <summary>
    ///  Gets a window related through native hierarchy or z-order.
    /// </summary>
    /// <param name="window">The window from which to navigate.</param>
    /// <param name="relationship">The relationship to query.</param>
    /// <returns>
    ///  A borrowed snapshot of the related window, or <see cref="HWND.Null"/> when no such window exists and USER32
    ///  reports success. For <see cref="WindowRelationship.EnabledPopup"/>, USER32 returns <paramref name="window"/>
    ///  itself when no enabled owned popup exists.
    /// </returns>
    public static HWND GetRelatedWindow<T>(this T window, WindowRelationship relationship)
        where T : IHandle<HWND>
    {
        Marshal.SetLastPInvokeError(0);
        HWND related = PInvoke.GetWindow(window.Handle, (GET_WINDOW_CMD)relationship);
        if (related.IsNull)
        {
            Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
        }

        GC.KeepAlive(window.Wrapper);
        return related;
    }

    private static void SetWindowPosition<T>(
        T window,
        HWND insertAfter,
        Rectangle position,
        WindowPositionFlags flags)
        where T : IHandle<HWND>
    {
        PInvoke.SetWindowPos(
            window.Handle,
            insertAfter,
            position.X,
            position.Y,
            position.Width,
            position.Height,
            (SET_WINDOW_POS_FLAGS)flags).ThrowLastErrorIfFalse();

        GC.KeepAlive(window.Wrapper);
    }

    /// <returns/>
    /// <inheritdoc cref="Interop.UpdateWindow(HWND)"/>
    public static void UpdateWindow<T>(this T window) where T : IHandle<HWND>
    {
        PInvoke.UpdateWindow(window.Handle).ThrowLastErrorIfFalse();
        GC.KeepAlive(window.Wrapper);
    }

    private const MessageBoxStyles TaskDialogValidMessageBoxStyles =
        MessageBoxStyles.Ok
        | MessageBoxStyles.OkCancel
        | MessageBoxStyles.RetryCancel
        | MessageBoxStyles.YesNo
        | MessageBoxStyles.YesNoCancel
        | MessageBoxStyles.IconAsterisk
        | MessageBoxStyles.IconExclamation
        | MessageBoxStyles.IconInformation
        | MessageBoxStyles.IconError;

    /// <summary>
    ///  Shows a task dialog.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="mainInstruction">Primary instruction text shown prominently.</param>
    /// <param name="content">Additional content text.</param>
    /// <param name="title">Dialog caption text.</param>
    /// <param name="buttons">Standard command buttons to display.</param>
    /// <param name="icon">Optional predefined main icon.</param>
    /// <returns>The command result selected by the user.</returns>
    public static DialogResult ShowTaskDialog<T>(
        this T owner,
        string? mainInstruction = null,
        string? content = null,
        string? title = null,
        TaskDialogButtons buttons = TaskDialogButtons.Ok,
        TaskDialogIcon? icon = null)
        where T : IHandle<HWND>
    {
        using var themeScope = Application.ThemingScope;
        Application.EnsureDpiAwareness();

        fixed (char* mi = mainInstruction)
        fixed (char* c = content)
        fixed (char* t = title)
        {
            int button;
            TASKDIALOGCONFIG config = new()
            {
                cbSize = (uint)sizeof(TASKDIALOGCONFIG),
                hwndParent = owner.Handle,
                dwFlags = TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION
                    | TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN
                    | TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON
                    | TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA,
                dwCommonButtons = (TASKDIALOG_COMMON_BUTTON_FLAGS)buttons,
                pszWindowTitle = t,
                pszMainInstruction = mi,
                pszContent = c,
            };

            if (icon.HasValue)
            {
                config.Anonymous1.pszMainIcon = (char*)(nint)icon.Value;
            }

            PInvoke.TaskDialogIndirect(
                &config,
                &button,
                null,
                null).ThrowOnFailure();

            return (DialogResult)button;
        }
    }

    /// <summary>
    ///  Shows a legacy message box.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="text">Message text.</param>
    /// <param name="caption">Caption text.</param>
    /// <param name="style">Button and icon style flags.</param>
    /// <returns>The button selected by the user.</returns>
    public static DialogResult MessageBox<T>(
        this T owner,
        string text,
        string caption,
        MessageBoxStyles style = MessageBoxStyles.Ok)
        where T : IHandle<HWND>
    {
        using var themeScope = Application.ThemingScope;
        Application.EnsureDpiAwareness();

        DialogResult result = (DialogResult)PInvoke.MessageBoxEx(
            owner.Handle,
            text,
            caption,
            (MESSAGEBOX_STYLE)style,
            wLanguageId: 0);

        if (result == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(owner.Wrapper);
        return result;
    }

    /// <inheritdoc cref="DeviceContext.BeginPaint{THwnd}(THwnd, bool, out Rectangle)"/>
    public static DeviceContext BeginPaint<T>(this T window, bool saveContext = true)
        where T : IHandle<HWND>
        => window.BeginPaint(saveContext, out _);

    /// <inheritdoc cref="DeviceContext.BeginPaint{THwnd}(THwnd, bool, out Rectangle)"/>
    public static DeviceContext BeginPaint<T>(this T window, out Rectangle paintBounds)
        where T : IHandle<HWND>
        => window.BeginPaint(saveContext: true, out paintBounds);

    /// <inheritdoc cref="DeviceContext.BeginPaint{THwnd}(THwnd, bool, out Rectangle)"/>
    public static DeviceContext BeginPaint<T>(this T window, bool saveContext, out Rectangle paintBounds)
        where T : IHandle<HWND> => DeviceContext.BeginPaint(window, saveContext, out paintBounds);

    /// <summary>
    ///  Invalidates a rectangle in the client area.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="rectangle">The client rectangle to invalidate.</param>
    /// <param name="erase"><see langword="true"/> to request background erasing.</param>
    /// <returns><see langword="true"/> on success.</returns>
    public static bool InvalidateRectangle<T>(this T window, Rectangle rectangle, bool erase)
        where T : IHandle<HWND>
    {
        RECT rect = rectangle;
        bool result = PInvoke.InvalidateRect(window.Handle, &rect, erase);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Invalidates the entire client area.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="erase"><see langword="true"/> to request background erasing.</param>
    /// <returns><see langword="true"/> on success.</returns>
    public static bool Invalidate<T>(this T window, bool erase = true)
        where T : IHandle<HWND>
    {
        bool result = PInvoke.InvalidateRect(window.Handle, (RECT*)null, erase);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Binds the given layout <paramref name="handler"/> to the window. This will call
    ///  <see cref="ILayoutHandler.Layout(Rectangle, float)"/> with the window's client rectangle whenever the window's
    ///  position or size changes.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This is the preferred way to handle dynamic layout in a window. Use the <see cref="Layout"/> class to
    ///   create layout handlers that perform the desired layout within the window. For example, in the constructor
    ///   of a <see cref="Window"/> you can split two text controls horizontally:
    ///  </para>
    ///  <code>
    ///   <![CDATA[
    ///     this.AddLayoutHandler(Layout.Vertical(
    ///       (.5f, _textBox1),
    ///       (.5f, _textBox2)));
    ///   ]]>
    ///  </code>
    /// </remarks>
    /// <param name="window">The target window.</param>
    /// <param name="handler">The layout callback implementation.</param>
    /// <returns>A binder that controls the registration lifetime.</returns>
    public static LayoutBinder AddLayoutHandler(this Window window, ILayoutHandler handler)
        => new(window, handler);

    /// <returns/>
    /// <inheritdoc cref="Interop.GetWindowRgn(HWND, HRGN)"/>
    public static HRGN GetWindowRegion<T>(this T window)
        where T : IHandle<HWND>
        => GetWindowRegion(window, out _);

    /// <returns/>
    /// <inheritdoc cref="Interop.GetWindowRgn(HWND, HRGN)"/>
    public static HRGN GetWindowRegion<T>(this T window, out GDI_REGION_TYPE type)
        where T : IHandle<HWND>
    {
        HRGN region = PInvoke.CreateRectRgn(0, 0, 0, 0);
        type = PInvoke.GetWindowRgn(window.Handle, region);
        GC.KeepAlive(window.Wrapper);
        return region;
    }

    /// <summary>
    ///  Set the window region. Windows takes ownership of the given <paramref name="region"/>, do not free it.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="region">The region handle to assign.</param>
    /// <param name="redraw"><see langword="true"/> to redraw after applying the region.</param>
    public static void SetWindowRegion<T>(this T window, HRGN region, bool redraw = false)
        where T : IHandle<HWND>
    {
        if (PInvoke.SetWindowRgn(window.Handle, region, redraw) == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(window.Wrapper);
    }

    /// <summary>
    ///  Creates a timer.
    /// </summary>
    /// <param name="window">Optional window to be associated with the timer.</param>
    /// <param name="id">Existing timer id to set a new timeout.</param>
    /// <param name="interval">Interval in milliseconds.</param>
    /// <param name="callback">Optional callback. Ensure the callback stays rooted while the timer is active.</param>
    /// <param name="delayTolerance">Delay tolerance in milliseconds (to improve power consumption).</param>
    /// <returns>The timer identifier.</returns>
    public static nuint SetTimer<T>(
        this T window,
        uint interval,
        nuint id = 0,
        TimerProcedure? callback = null,
        uint delayTolerance = 0) where T : IHandle<HWND>
    {
        void* cb = callback is null ? null : (void*)Marshal.GetFunctionPointerForDelegate(callback);
        nuint result = PInvoke.SetCoalescableTimer(
            window.Handle,
            id,
            interval,
            (delegate* unmanaged[Stdcall]<HWND, uint, nuint, uint, void>)cb,
            delayTolerance);

        if (result == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Stops a timer associated with a window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="id">The timer identifier to stop.</param>
    public static void KillTimer<T>(this T window, nuint id) where T : IHandle<HWND>
    {
        bool success = PInvoke.KillTimer(window.Handle, id);
        if (!success)
        {
            WIN32_ERROR.NO_ERROR.ThrowIfLastErrorNot();
        }

        GC.KeepAlive(window.Wrapper);
    }

    /// <summary>
    ///  Sets keyboard focus to the target window.
    /// </summary>
    /// <param name="window">The window that should receive focus.</param>
    /// <returns>The window that previously had focus, or <see cref="HWND.Null"/>.</returns>
    public static HWND SetFocus<T>(this T window) where T : IHandle<HWND>
    {
        HWND prior = PInvoke.SetFocus(window.Handle);
        if (prior.IsNull)
        {
            WIN32_ERROR.NO_ERROR.ThrowIfLastErrorNot();
        }

        GC.KeepAlive(window.Wrapper);
        return prior;
    }

    /// <summary>
    ///  Gets a child dialog item by identifier.
    /// </summary>
    /// <param name="window">The dialog or parent window.</param>
    /// <param name="id">The child control identifier.</param>
    /// <returns>The child control handle.</returns>
    public static HWND GetDialogItem<T>(this T window, int id) where T : IHandle<HWND>
    {
        HWND control = PInvoke.GetDlgItem(window.Handle, id);
        if (control.IsNull)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        return control;
    }

    /// <summary>
    ///  Gets the dialog/control identifier of a child window.
    /// </summary>
    /// <param name="window">The child window.</param>
    /// <returns>The current dialog/control identifier.</returns>
    public static int GetDialogControlId<T>(this T window) where T : IHandle<HWND>
    {
        // GWLP_ID is the control ID or the handle to the menu, depending on whether the window has the WS_CHILD style.
        // Using this API you'll only get the control ID or 0 if it is not a child control (as of 20H2).
        int id = PInvoke.GetDlgCtrlID(window.Handle);
        if (id == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        GC.KeepAlive(window.Wrapper);
        return id;
    }

    /// <summary>
    ///  Sets the dialog/control identifier on a child window.
    /// </summary>
    /// <param name="window">The child window.</param>
    /// <param name="id">The identifier to assign.</param>
    /// <returns>The previous identifier value.</returns>
    public static int SetDialogControlId<T>(this T window, int id) where T : IHandle<HWND>
    {
        if (!window.IsChildWindow())
        {
            throw new InvalidOperationException("Cannot set the control ID on a top level window.");
        }

        int result = (int)window.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_ID, id);
        GC.KeepAlive(window.Wrapper);
        return result;
    }

    /// <summary>
    ///  Gets whether a window has the <c>WS_CHILD</c> style.
    /// </summary>
    /// <param name="window">The window to inspect.</param>
    /// <returns><see langword="true"/> when the window is a child window.</returns>
    public static bool IsChildWindow<T>(this T window) where T : IHandle<HWND> =>
        window.GetWindowStyle().HasFlag(WindowStyles.Child);

    /// <summary>
    ///  Gets the current window style flags.
    /// </summary>
    /// <param name="window">The window to inspect.</param>
    /// <returns>The current <see cref="WindowStyles"/> bit flags.</returns>
    public static WindowStyles GetWindowStyle<T>(this T window) where T : IHandle<HWND> =>
        (WindowStyles)window.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE);

    /// <summary>
    ///  Gets the current extended window style flags.
    /// </summary>
    /// <param name="window">The window to inspect.</param>
    /// <returns>The current <see cref="ExtendedWindowStyles"/> bit flags.</returns>
    public static ExtendedWindowStyles GetExtendedWindowStyle<T>(this T window) where T : IHandle<HWND> =>
        (ExtendedWindowStyles)window.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
}