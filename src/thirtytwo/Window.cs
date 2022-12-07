// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Windows.Support;

namespace Windows;

public unsafe class Window : ComponentBase, IHandle<HWND>, ILayoutHandler
{
    // Stash the delegate to keep it from being collected
    private readonly WindowProcedure _windowProcedure;
    private readonly WNDPROC _priorWindowProcedure;
    private readonly WindowClass _windowClass;
    private static readonly object s_lock = new();

    private static readonly WindowClass s_defaultWindowClass = new(className: $"DefaultWindowClass_{Guid.NewGuid()}");

    private string? _text;
    private uint _lastDpi;

    private static readonly ConcurrentDictionary<HWND, WeakReference<Window>> s_windows = new();
    private HFONT _lastCreatedFont;

    // Default fonts for each DPI
    private static readonly ConcurrentDictionary<int, HFONT> s_defaultFonts = new();

    public static Rectangle DefaultBounds { get; }
        = new(Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT);

    public HWND Handle { get; }

    public event WindowsMessageEvent? MessageHandler;

    public Window(
        Rectangle bounds,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default)
    {
        _windowClass = windowClass ?? s_defaultWindowClass;
        if (!_windowClass.IsRegistered)
        {
            _windowClass.Register();
        }

        _text = text;
        Handle = _windowClass.CreateWindow(
            bounds,
            text,
            style,
            extendedStyle,
            parentWindow?.Handle ?? default,
            parameters,
            menuHandle);

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

        // Need to set our Window Procedure to get messages before we set
        // the font (which sends a message to do so).
        _windowProcedure = WindowProcedureInternal;
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

    private LRESULT WindowProcedureInternal(HWND window, uint message, WPARAM wParam, LPARAM lParam)
        => WindowProcedure(window, (MessageType)message, wParam, lParam);

    protected virtual LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if (MessageHandler is { } handlers)
        {
            foreach (var handler in handlers.GetInvocationList().OfType<WindowsMessageEvent>())
            {
                var result = handler(this, window, message, wParam, lParam);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }
        }

        switch (message)
        {
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

        return Interop.CallWindowProc(_priorWindowProcedure, window, (uint)message, wParam, lParam);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            bool success = s_windows.TryRemove(Handle, out _);
            Debug.Assert(success);
        }

        this.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)(void*)_priorWindowProcedure.Value);
    }

    void ILayoutHandler.Layout(Rectangle bounds) => Handle.MoveWindow(bounds, repaint: true);

    public static implicit operator HWND(Window window) => window.Handle;
}