﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Windows.Components;
using Windows.Support;

namespace Windows;

public unsafe class Window : ComponentBase, IHandle<HWND>, ILayoutHandler
{
    // High precision metric units are .01mm each
    private const int HiMetricUnitsPerInch = 2540;

    // Stash the delegate to keep it from being collected
    private readonly WindowProcedure _windowProcedure;
    private readonly WNDPROC _priorWindowProcedure;
    private readonly WindowClass _windowClass;
    private static readonly object s_lock = new();
    private readonly object _lock = new();
    private bool _destroyed;
    private HWND _handle;

    // When I send a WM_GETFONT message to a window, why don't I get a font?
    // https://devblogs.microsoft.com/oldnewthing/20140724-00/?p=413

    // Who is responsible for destroying the font passed in the WM_SETFONT message?
    // https://devblogs.microsoft.com/oldnewthing/20080912-00/?p=20893

    private HFONT _font;

    private readonly HBRUSH _backgroundBrush;

    private static readonly WindowClass s_defaultWindowClass = new(className: $"DefaultWindowClass_{Guid.NewGuid()}");
    internal static WNDPROC DefaultWindowProcedure { get; } = GetDefaultWindowProcedure();

    private string? _text;
    private uint _lastDpi;

    private static readonly ConcurrentDictionary<HWND, WeakReference<Window>> s_windows = new();
    private HFONT _lastCreatedFont;

    // Default fonts for each DPI
    private static readonly ConcurrentDictionary<int, HFONT> s_defaultFonts = new();

    public static Rectangle DefaultBounds { get; }
        = new(Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT);

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
        HBRUSH backgroundBrush = default)
    {
        _windowClass = windowClass ?? s_defaultWindowClass;

        if (bounds.IsEmpty)
        {
            bounds = DefaultBounds;
        }

        _text = text;

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

        _backgroundBrush = backgroundBrush;

        s_windows[Handle] = new(this);

        if (parentWindow is null)
        {
            // Set up HDC for scaling
            using var deviceContext = this.GetDeviceContext();
            deviceContext.SetGraphicsMode(GRAPHICS_MODE.GM_ADVANCED);
            uint dpi = this.GetDpi();
            Matrix3x2 transform = Matrix3x2.CreateScale((dpi / 96.0f) * 5.0f);
            deviceContext.SetWorldTransform(ref transform);
        }

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
        if (MessageHandler is { } handlers)
        {
            foreach (var handler in handlers.GetInvocationList().OfType<WindowsMessageEvent>())
            {
                LRESULT? result = handler(this, window, (MessageType)message, wParam, lParam);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }
        }

        // What is the difference between WM_DESTROY and WM_NCDESTROY?
        // https://devblogs.microsoft.com/oldnewthing/20050726-00/?p=34803

        if ((MessageType)message == MessageType.NonClientDestroy)
        {
            lock (_lock)
            {
                // This should be the final message. Track that we've been destroyed so we know we don't have
                // to manually clean up.

                bool success = s_windows.TryRemove(Handle, out _);
                Debug.Assert(success);
                _handle = default;
                _destroyed = true;
            }
        }

        LRESULT windProcResult = WindowProcedure(window, (MessageType)message, wParam, lParam);

        // Ensure we're not collected while we're processing a message.
        GC.KeepAlive(this);
        return windProcResult;
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
            case MessageType.EraseBackground:
                if (!_backgroundBrush.IsNull)
                {
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

            case MessageType.SetText:
                // Update our cached text if necessary

                if (lParam == 0)
                {
                    _text = null;
                }
                else
                {
                    Message.SetText setText = new(lParam);
                    if (!setText.Text.Equals(_text, StringComparison.Ordinal))
                    {
                        _text = setText.Text.ToString();
                    }
                }

                // The default proc actually sets the text, so we shouldn't return from here
                break;

            case MessageType.DpiChanged:
                {
                    // Resize and reposition for the new DPI
                    HandleDpiChanged(new(wParam, lParam));
                    break;
                }
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

    public string Text
    {
        get => _text ?? string.Empty;
        set
        {
            this.SetWindowText(value);
            _text = value;
        }
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
                // We don't want any messages coming in anymore (as the Window will be collected eventually and
                // our callback will no longer exist). Set the default window procedure to the window and post
                // a close message so it will destroy the window on the correct thread.
                Handle.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)(void*)DefaultWindowProcedure);
                Handle.PostMessage(MessageType.Close);

                if (disposing)
                {
                    bool success = s_windows.TryRemove(Handle, out _);
                    Debug.Assert(success);
                }
            }
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