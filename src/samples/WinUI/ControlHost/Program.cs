// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

/// <summary>
///  Raw HWND oracle for WinUI 3 hosting. Product abstractions should be compared against this sample.
/// </summary>
internal static unsafe class Program
{
    private static DesktopWindowXamlSource? s_xamlSource;
    private static Grid? s_root;
    private static ColorPicker? s_colorPicker;

    [STAThread]
    private static void Main()
    {
        DispatcherQueueController dispatcher = DispatcherQueueController.CreateOnCurrentThread();
        try
        {
            using XamlApplication application = new();
            RunMessageLoop();
            GC.KeepAlive(application);
        }
        finally
        {
            dispatcher.ShutdownQueue();
        }
    }

    private static void RunMessageLoop()
    {
        const string ClassName = "ThirtyTwoWinUIControlHost";
        WindowProcedure windowProcedure = WindowProcedure;

        HMODULE module;
        if (!PInvoke.GetModuleHandleEx(0, (PCWSTR)null, &module))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        HWND window;
        fixed (char* className = ClassName)
        fixed (char* title = "ThirtyTwo WinUI 3 Control Host")
        {
            WNDCLASSEXW windowClass = new()
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                lpfnWndProc = (WNDPROC)Marshal.GetFunctionPointerForDelegate(windowProcedure),
                hInstance = module,
                hIcon = PInvoke.LoadIcon(default, PInvoke.IDI_APPLICATION),
                hCursor = PInvoke.LoadCursor(default, PInvoke.IDC_ARROW),
                hbrBackground = (HBRUSH)PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH),
                lpszClassName = className
            };

            ATOM atom = PInvoke.RegisterClassEx(&windowClass);
            if (!atom.IsValid)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            window = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,
                className,
                title,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                PInvoke.CW_USEDEFAULT,
                PInvoke.CW_USEDEFAULT,
                900,
                700,
                HWND.Null,
                HMENU.Null,
                module,
                null);

            if (window.IsNull)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
        }

        try
        {
            PInvoke.ShowWindow(window, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
            PInvoke.UpdateWindow(window);

            while (true)
            {
                BOOL result = PInvoke.GetMessage(out MSG message, HWND.Null, 0, 0);
                if ((int)result == -1)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }

                if (!result)
                {
                    break;
                }

                if (WindowsAppSdkInterop.ContentPreTranslateMessage(&message))
                {
                    continue;
                }

                PInvoke.TranslateMessage(message);
                PInvoke.DispatchMessage(message);
            }
        }
        finally
        {
            DisposeIsland();
        }

        GC.KeepAlive(windowProcedure);
    }

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case Interop.WM_CREATE:
                s_xamlSource = new DesktopWindowXamlSource();
                s_xamlSource.Initialize(Win32Interop.GetWindowIdFromWindow((nint)window.Value));
                s_xamlSource.ShouldConstrainPopupsToWorkArea = true;

                s_colorPicker = new ColorPicker();
                s_root = new Grid();
                s_root.Children.Add(s_colorPicker);
                s_xamlSource.Content = s_root;
                return (LRESULT)0;

            case Interop.WM_SIZE:
                s_xamlSource?.SiteBridge.MoveAndResize(new RectInt32(0, 0, lParam.LOWORD, lParam.HIWORD));
                return (LRESULT)0;

            case Interop.WM_SETFOCUS:
                // Returning focus to native siblings is intentionally deferred to the focus milestone.
                s_xamlSource?.NavigateFocus(new XamlSourceFocusNavigationRequest(XamlSourceFocusNavigationReason.First));
                return (LRESULT)0;

            case Interop.WM_PAINT:
                PAINTSTRUCT paint;
                PInvoke.BeginPaint(window, &paint);
                PInvoke.EndPaint(window, &paint);
                return (LRESULT)0;

            case Interop.WM_DESTROY:
                DisposeIsland();
                PInvoke.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return PInvoke.DefWindowProc(window, message, wParam, lParam);
    }

    private static void DisposeIsland()
    {
        if (s_xamlSource is null)
        {
            return;
        }

        s_xamlSource.Content = null;
        s_xamlSource.Dispose();
        s_xamlSource = null;
        s_root = null;
        s_colorPicker = null;
    }
}