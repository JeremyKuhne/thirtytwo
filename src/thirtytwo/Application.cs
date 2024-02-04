// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Support;

namespace Windows;

public static unsafe class Application
{
    private static ActivationContext? s_visualStylesContext;
    internal static ActivationScope ThemingScope => new(GetStylesContext());

    internal static void EnsureDpiAwareness()
    {
        // Enable High DPI awareness if not enabled already. Requires Windows 10.
        if (Interop.GetAwarenessFromDpiAwarenessContext(Interop.GetThreadDpiAwarenessContext()) == DPI_AWARENESS.DPI_AWARENESS_UNAWARE
            && Interop.SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2).IsNull)
        {
            // Fall back from V2 if needed
            Interop.SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
        }
    }

    public static DialogResult ShowTaskDialog(
        string? mainInstruction = null,
        string? content = null,
        string? title = null,
        TaskDialogButtons buttons = TaskDialogButtons.Ok,
        TaskDialogIcon? icon = null)
    {
        HWND active = Interop.GetActiveWindow();
        return new HandleRef<HWND>(Window.FromHandle(active), active).TaskDialog(mainInstruction, content, title, buttons, icon);
    }

    public static DialogResult ShowTaskDialog<T>(
        T owner,
        string? mainInstruction = null,
        string? content = null,
        string? title = null,
        TaskDialogButtons buttons = TaskDialogButtons.Ok,
        TaskDialogIcon? icon = null)
        where T : IHandle<HWND>
    {
        return owner.TaskDialog(mainInstruction, content, title, buttons, icon);
    }

    // TaskDialog is not a 1-1 replacement for MessageBox. There is very little reason to use MessageBox, but leaving
    // this here to help discovery of ShowTaskDialog.

    [Obsolete($"{nameof(ShowMessageBox)} does not support high DPI, use {nameof(ShowTaskDialog)} instead.", error: false)]
    public static DialogResult ShowMessageBox(
        string text,
        string caption,
        MessageBoxStyles style = MessageBoxStyles.Ok)
    {
        HWND active = Interop.GetActiveWindow();
        return new HandleRef<HWND>(Window.FromHandle(active), active).MessageBox(text, caption, style);
    }

    [Obsolete($"{nameof(ShowMessageBox)} does not support high DPI, use {nameof(ShowTaskDialog)} instead.", error: false)]
    public static DialogResult ShowMessageBox<T>(
        T owner,
        string text,
        string caption,
        MessageBoxStyles style = MessageBoxStyles.Ok)
        where T : IHandle<HWND>
    {
        return owner.MessageBox(text, caption, style);
    }

    public static void Run(
        WindowClass windowClass,
        string? windowTitle = null,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        HMENU menuHandle = default) => Run(
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
        HMENU menuHandle = default) => Run(new Window(
            bounds,
            windowTitle,
            style,
            extendedStyle,
            windowClass: windowClass,
            menuHandle: menuHandle));

    public static void Run(Window window, bool disposeWindow = true)
    {
        try
        {
            window.MessageHandler += Window_QuitHandler;

            window.ShowWindow(ShowWindowCommand.Normal);
            window.UpdateWindow();

            while (Interop.GetMessage(out MSG message, HWND.Null, 0, 0))
            {
                if (Window.FromHandle(message.hwnd) is { } target && target.PreProcessMessage(ref message))
                {
                    continue;
                }

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
        finally
        {
            if (disposeWindow)
            {
                window.Dispose();
            }
        }
    }

    private static LRESULT? Window_QuitHandler(object obj, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if (message == MessageType.Destroy)
        {
            Interop.PostQuitMessage(0);
        }

        return null;
    }

    /// <summary>
    ///  If <see langword="true"/>, styles support for common controls will be used for newly created controls. If
    ///  <see langword="false"/> the application manifest setting (if any) will be used.
    /// </summary>
    public static bool UseVisualStyles { get; set; } = true;

    private static ActivationContext? GetStylesContext()
    {
        if (!UseVisualStyles)
        {
            return null;
        }

        if (s_visualStylesContext is not null)
        {
            return s_visualStylesContext;
        }

        HINSTANCE instance = (HINSTANCE)Marshal.GetHINSTANCE(typeof(Application).Module);
        if (!instance.IsNull && instance != (HINSTANCE)(-1))
        {
            // We have a native module, point to our native embedded manifest resource.
            // CSC embeds DLL manifests as native resource ID 2.
            s_visualStylesContext = new ActivationContext(instance, nativeResourceManifestID: 2);
        }

        return s_visualStylesContext;
    }

    /// <summary>
    ///  Enters a modal scope for the current thread. All active visible windows are disabled until the returned
    ///  scope is disposed.
    /// </summary>
    public static ThreadModalScope EnterThreadModalScope() => new();

    /// <summary>
    ///  Enumerates thread windows for the current thread.
    /// </summary>
    /// <param name="callback">
    ///  The provided function will be passed thread window handles. Return <see langword="true"/> to continue enumeration.
    /// </param>
    public static void EnumerateThreadWindows(
        Func<HWND, bool> callback)
    {
        using var enumerator = new ThreadWindowEnumerator(Interop.GetCurrentThreadId(), callback);
    }

    /// <summary>
    ///  Enumerates thread windows for the given <paramref name="threadId"/>.
    /// </summary>
    /// <param name="callback">
    ///  The provided function will be passed thread window handles. Return <see langword="true"/> to continue enumeration.
    /// </param>
    public static void EnumerateThreadWindows(
        uint threadId,
        Func<HWND, bool> callback)
    {
        using var enumerator = new ThreadWindowEnumerator(threadId, callback);
    }
}