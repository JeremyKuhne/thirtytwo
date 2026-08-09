// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal sealed unsafe class RawScrollingScene : IDisposable
{
    private readonly RawWindowClass _viewportClass;
    private readonly RawWindowClass _contentClass;
    private readonly RawWindowClass _hostClass;
    private readonly RawWindowClass _focusClass;
    private bool _disposed;

    private RawScrollingScene(
        RawWindowClass viewportClass,
        RawWindowClass contentClass,
        RawWindowClass hostClass,
        RawWindowClass focusClass,
        HWND viewport,
        HWND content,
        RawXamlHost host,
        HWND focusTarget)
    {
        _viewportClass = viewportClass;
        _contentClass = contentClass;
        _hostClass = hostClass;
        _focusClass = focusClass;
        Viewport = viewport;
        Content = content;
        Host = host;
        FocusTarget = focusTarget;
    }

    internal HWND Viewport { get; }

    internal HWND Content { get; }

    internal RawXamlHost Host { get; }

    internal HWND FocusTarget { get; }

    internal static RawScrollingScene Create(HWND parent)
    {
        HMODULE module;
        if (!PInvoke.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (PCWSTR)null,
            &module))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        RawWindowClass? viewportClass = null;
        RawWindowClass? contentClass = null;
        RawWindowClass? hostClass = null;
        RawWindowClass? focusClass = null;
        HWND viewport = default;
        HWND content = default;
        RawXamlHost? host = null;
        HWND focusTarget = default;
        try
        {
            viewportClass = new(module, "ThirtyTwoRawScrollingViewport", Color.FromArgb(255, 48, 48, 48));
            contentClass = new(module, "ThirtyTwoRawScrollingContent", Color.FromArgb(255, 24, 24, 24));
            hostClass = new(module, "ThirtyTwoRawScrollingHost", Color.Black);
            focusClass = new(module, "ThirtyTwoRawScrollingFocus", Color.SeaGreen);

            viewport = viewportClass.CreateChild(
                parent,
                RawScrollingScenario.InitialViewportBounds,
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE
                    | WINDOW_STYLE.WS_CLIPCHILDREN | WINDOW_STYLE.WS_CLIPSIBLINGS);
            content = contentClass.CreateChild(
                viewport,
                RawScrollingScenario.InitialContentBounds,
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE
                    | WINDOW_STYLE.WS_CLIPCHILDREN | WINDOW_STYLE.WS_CLIPSIBLINGS);
            host = RawXamlHost.Create(
                hostClass,
                content,
                RawScrollingScenario.HostBounds,
                "Translated XAML island",
                Microsoft.UI.Colors.RoyalBlue);
            focusTarget = focusClass.CreateChild(
                parent,
                RawScrollingScenario.FocusBounds,
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE
                    | WINDOW_STYLE.WS_TABSTOP | WINDOW_STYLE.WS_CLIPSIBLINGS);

            return new(
                viewportClass,
                contentClass,
                hostClass,
                focusClass,
                viewport,
                content,
                host,
                focusTarget);
        }
        catch
        {
            DestroyWindow(focusTarget);
            host?.Dispose();
            DestroyWindow(content);
            DestroyWindow(viewport);
            focusClass?.Dispose();
            hostClass?.Dispose();
            contentClass?.Dispose();
            viewportClass?.Dispose();
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
        DestroyWindow(FocusTarget);
        Host.Dispose();
        DestroyWindow(Content);
        DestroyWindow(Viewport);
        _focusClass.Dispose();
        _hostClass.Dispose();
        _contentClass.Dispose();
        _viewportClass.Dispose();
    }

    private static void DestroyWindow(HWND window)
    {
        if (!window.IsNull)
        {
            _ = PInvoke.DestroyWindow(window);
        }
    }
}