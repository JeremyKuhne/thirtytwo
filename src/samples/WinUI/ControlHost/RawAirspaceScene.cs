// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal sealed unsafe class RawAirspaceScene : IDisposable
{
    private readonly RawWindowClass _viewportClass;
    private readonly RawWindowClass _hostClass;
    private readonly RawWindowClass _magentaClass;
    private readonly RawWindowClass _greenClass;
    private bool _disposed;

    private RawAirspaceScene(
        RawWindowClass viewportClass,
        RawWindowClass hostClass,
        RawWindowClass magentaClass,
        RawWindowClass greenClass,
        HWND viewport,
        RawXamlHost hostUnderNative,
        HWND nativeAbove,
        HWND nativeUnder,
        RawXamlHost hostAboveNative,
        RawXamlHost clippedHost)
    {
        _viewportClass = viewportClass;
        _hostClass = hostClass;
        _magentaClass = magentaClass;
        _greenClass = greenClass;
        Viewport = viewport;
        HostUnderNative = hostUnderNative;
        NativeAbove = nativeAbove;
        NativeUnder = nativeUnder;
        HostAboveNative = hostAboveNative;
        ClippedHost = clippedHost;
    }

    internal HWND Viewport { get; }

    internal RawXamlHost HostUnderNative { get; }

    internal HWND NativeAbove { get; }

    internal HWND NativeUnder { get; }

    internal RawXamlHost HostAboveNative { get; }

    internal RawXamlHost ClippedHost { get; }

    internal static RawAirspaceScene Create(HWND parent)
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
        RawWindowClass? hostClass = null;
        RawWindowClass? magentaClass = null;
        RawWindowClass? greenClass = null;
        HWND viewport = default;
        RawXamlHost? hostUnderNative = null;
        HWND nativeAbove = default;
        HWND nativeUnder = default;
        RawXamlHost? hostAboveNative = null;
        RawXamlHost? clippedHost = null;
        try
        {
            viewportClass = new(module, "ThirtyTwoRawAirspaceViewport", Color.FromArgb(255, 24, 24, 24));
            hostClass = new(module, "ThirtyTwoRawAirspaceHost", Color.Black);
            magentaClass = new(module, "ThirtyTwoRawAirspaceMagenta", Color.Magenta);
            greenClass = new(module, "ThirtyTwoRawAirspaceGreen", Color.SeaGreen);

            viewport = viewportClass.CreateChild(
                parent,
                new Rectangle(40, 40, 800, 460),
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_CLIPCHILDREN | WINDOW_STYLE.WS_CLIPSIBLINGS);
            hostUnderNative = RawXamlHost.Create(
                hostClass,
                viewport,
                new Rectangle(40, 40, 300, 200),
                "XAML below native",
                Microsoft.UI.Colors.RoyalBlue);
            nativeAbove = magentaClass.CreateChild(
                viewport,
                new Rectangle(160, 100, 220, 100),
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_TABSTOP | WINDOW_STYLE.WS_CLIPSIBLINGS);
            nativeUnder = greenClass.CreateChild(
                viewport,
                new Rectangle(420, 40, 300, 200),
                WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_TABSTOP | WINDOW_STYLE.WS_CLIPSIBLINGS);
            hostAboveNative = RawXamlHost.Create(
                hostClass,
                viewport,
                new Rectangle(540, 100, 220, 100),
                "XAML above native",
                Microsoft.UI.Colors.DarkOrange);
            clippedHost = RawXamlHost.Create(
                hostClass,
                viewport,
                new Rectangle(-100, 330, 300, 180),
                "Negative X, clipped by parent",
                Microsoft.UI.Colors.Purple);

            return new(
                viewportClass,
                hostClass,
                magentaClass,
                greenClass,
                viewport,
                hostUnderNative,
                nativeAbove,
                nativeUnder,
                hostAboveNative,
                clippedHost);
        }
        catch
        {
            clippedHost?.Dispose();
            hostAboveNative?.Dispose();
            DestroyWindow(nativeUnder);
            DestroyWindow(nativeAbove);
            hostUnderNative?.Dispose();
            DestroyWindow(viewport);
            greenClass?.Dispose();
            magentaClass?.Dispose();
            hostClass?.Dispose();
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
        ClippedHost.Dispose();
        HostAboveNative.Dispose();
        DestroyWindow(NativeUnder);
        DestroyWindow(NativeAbove);
        HostUnderNative.Dispose();
        DestroyWindow(Viewport);
        _greenClass.Dispose();
        _magentaClass.Dispose();
        _hostClass.Dispose();
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