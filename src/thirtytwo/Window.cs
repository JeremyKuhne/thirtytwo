// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Windows.Components;
using Windows.Support;
using Windows.Win32.Graphics.Direct2D;

namespace Windows;

public unsafe partial class Window : ComponentBase, IHandle<HWND>, ILayoutHandler
{
    private static readonly object s_lock = new();
    private static readonly ConcurrentDictionary<HWND, WeakReference<Window>> s_windows = new();
    private static readonly WindowClass s_defaultWindowClass = new(className: $"DefaultWindowClass_{Guid.NewGuid()}");

    public static Rectangle DefaultBounds { get; }
        = new(Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT);

    // Default fonts for each DPI
    private static readonly ConcurrentDictionary<int, HFONT> s_defaultFonts = new();
    internal static WNDPROC DefaultWindowProcedure { get; } = GetDefaultWindowProcedure();

    // High precision metric units are .01mm each
    private const int HiMetricUnitsPerInch = 2540;

    private readonly object _lock = new();

    // Stash the delegate to keep it from being collected
    private readonly WindowProcedure _windowProcedure;
    private readonly WNDPROC _priorWindowProcedure;
    protected readonly WindowClass _windowClass;
    private bool _destroyed;
    private HWND _handle;

    // When I send a WM_GETFONT message to a window, why don't I get a font?
    // https://devblogs.microsoft.com/oldnewthing/20140724-00/?p=413

    // Who is responsible for destroying the font passed in the WM_SETFONT message?
    // https://devblogs.microsoft.com/oldnewthing/20080912-00/?p=20893

    private HFONT _font;
    private HFONT _lastCreatedFont;

    private HwndRenderTarget? _renderTarget;

    protected HwndRenderTarget RenderTarget => _renderTarget ?? throw new InvalidOperationException();

    private uint _lastDpi;
    private Color _backgroundColor;
    private HBRUSH _backgroundBrush;

    private readonly Features _features;

    [MemberNotNullWhen(true, nameof(_renderTarget))]
    protected bool IsDirect2dEnabled()
    {
        bool enabled = _features.AreFlagsSet(Features.EnableDirect2d);
        if (enabled && _renderTarget is null)
        {
            UpdateRenderTarget(Handle, this.GetClientRectangle().Size);
        }

        return enabled;
    }

    /// <summary>
    ///  The window handle. This will be <see cref="HWND.Null"/> after the window is destroyed.
    /// </summary>
    public HWND Handle => _handle;

    public event WindowsMessageEvent? MessageHandler;

    public Window(
        Rectangle bounds = default,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        Color backgroundColor = default,
        Features features = default)
    {
        _windowClass = windowClass ?? s_defaultWindowClass;

        if (bounds.IsEmpty)
        {
            bounds = DefaultBounds;
        }

        _features = features;
        _backgroundColor = backgroundColor;

        try
        {
            _handle = _windowClass.CreateWindow(
                bounds,
                text,
                style,
                extendedStyle,
                parentWindow?.Handle ?? default,
                parameters,
                menuHandle,
                InitializationWindowProcedure);
        }
        catch
        {
            // Make sure we don't leave a window handle around if we fail to create the window.
            _handle = default;
            throw;
        }

        // Need to set our Window Procedure to get messages before we set
        // the font (which sends a message to do so).
        _windowProcedure = WindowProcedureInternal;

        s_windows[Handle] = new(this);
        _priorWindowProcedure = this.SetWindowProcedure(_windowProcedure);

        _lastDpi = this.GetDpi();
        if (this.GetFontHandle().IsNull)
        {
            // Default system font is applied, use a nicer (ClearType) font
            this.SetFontHandle(GetDefaultFontForDpi((int)_lastDpi));
        }
    }

    private static HFONT GetDefaultFontForDpi(int dpi)
    {
        if (!s_defaultFonts.TryGetValue(dpi, out HFONT font))
        {
            lock (s_lock)
            {
                if (!s_defaultFonts.TryGetValue(dpi, out font))
                {
                    font = HFONT.CreateFont(
                        typeface: "Microsoft Sans Serif",
                        height: HFONT.GetHeightForDpi(pointSize: 12, dpi),
                        quality: FontQuality.ClearTypeNatural);

                    s_defaultFonts[dpi] = font;
                }
            }
        }

        return font;
    }

