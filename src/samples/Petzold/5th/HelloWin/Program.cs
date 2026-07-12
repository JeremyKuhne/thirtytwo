// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Media.Audio;
using System.Runtime.InteropServices;
using Windows;
using System.Drawing;

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
        PInvoke.GetModuleHandleEx(0, (PCWSTR)null, &module);

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
                hIcon = PInvoke.LoadIcon(default, PInvoke.IDI_APPLICATION),
                hCursor = PInvoke.LoadCursor(default, PInvoke.IDC_ARROW),
                hbrBackground = (HBRUSH)PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH),
                lpszClassName = appName
            };

            ATOM atom = PInvoke.RegisterClassEx(&wndClass);

            hwnd = PInvoke.CreateWindowEx(
                WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,
                appName,
                title,
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT,
                HWND.Null,
                HMENU.Null,
                module,
                null);


        }

        PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
        PInvoke.UpdateWindow(hwnd);

        while (PInvoke.GetMessage(out MSG msg, HWND.Null, 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }

        GC.KeepAlive(wndProc);
    }

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case Interop.WM_CREATE:
                PInvoke.PlaySound(
                    (char*)SND_ALIAS_SYSTEMHAND,
                    HMODULE.Null,
                    SND_FLAGS.SND_ASYNC | SND_FLAGS.SND_NODEFAULT | SND_FLAGS.SND_ALIAS_ID);
                return (LRESULT)0;
            case Interop.WM_PAINT:
                PAINTSTRUCT ps;
                HDC hdc = PInvoke.BeginPaint(window, &ps);

                RECT rect;
                PInvoke.GetClientRect(window, &rect);

                // Technically this is unsafe as ellipsis options will modify the passed in string.
                fixed (char* text = "Hello, Windows 98!")
                {
                    _ = PInvoke.DrawTextEx(
                        hdc,
                        text,
                        -1,
                        &rect,
                        DRAW_TEXT_FORMAT.DT_SINGLELINE | DRAW_TEXT_FORMAT.DT_CENTER | DRAW_TEXT_FORMAT.DT_VCENTER,
                        null);
                }

                PInvoke.EndPaint(window, &ps);
                return (LRESULT)0;
            case Interop.WM_DESTROY:
                PInvoke.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return PInvoke.DefWindowProc(window, message, wParam, lParam);
    }

    private class HelloWindow : MainWindow
    {
        public HelloWindow(string title) : base(title: title, backgroundColor: Color.White)
        {
        }

        protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            switch (message)
            {
                case MessageType.Create:
                    PInvoke.PlaySound(
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
