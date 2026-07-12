// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace RandRect;

/// <summary>
///  Sample from Programming Windows, 5th Edition.
///  Original (c) Charles Petzold, 1998
///  Figure 5-26, Pages 200-202.
/// </summary>
internal unsafe static class Program
{
    [STAThread]
    private static void Main()
    {
        const string szAppName = "RandRect";

        WindowProcedure wndProc = WindowProcedure;
        HMODULE module;
        PInvoke.GetModuleHandleEx(0, (PCWSTR)null, &module);

        HWND hwnd;

        fixed (char* appName = szAppName)
        fixed (char* title = "Random Rectangles")
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

        while (true)
        {
            if (PInvoke.PeekMessage(out MSG message, HWND.Null, 0, uint.MaxValue, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
            {
                if (message.message == PInvoke.WM_QUIT)
                {
                    break;
                }

                PInvoke.TranslateMessage(message);
                PInvoke.DispatchMessage(message);
            }

            // We're crazy fast over 25 years past the source sample,
            // sleeping to make this a bit more interesting.
            Thread.Sleep(100);
            DrawRectangle(hwnd);
        }
    }

    private static int s_cxClient, s_cyClient;
    private static readonly Random s_rand = new();

    private static LRESULT WindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch ((MessageType)message)
        {
            case MessageType.Size:
                s_cxClient = lParam.LOWORD;
                s_cyClient = lParam.HIWORD;
                return (LRESULT)0;
            case MessageType.Destroy:
                PInvoke.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return PInvoke.DefWindowProc(window, message, wParam, lParam);
    }

    private static void DrawRectangle(HWND window)
    {
        if (s_cxClient == 0 || s_cyClient == 0)
            return;

        Rectangle rect = Rectangle.FromLTRB(
            s_rand.Next() % s_cxClient,
            s_rand.Next() % s_cyClient,
            s_rand.Next() % s_cxClient,
            s_rand.Next() % s_cyClient);

        using HBRUSH brush = HBRUSH.CreateSolid(
            Color.FromArgb((byte)(s_rand.Next() % 256), (byte)(s_rand.Next() % 256), (byte)(s_rand.Next() % 256)));
        using DeviceContext dc = window.GetDeviceContext();
        dc.FillRectangle(rect, brush);
    }
}
