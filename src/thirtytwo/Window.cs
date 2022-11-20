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

    private string? _text;

    private static readonly ConcurrentDictionary<HWND, WeakReference<Window>> s_windows = new();

    private static readonly HFONT s_defaultFont = CreateDefaultFont();

    public static Rectangle DefaultBounds { get; }
        = new(Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT);

    public HWND Handle { get; }

    public event WindowsMessageEvent? MessageHandler;

    public Window(
        WindowClass windowClass,
        Rectangle bounds,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default,
        HMENU menuHandle = default)
    {
        _windowClass = windowClass;
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
            uint dpi = this.GetDpiForWindow();
            Matrix3x2 transform = Matrix3x2.CreateScale((dpi / 96.0f) * 5.0f);
            deviceContext.SetWorldTransform(ref transform);
        }

        if (this.GetFont().IsNull)
        {
            // Default system font is applied, use a nicer (ClearType) font
            Handle.SetFont(s_defaultFont);
        }

        _windowProcedure = WindowProcedureInternal;
        _priorWindowProcedure = Handle.SetWindowProcedure(_windowProcedure);
    }

    private static HFONT CreateDefaultFont()
    {
        // Get the Screen DC
        using var hdc = HWND.Null.GetDeviceContext();
        return HFONT.CreateFont(
            typeface: "Microsoft Sans Serif",
            height: hdc.FontPointSizeToHeight(11),
            quality: FontQuality.ClearTypeNatural);
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
                    Message.DpiChanged dpiChanged = new(wParam, lParam);

                    using var oldFont = s_defaultFont;
                    var newFont = HFONT.CreateFont(
                        typeface: "Microsoft Sans Serif",
                        height: window.FontPointSizeToHeight(11),
                        quality: FontQuality.ClearTypeNatural);

                    window.SetFont(newFont);
                    window.EnumerateChildWindows(
                        (HWND child) =>
                        {
                            child.SetFont(newFont);
                            return true;
                        });

                    window.MoveWindow(dpiChanged.SuggestedBounds, repaint: true);

                    break;
                }
        }

        return Interop.CallWindowProc(_priorWindowProcedure, window, (uint)message, wParam, lParam);
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