    public void SetFont(string typeFace, int pointSize)
    {
        HFONT newFont = HFONT.CreateFont(
            typeface: typeFace,
            height: HFONT.GetHeightForDpi(pointSize, (int)this.GetDpi()),
            quality: FontQuality.ClearTypeNatural);

        if (!_lastCreatedFont.IsNull)
        {
            _lastCreatedFont.Dispose();
        }

        _lastCreatedFont = newFont;

        this.SetFontHandle(_lastCreatedFont);
    }

    private void UpdateRenderTarget(HWND window, Size size)
    {
        if (_renderTarget is null)
        {
            _renderTarget = HwndRenderTarget.CreateForWindow(Application.Direct2dFactory, window, size);
            RenderTargetCreated();
        }
        else
        {
            _renderTarget.Resize(size);
        }
    }

    /// <summary>
    ///  Called whenever the Direct2D render target has been created or recreated.
    /// </summary>
    protected virtual void RenderTargetCreated()
    {
    }

    protected virtual void OnPaint()
    {
    }

    protected virtual void OnSize(Size size)
    {
    }

    /// <summary>
    ///  Called whenever a command is sent to the window.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Control classes send the command to the parent Window first. We also reflect the message back to
    ///   the control Window so that it can handle the message. This is similar to MFC/WinForms behavior.
    ///  </para>
    /// </remarks>
    protected virtual void OnCommand(int controlId, int notificationCode)
    {
    }

    private LRESULT InitializationWindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        if (Handle.IsNull)
        {
            // In the middle of CreateWindow, set our handle so that the "this" pointer is valid for use.
            // This enables things such as parenting children during WM_CREATE.

            _handle = window;
        }

