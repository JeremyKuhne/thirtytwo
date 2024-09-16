// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI;

using Xaml = Microsoft.UI.Xaml;
using Windows.Graphics;

namespace ControlHost;

/// <summary>
///  Bare bones hosting of a WinUI framework control.
/// </summary>
internal unsafe static class Program
{
    private static DispatcherQueueController? s_dispatcher;
    private static DesktopWindowXamlSource? s_xamlSource;
    private static Xaml.Controls.Control? s_control;

    [STAThread]
    private static void Main()
    {
        // Note that the .NET Runtime will initialize COM with CoInitialze and RoInitialize for STA.

        s_dispatcher = DispatcherQueueController.CreateOnCurrentThread();

        WindowProcedure wndProc = WindowProcedure;
        HMODULE module;
        Interop.GetModuleHandleEx(0, (PCWSTR)null, &module);

        HWND hwnd;

        fixed (char* appName = "WinUIControl")
        fixed (char* title = "Hosted WinUI Control")
        {
            WNDCLASSEXW wndClass = new()
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                lpfnWndProc = (WNDPROC)Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = module,
                hIcon = Interop.LoadIcon(default, Interop.IDI_APPLICATION),
                hCursor = Interop.LoadCursor(default, Interop.IDC_ARROW),
                hbrBackground = (HBRUSH)Interop.GetStockObject(GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH),
                lpszClassName = appName
            };

            ATOM atom = Interop.RegisterClassEx(&wndClass);

            hwnd = Interop.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,
                appName,
                title,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT,
                HWND.Null,
                HMENU.Null,
                module,
                null);
        }

        Interop.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
        Interop.UpdateWindow(hwnd);

        while (Interop.GetMessage(out MSG msg, HWND.Null, 0, 0))
        {
            Interop.TranslateMessage(msg);
            Interop.DispatchMessage(msg);
        }

        s_dispatcher.ShutdownQueue();
        GC.KeepAlive(wndProc);
    }

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case Interop.WM_CREATE:
                s_xamlSource = new DesktopWindowXamlSource();
                s_xamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(window));

                HWND xamlSourceWindow = (HWND)Win32Interop.GetWindowFromWindowId(s_xamlSource.SiteBridge.WindowId);

                // Style is already WS_CHILD | WS_VISIBLE

                // WINDOW_STYLE existing = (WINDOW_STYLE)Interop.GetWindowLongPtr(xamlSourceWindow, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

                // Interop.SetWindowLongPtr(
                //     xamlSourceWindow,
                //     WINDOW_LONG_PTR_INDEX.GWL_STYLE,
                //     (nint)(WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_VISIBLE));

                s_control = new Xaml.Controls.ColorPicker();
                s_xamlSource.Content = s_control;
                return (LRESULT)0;
            case Interop.WM_SIZE:
                if (s_xamlSource != null)
                {
                    RectInt32 size = new(0, 0, lParam.LOWORD, lParam.HIWORD);
                    s_xamlSource.SiteBridge.MoveAndResize(size);
                }
                return (LRESULT)0;
            case Interop.WM_PAINT:
                PAINTSTRUCT paintStruct;
                Interop.BeginPaint(window, &paintStruct);
                Interop.EndPaint(window, &paintStruct);
                return (LRESULT)0;
            case Interop.WM_DESTROY:
                Interop.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return Interop.DefWindowProc(window, message, wParam, lParam);
    }
}
