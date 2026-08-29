// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Support;
using Windows.Threading;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.Graphics.Imaging;

namespace Windows;

/// <summary>
///  Main application class.
/// </summary>
public static unsafe partial class Application
{
    // Keep all framework-owned WM_APP identifiers together so new allocations cannot silently overlap.

    /// <summary>
    ///  Identifies the dispatcher queue wake message.
    /// </summary>
    internal const uint DispatcherWakeMessage = Interop.WM_APP + 1;

    private static ActivationContext? s_visualStylesContext;
    private static Direct2dFactory? s_direct2dFactory;
    private static DirectWriteFactory? s_directWriteFactory;
    private static DirectWriteGdiInterop? s_directWriteGdiInterop;
    private static ImagingFactory? s_imagingFactory;

    /// <summary>
    ///  Gets an activation scope that enables visual-styles manifests for common controls.
    /// </summary>
    /// <returns>An activation scope for the current thread.</returns>
    internal static ActivationScope ThemingScope
    {
        get
        {
            return new(GetStylesContext());

            static ActivationContext? GetStylesContext()
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
        }
    }

    /// <summary>
    ///  Ensures the current thread uses Per-Monitor DPI awareness for Win32 window creation.
    /// </summary>
    internal static void EnsureDpiAwareness()
    {
        // Enable High DPI awareness if not enabled already. Requires Windows 10.
        if (PInvoke.GetAwarenessFromDpiAwarenessContext(PInvoke.GetThreadDpiAwarenessContext()) == DPI_AWARENESS.DPI_AWARENESS_UNAWARE
            && PInvoke.SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2).IsNull)
        {
            // Fall back from V2 if needed
            PInvoke.SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE);
        }
    }

    /// <summary>
    ///  Shows a task dialog owned by the current active window.
    /// </summary>
    /// <param name="mainInstruction">Primary instruction text shown prominently.</param>
    /// <param name="content">Additional content text.</param>
    /// <param name="title">Dialog caption text.</param>
    /// <param name="buttons">Standard command buttons to display.</param>
    /// <param name="icon">Optional predefined main icon.</param>
    /// <returns>The command result selected by the user.</returns>
    public static DialogResult ShowTaskDialog(
        string? mainInstruction = null,
        string? content = null,
        string? title = null,
        TaskDialogButtons buttons = TaskDialogButtons.Ok,
        TaskDialogIcon? icon = null)
    {
        HWND active = PInvoke.GetActiveWindow();
        return new HandleRef<HWND>(Window.FromHandle(active), active).ShowTaskDialog(mainInstruction, content, title, buttons, icon);
    }

    /// <summary>
    ///  Shows a task dialog owned by the specified window handle wrapper.
    /// </summary>
    /// <typeparam name="T">The owner wrapper type.</typeparam>
    /// <param name="owner">The native owner window.</param>
    /// <param name="mainInstruction">Primary instruction text shown prominently.</param>
    /// <param name="content">Additional content text.</param>
    /// <param name="title">Dialog caption text.</param>
    /// <param name="buttons">Standard command buttons to display.</param>
    /// <param name="icon">Optional predefined main icon.</param>
    /// <returns>The command result selected by the user.</returns>
    public static DialogResult ShowTaskDialog<T>(
        T owner,
        string? mainInstruction = null,
        string? content = null,
        string? title = null,
        TaskDialogButtons buttons = TaskDialogButtons.Ok,
        TaskDialogIcon? icon = null)
        where T : IHandle<HWND>
    {
        return owner.ShowTaskDialog(mainInstruction, content, title, buttons, icon);
    }

    // TaskDialog is not a 1-1 replacement for MessageBox. There is very little reason to use MessageBox, but leaving
    // this here to help discovery of ShowTaskDialog.

    /// <summary>
    ///  Shows a legacy message box owned by the current active window.
    /// </summary>
    /// <param name="text">Message text.</param>
    /// <param name="caption">Window caption text.</param>
    /// <param name="style">Button and icon style flags.</param>
    /// <returns>The button selected by the user.</returns>
    [Obsolete($"{nameof(ShowMessageBox)} does not support high DPI, use {nameof(ShowTaskDialog)} instead.", error: false)]
    public static DialogResult ShowMessageBox(
        string text,
        string caption,
        MessageBoxStyles style = MessageBoxStyles.Ok)
    {
        HWND active = PInvoke.GetActiveWindow();
        return new HandleRef<HWND>(Window.FromHandle(active), active).MessageBox(text, caption, style);
    }

    /// <summary>
    ///  Shows a legacy message box owned by the specified window handle wrapper.
    /// </summary>
    /// <typeparam name="T">The owner wrapper type.</typeparam>
    /// <param name="owner">The native owner window.</param>
    /// <param name="text">Message text.</param>
    /// <param name="caption">Window caption text.</param>
    /// <param name="style">Button and icon style flags.</param>
    /// <returns>The button selected by the user.</returns>
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

    /// <summary>
    ///  Creates, shows, and runs a root window with default bounds.
    /// </summary>
    /// <param name="windowClass">The window class used to create the root window.</param>
    /// <param name="windowTitle">Optional title text.</param>
    /// <param name="style">Window style flags.</param>
    /// <param name="extendedStyle">Extended window style flags.</param>
    /// <param name="menuHandle">Optional native menu handle.</param>
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

    /// <summary>
    ///  Creates, shows, and runs a root window with explicit bounds.
    /// </summary>
    /// <param name="windowClass">The window class used to create the root window.</param>
    /// <param name="bounds">Initial window bounds.</param>
    /// <param name="windowTitle">Optional title text.</param>
    /// <param name="style">Window style flags.</param>
    /// <param name="extendedStyle">Extended window style flags.</param>
    /// <param name="menuHandle">Optional native menu handle.</param>
    public static void Run(
        WindowClass windowClass,
        Rectangle bounds,
        string? windowTitle = null,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        HMENU menuHandle = default) => Run(() => new Window(
            bounds,
            windowTitle,
            style,
            extendedStyle,
            windowClass: windowClass,
            menuHandle: menuHandle));

    /// <summary>
    ///  Creates the root window with an active dispatcher, then shows it and runs the UI message loop.
    /// </summary>
    /// <param name="windowFactory">Creates the root window on the current UI thread.</param>
    public static void Run(Func<Window> windowFactory)
    {
        ArgumentNullException.ThrowIfNull(windowFactory);
        Run(windowFactory, disposeWindow: true);
    }

    /// <summary>
    ///  Associates and shows an existing root window, then runs the UI message loop.
    /// </summary>
    /// <param name="window">The root window.</param>
    /// <param name="disposeWindow">Whether to dispose the window after the message loop exits.</param>
    public static void Run(Window window, bool disposeWindow = true)
    {
        ArgumentNullException.ThrowIfNull(window);
        Run(() => window, disposeWindow);
    }

    private static void Run(Func<Window> windowFactory, bool disposeWindow)
    {
        using ThreadContext threadContext = ThreadContext.Create();
        Window? window = null;

        try
        {
            threadContext.RunMessageLoop(() =>
            {
                window = windowFactory()
                    ?? throw new InvalidOperationException("The window factory returned null.");
                window.AttachDispatcher(threadContext.Dispatcher);
                window.MessageHandler += Window_QuitHandler;

                window.ShowWindow(ShowWindowCommand.Normal);
                window.UpdateWindow();
            });

            // Make sure our window doesn't get collected while we're pumping messages
            GC.KeepAlive(window);
        }
        catch
        {
            if (window is not null && !window.Handle.IsNull)
            {
                PInvoke.DestroyWindow(window);
            }

            throw;
        }
        finally
        {
            if (window is not null)
            {
                window.MessageHandler -= Window_QuitHandler;
            }

            if (disposeWindow)
            {
                window?.Dispose();
            }
        }

        static LRESULT? Window_QuitHandler(object obj, HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
        {
            if (message == MessageType.Destroy)
            {
                ThreadContext.RequestExitCurrentThread();
            }

            return null;
        }
    }

    /// <summary>
    ///  If <see langword="true"/>, styles support for common controls will be used for newly created controls. If
    ///  <see langword="false"/> the application manifest setting (if any) will be used.
    /// </summary>
    public static bool UseVisualStyles { get; set; } = true;

    /// <summary>
    ///  Enters a modal scope for the current thread. All active visible windows are disabled until the returned
    ///  scope is disposed.
    /// </summary>
    /// <returns>A modal scope that reenables affected windows when disposed.</returns>
    public static ThreadModalScope EnterThreadModalScope() => new();

    /// <summary>
    ///  Adds a message filter to the current UI thread. Filters run in registration order.
    /// </summary>
    /// <param name="filter">The filter to register for the current thread message loop.</param>
    /// <returns>A registration token that removes the filter when disposed.</returns>
    /// <exception cref="InvalidOperationException">A message loop is not active on this thread.</exception>
    public static MessageFilterRegistration AddMessageFilter(IMessageFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        ThreadContext context = ThreadContext.CurrentContext
            ?? throw new InvalidOperationException("A message loop is not running on this thread.");

        return context.AddMessageFilter(filter);
    }

    /// <summary>
    ///  Enumerates thread windows for the current thread.
    /// </summary>
    /// <param name="callback">
    ///  The provided function will be passed thread window handles. Return <see langword="true"/> to continue enumeration.
    /// </param>
    public static void EnumerateThreadWindows(
        Func<HWND, bool> callback)
    {
        using var enumerator = new ThreadWindowEnumerator(PInvoke.GetCurrentThreadId(), callback);
    }

    /// <summary>
    ///  Enumerates thread windows for the given <paramref name="threadId"/>.
    /// </summary>
    /// <param name="threadId">The native thread identifier whose top-level and owned windows are enumerated.</param>
    /// <param name="callback">
    ///  The provided function will be passed thread window handles. Return <see langword="true"/> to continue enumeration.
    /// </param>
    public static void EnumerateThreadWindows(
        uint threadId,
        Func<HWND, bool> callback)
    {
        using var enumerator = new ThreadWindowEnumerator(threadId, callback);
    }

    /// <summary>
    ///  Gets the current user's default locale name.
    /// </summary>
    /// <returns>A BCP-47 locale name such as <c>en-US</c>.</returns>
    public static string GetUserDefaultLocaleName()
    {
        Span<char> localeName = stackalloc char[(int)PInvoke.LOCALE_NAME_MAX_LENGTH];
        fixed (char* ln = localeName)
        {
            int length = PInvoke.GetUserDefaultLocaleName(ln, (int)PInvoke.LOCALE_NAME_MAX_LENGTH);

            if (length == 0)
            {
                Error.GetLastError().ThrowThirtyTwoException();
            }

            return localeName[..(length - 1)].ToString();
        }
    }

    /// <inheritdoc cref="Win32.Graphics.Direct2D.Direct2dFactory"/>
    public static Direct2dFactory Direct2dFactory => s_direct2dFactory ??= new();

    /// <summary>
    ///  Factory that is used to create DirectWrite resources.
    /// </summary>
    /// <inheritdoc cref="Win32.Graphics.DirectWrite.DirectWriteFactory"/>
    public static DirectWriteFactory DirectWriteFactory => s_directWriteFactory ??= new();

    /// <inheritdoc cref="Win32.Graphics.DirectWrite.DirectWriteGdiInterop"/>
    public static DirectWriteGdiInterop DirectWriteGdiInterop => s_directWriteGdiInterop ??= new();

    /// <inheritdoc cref="Windows.Win32.Graphics.Imaging.ImagingFactory"/>
    public static ImagingFactory ImagingFactory => s_imagingFactory ??= new();
}