        return WindowProcedureInternal(window, message, wParam, lParam);
    }

    private LRESULT WindowProcedureInternal(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        // What is the difference between WM_DESTROY and WM_NCDESTROY?
        // https://devblogs.microsoft.com/oldnewthing/20050726-00/?p=34803

        // Check for messages that we need to process before invoking handlers. Currently this means making
        // sure that Direct2D is in the right state if it has been opted into.
        switch (message)
        {
            case Interop.WM_SIZE:
                Size size = new(lParam.LOWORD, lParam.HIWORD);

                // Check the flag directly here so we don't create then resize.
                if (_features.AreFlagsSet(Features.EnableDirect2d))
                {
                    UpdateRenderTarget(window, size);
                }

                break;

            case Interop.WM_PAINT:
                if (IsDirect2dEnabled())
                {
                    _renderTarget.BeginDraw();
                    _renderTarget.SetTransform(Matrix3x2.Identity);
                    _renderTarget.Clear(_backgroundColor);
                }

                break;
        }

        // Let attached handlers have a chance to deal with the message.
        bool handled = InvokeHandlers(out LRESULT result);

        // Handle messages that we need to update state or invoke virtuals on.
        switch (message)
        {
            case Interop.WM_NCDESTROY:
                lock (_lock)
                {
                    // This should be the final message. Track that we've been destroyed so we know we don't have
                    // to manually clean up.

                    bool success = s_windows.TryRemove(Handle, out _);
                    Debug.Assert(success);
                    _handle = default;
                    _destroyed = true;
                }

                break;

            case Interop.WM_SIZE:
                Size size = new(lParam.LOWORD, lParam.HIWORD);
                OnSize(size);
                break;

            case Interop.WM_PAINT:
                OnPaint();
                break;
        }

        if (!handled)
        {
            // Not marked as handled, call the virtual method to allow for "normal" processing.
            result = WindowProcedure(window, (MessageType)message, wParam, lParam);
        }

        if (message == Interop.WM_PAINT && IsDirect2dEnabled())
        {
            _renderTarget.EndDraw(out bool recreateTarget);
            if (recreateTarget)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
                UpdateRenderTarget(window, this.GetClientRectangle().Size);
            }
        }

        // Ensure we're not collected while we're processing a message.
        GC.KeepAlive(this);
        return result;

        bool InvokeHandlers(out LRESULT result)
        {
            if (MessageHandler is { } handlers)
            {
                foreach (var handler in handlers.GetInvocationList().OfType<WindowsMessageEvent>())
                {
                    LRESULT? handlerResult = handler(this, window, (MessageType)message, wParam, lParam);
                    if (handlerResult.HasValue)
                    {
                        result = handlerResult.Value;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    ///  Override to handle window messages. Call base to allow default handling.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Note that some messages will be sent before the class constructor has fully run. These messages are
    ///   <see cref="MessageType.GetMinMaxInfo"/>, <see cref="MessageType.NonClientCreate"/>,
    ///   <see cref="MessageType.NonClientCalculateSize"/> and <see cref="MessageType.Create"/>. Do not access
    ///  </para>
    /// </remarks>
    protected virtual LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            // https://learn.microsoft.com/windows/win32/gdi/window-background
            // https://learn.microsoft.com/windows/win32/gdi/drawing-a-custom-window-background
            case MessageType.EraseBackground:

                if (IsDirect2dEnabled())
                {
                    // Having the HDC erased will cause flicker, so say we handled it. We could erase using
                    // Direct2D, but pushing that to the paint method is avoids an extra BeginDraw/EndDraw.
                    return (LRESULT)1;
                }
                else if (!_backgroundColor.IsEmpty)
                {
                    if (_backgroundBrush.IsNull)
                    {
                        _backgroundBrush = HBRUSH.CreateSolid(_backgroundColor);
                    }

                    ((HDC)wParam).FillRectangle(this.GetClientRectangle(), _backgroundBrush);
                    return (LRESULT)1;
                }

                break;

            case MessageType.GetFont:
                // We only want to handle fonts if we're not an externally registered class.
                if (!_windowClass.ModuleInstance.IsNull)
                {
                    return (LRESULT)_font.Value;
                }

                break;

            case MessageType.SetFont:
                if (!_windowClass.ModuleInstance.IsNull)
                {
                    _font = (HFONT)(nint)wParam.Value;
                    if ((BOOL)lParam.LOWORD)
                    {
                        this.Invalidate();
                    }

                    return (LRESULT)0;
                }

                break;

            case MessageType.DpiChanged:
                // Resize and reposition for the new DPI
                HandleDpiChanged(new(wParam, lParam));
                break;

            case MessageType.Command:
                if (lParam != 0 && FromHandle((HWND)lParam, walkParents: false) is Window child)
                {
                    // Control command from a child control, reflect the message to the control Window.
                    // (Matching MFC/WinForms behavior here.)
                    LRESULT result = child.SendMessage(MessageType.ReflectCommand, wParam, lParam);
                    OnCommand(wParam.LOWORD, wParam.HIWORD);
                    return result;
                }

                break;

            case MessageType.ReflectCommand:
                OnCommand(wParam.LOWORD, wParam.HIWORD);

                // 0 means we handled the Command, no reason to call base as this message was reflected to us.
                return (LRESULT)0;
        }

        return _priorWindowProcedure.IsNull
            // Still creating the window.
            ? (LRESULT)(-1)
            : Interop.CallWindowProc(_priorWindowProcedure, window, (uint)message, wParam, lParam);
    }

    private void HandleDpiChanged(Message.DpiChanged dpiChanged)
    {
        uint lastDpi = _lastDpi;
        _lastDpi = dpiChanged.Dpi;
        UpdateFontsForDpi(lastDpi, _lastDpi);
        this.MoveWindow(dpiChanged.SuggestedBounds, repaint: true);
    }

    private void UpdateFontsForDpi(uint lastDpi, uint newDpi)
    {
        HFONT currentFont = this.GetFontHandle();
        HFONT lastCreatedFont = _lastCreatedFont;

        // Check to see if we're using one of our managed fonts.

        if (!lastCreatedFont.IsNull && lastCreatedFont == currentFont)
        {
            // One that we created that isn't a static default
            var logfont = currentFont.GetLogicalFont();
            float scale = (float)newDpi / lastDpi;
            logfont.lfHeight = (int)(logfont.lfHeight * scale);
            HFONT newFont = Interop.CreateFontIndirect(&logfont);
            this.SetFontHandle(newFont);
            _lastCreatedFont = newFont;
            lastCreatedFont.Dispose();
        }
        else if (GetDefaultFontForDpi((int)lastDpi) == currentFont)
        {
            // Was our default font, use the new scale
            this.SetFontHandle(GetDefaultFontForDpi((int)newDpi));
        }

        this.EnumerateChildWindows((HWND child) =>
        {
            FromHandle(child)?.UpdateFontsForDpi(lastDpi, newDpi);
            return true;
        });
    }

    /// <summary>
    ///  Try to get the <see cref="Window"/> from the given <paramref name="handle"/>. Walks parent windows
    ///  if there is no matching <see cref="Window"/> and <paramref name="walkParents"/> is <see langword="true"/>.
    /// </summary>
    public static Window? FromHandle<T>(T handle, bool walkParents = false)
        where T : IHandle<HWND>
    {
        if (handle is null || handle.Handle.IsNull)
        {
            return null;
        }

        if (handle is Window window)
        {
            return window;
        }

        HWND hwnd = handle.Handle;
        if (s_windows.TryGetValue(hwnd, out var weakReference))
        {
            if (weakReference.TryGetTarget(out Window? found))
            {
                return found;
            }
            else
            {
                Debug.Fail("Dead weak ref. Window.Dispose not called.");
            }
        }

        if (!walkParents)
        {
            return null;
        }

        hwnd = Interop.GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_PARENT);
        return hwnd.IsNull ? null : FromHandle(hwnd, walkParents: true);
    }

    /// <remarks>
    ///  <para>
    ///   Note that the <see cref="Handle"/> may be <see cref="HWND.Null"/> when this method is called. When the
    ///   underlying <see cref="HWND"/> is destroyed, the handle is no longer valid and will be set to null.
    ///  </para>
    /// </remarks>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        // We want to block at a WM_NCDESTROY message so that we know our handle is still valid for cleanup.
        lock (_lock)
        {
            if (!_destroyed)
            {
                // Set back the default Window procedure as we don't want any messages coming in anymore.
                Handle.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)(void*)DefaultWindowProcedure);

                // Send a close message to the window. This will cause the window to be destroyed. If we're being
                // finalized, post instead of send to ensure the message is processed on the right thread.
                if (!disposing)
                {
                    Handle.PostMessage(MessageType.Close);
                }
                else
                {
                    Handle.SendMessage(MessageType.Close);
                    bool success = s_windows.TryRemove(Handle, out _);
                    Debug.Assert(success);
                }
            }
        }

        if (disposing)
        {
            _backgroundBrush.Dispose();
            _lastCreatedFont.Dispose();
            _font.Dispose();
            _renderTarget?.Dispose();
        }
    }

    void ILayoutHandler.Layout(Rectangle bounds) => LayoutWindow(bounds);

    protected virtual void LayoutWindow(Rectangle bounds)
    {
        if (bounds != this.GetClientRectangle())
        {
            Handle.MoveWindow(bounds, repaint: true);
        }
    }

    public static implicit operator HWND(Window window) => window.Handle;

    /// <summary>
    ///  Allows preprocessing messages before they are translated and dispatched.
    /// </summary>
    /// <returns><see langword="true"/> if handled and translation and dispatching should be skipped.</returns>
    protected internal virtual bool PreProcessMessage(ref MSG message) => false;

    public int PixelToHiMetric(int pixels)
        => (int)(((HiMetricUnitsPerInch * pixels) + (_lastDpi >> 1)) / _lastDpi);

    public Size PixelToHiMetric(Size size)
        => new(PixelToHiMetric(size.Width), PixelToHiMetric(size.Height));

    public int HiMetricToPixel(int units)
        => (int)(((_lastDpi * units) + (HiMetricUnitsPerInch / 2)) / HiMetricUnitsPerInch);

    private static WNDPROC GetDefaultWindowProcedure()
    {
        HMODULE module = Interop.LoadLibrary("user32.dll");
        Debug.Assert(!module.IsNull);
        FARPROC address = Interop.GetProcAddress(module, "DefWindowProcW");
        Debug.Assert(!address.IsNull);
        return (WNDPROC)(void*)address.Value;
    }
}