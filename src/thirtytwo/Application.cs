// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

public unsafe static class Application
{
    public static MessageBoxResult MessageBox(
        string text,
        string caption,
        MessageBoxStyle style = MessageBoxStyle.Ok)
    {
        return HWND.Null.MessageBox(text, caption, style);
    }

    public static MessageBoxResult MessageBox<T>(
        T owner,
        string text,
        string caption,
        MessageBoxStyle style = MessageBoxStyle.Ok)
        where T : IHandle<HWND>
    {
        return owner.Handle.MessageBox(text, caption, style);
    }

    public static void Run(
        WindowClass windowClass,
        string? windowTitle = null,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        HMENU menuHandle = default)
        => Run(
            windowClass,
            Window.DefaultBounds,
            windowTitle,
            style,
            extendedStyle,
            menuHandle);

    public static void Run(
        WindowClass windowClass,
        Rectangle bounds,
        string? windowTitle = null,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        HMENU menuHandle = default)
    {
        if (!windowClass.IsRegistered)
        {
            windowClass.Register();
        }

        Window mainWindow = new(
            windowClass,
            bounds,
            windowTitle,
            style,
            extendedStyle,
            menuHandle: menuHandle);

        Run(mainWindow);
    }

    public static void Run(Window window)
    {
        try
        {
            window.MessageHandler += Window_QuitHandler;

            window.ShowWindow(ShowWindowCommand.Normal);
            window.UpdateWindow();

            while (Interop.GetMessage(out MSG message, HWND.Null, 0, 0))
            {
                Interop.TranslateMessage(&message);
                Interop.DispatchMessage(&message);
            }

            // Make sure our window doesn't get collected while we're pumping messages
            GC.KeepAlive(window);
        }
        catch
        {
            Interop.DestroyWindow(window);
            throw;
        }
    }

    private static LRESULT? Window_QuitHandler(object _, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if (message == MessageType.Destroy)
        {
            Interop.PostQuitMessage(0);
            return (LRESULT)0;
        }

        return null;
    }
}
