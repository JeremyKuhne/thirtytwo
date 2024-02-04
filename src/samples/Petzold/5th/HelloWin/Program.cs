// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Media.Audio;
using System.Runtime.InteropServices;
using Windows;

namespace HelloWin;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 3-1, Pages 44-46.
/// </summary>
internal unsafe static class Program
{
    // Windows metadata doesn't currently define this as it is a macro.
    const uint SND_ALIAS_SYSTEMHAND = 'S' | (((uint)'H') << 8);

    [STAThread]
    private static void Main()
    {
        Application.Run(new HelloWindow("HelloWin"));

        // Using Window and WindowClass are recommended. They do all of this setup for you.
        // To do the same thing manually, you would do the following:

        const string szAppName = "HelloWin";

        WindowProcedure wndProc = WindowProcedure;
        HMODULE module;
        Interop.GetModuleHandleEx(0, (PCWSTR)null, &module);

        HWND hwnd;

        fixed (char* appName = szAppName)
        fixed (char* title = "The Hello Program")
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

        GC.KeepAlive(wndProc);
    }

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case Interop.WM_CREATE:
                Interop.PlaySound(
                    (char*)SND_ALIAS_SYSTEMHAND,
                    HMODULE.Null,
                    SND_FLAGS.SND_ASYNC | SND_FLAGS.SND_NODEFAULT | SND_FLAGS.SND_ALIAS_ID);
                return (LRESULT)0;
            case Interop.WM_PAINT:
                PAINTSTRUCT ps;
                HDC hdc = Interop.BeginPaint(window, &ps);

                RECT rect;
                Interop.GetClientRect(window, &rect);

                // Technically this is unsafe as ellipsis options will modify the passed in string.
                fixed (char* text = "Hello, Windows 98!")
                {
                    _ = Interop.DrawTextEx(
                        hdc,
                        text,
                        -1,
                        &rect,
                        DRAW_TEXT_FORMAT.DT_SINGLELINE | DRAW_TEXT_FORMAT.DT_CENTER | DRAW_TEXT_FORMAT.DT_VCENTER,
                        null);
                }

                Interop.EndPaint(window, &ps);
                return (LRESULT)0;
            case Interop.WM_DESTROY:
                Interop.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return Interop.DefWindowProc(window, message, wParam, lParam);
    }

    private class HelloWindow : MainWindow
    {
        public HelloWindow(string text) : base(text, backgroundBrush: StockBrush.White)
        {
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Create:
                    Interop.PlaySound(
                        (char*)SND_ALIAS_SYSTEMHAND,
                        HMODULE.Null,
                        SND_FLAGS.SND_ASYNC | SND_FLAGS.SND_NODEFAULT | SND_FLAGS.SND_ALIAS_ID);
                    return (LRESULT)0;
                case MessageType.Paint:
                    using (DeviceContext dc = window.BeginPaint())
                    {
                        dc.DrawText(
                            "Hello, Windows 98!",
                            window.GetClientRectangle(),
                            DrawTextFormat.SingleLine | DrawTextFormat.Center | DrawTextFormat.VerticallyCenter);
                    }
                    return (LRESULT)0;
            }

            return base.WindowProcedure(window, message, wParam, lParam);
        }
    }
}
