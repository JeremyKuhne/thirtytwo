// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using WinUIColor = Windows.UI.Color;
using XamlHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using XamlVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace ControlHost;

internal sealed unsafe class RawXamlHost : IDisposable
{
    private readonly DesktopWindowXamlSource _source;
    private bool _disposed;

    private RawXamlHost(HWND handle, DesktopWindowXamlSource source, FrameworkElement content)
    {
        Handle = handle;
        _source = source;
        Content = content;
    }

    internal HWND Handle { get; }

    internal FrameworkElement Content { get; }

    internal DesktopWindowXamlSource Source => _source;

    internal HWND SiteBridge => (HWND)Win32Interop.GetWindowFromWindowId(_source.SiteBridge.WindowId);

    internal static RawXamlHost Create(
        RawWindowClass windowClass,
        HWND parent,
        Rectangle bounds,
        string text,
        WinUIColor color)
    {
        HWND handle = windowClass.CreateChild(
            parent,
            bounds,
            WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_TABSTOP
                | WINDOW_STYLE.WS_CLIPCHILDREN | WINDOW_STYLE.WS_CLIPSIBLINGS);
        DesktopWindowXamlSource? source = null;
        try
        {
            source = new();
            source.Initialize(Win32Interop.GetWindowIdFromWindow((nint)handle.Value));
            source.ShouldConstrainPopupsToWorkArea = true;
            FrameworkElement content = CreateIsland(text, color);
            source.Content = content;
            source.SiteBridge.MoveAndResize(new RectInt32(0, 0, bounds.Width, bounds.Height));
            return new(handle, source, content);
        }
        catch
        {
            if (source is not null)
            {
                source.Content = null;
                source.Dispose();
            }

            _ = PInvoke.DestroyWindow(handle);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _source.Content = null;
        _source.Dispose();
        _ = PInvoke.DestroyWindow(Handle);
    }

    private static Border CreateIsland(string text, WinUIColor color)
        => new()
        {
            Background = new SolidColorBrush(color),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = XamlHorizontalAlignment.Center,
                VerticalAlignment = XamlVerticalAlignment.Center
            }
        };
}