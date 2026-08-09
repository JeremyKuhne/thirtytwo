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
    private static RawAirspaceScenario? s_airspaceScenario;
    private static RawScrollingScenario? s_scrollingScenario;
    private static ScenarioReporter? s_reporter;
    private static ControlHostScenario s_scenario;

    [STAThread]
    private static int Main(string[] arguments)
    {
        try
        {
            s_scenario = ScenarioArguments.Parse(arguments);
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }

        s_reporter = s_scenario == ControlHostScenario.Interactive ? null : new(s_scenario);
        s_reporter?.Write("process-started");

        try
        {
            DispatcherQueueController dispatcher = DispatcherQueueController.CreateOnCurrentThread();
            s_reporter?.Write("dispatcher-queue-created");
            try
            {
                using XamlApplication application = new();
                s_reporter?.Write("xaml-application-created");
                RunMessageLoop();
                GC.KeepAlive(application);
            }
            finally
            {
                s_reporter?.Write("dispatcher-queue-shutdown-started");
                dispatcher.ShutdownQueue();
                s_reporter?.Write("dispatcher-queue-shutdown-completed");
            }

            s_reporter?.Write("scenario-completed");
            return 0;
        }
        catch (Exception exception) when (s_reporter is not null)
        {
            s_reporter.Write("scenario-failed", message: exception.ToString());
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void RunMessageLoop()
    {
        const string ClassName = "ThirtyTwoWinUIControlHost";
        WindowProcedure windowProcedure = WindowProcedure;

        HMODULE module;
        if (!PInvoke.GetModuleHandleEx(
            Interop.GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (PCWSTR)null,
            &module))
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
            s_reporter?.Write("ready", window);
            s_airspaceScenario?.Start();
            s_scrollingScenario?.Start();

            if (s_scenario == ControlHostScenario.Startup)
            {
                if (!PInvoke.PostMessage(window, Interop.WM_CLOSE, default, default))
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }

                s_reporter?.Write("close-requested", window);
            }

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
                s_reporter?.Write("window-created", window);
                if (s_scenario == ControlHostScenario.Airspace)
                {
                    s_airspaceScenario = new(
                        window,
                        s_reporter ?? throw new InvalidOperationException("The airspace scenario requires a reporter."));
                    return (LRESULT)0;
                }

                if (s_scenario == ControlHostScenario.Scrolling)
                {
                    s_scrollingScenario = new(
                        window,
                        s_reporter ?? throw new InvalidOperationException("The scrolling scenario requires a reporter."));
                    return (LRESULT)0;
                }

                s_xamlSource = new DesktopWindowXamlSource();
                s_xamlSource.Initialize(Win32Interop.GetWindowIdFromWindow((nint)window.Value));
                s_xamlSource.ShouldConstrainPopupsToWorkArea = true;

                if (s_scenario == ControlHostScenario.UiaTree)
                {
                    s_xamlSource.Content = new AccessibilityContent(
                        s_reporter ?? throw new InvalidOperationException("The UIA scenario requires a reporter."));
                    return (LRESULT)0;
                }

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

            case Interop.WM_CLOSE:
                if (s_scenario == ControlHostScenario.ShutdownTimeout)
                {
                    s_reporter?.Write("close-ignored", window);
                    return (LRESULT)0;
                }

                s_reporter?.Write("close-received", window);
                if (s_scenario is ControlHostScenario.Airspace or ControlHostScenario.Scrolling)
                {
                    s_airspaceScenario?.Dispose();
                    s_scrollingScenario?.Dispose();
                }

                if (!PInvoke.DestroyWindow(window))
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                }

                return (LRESULT)0;

            case Interop.WM_DESTROY:
                DisposeIsland();
                s_reporter?.Write("window-destroyed", window);
                PInvoke.PostQuitMessage(0);
                return (LRESULT)0;
        }

        return PInvoke.DefWindowProc(window, message, wParam, lParam);
    }

    private static void DisposeIsland()
    {
        s_airspaceScenario?.Dispose();
        s_airspaceScenario = null;
        s_scrollingScenario?.Dispose();
        s_scrollingScenario = null;

        if (s_xamlSource is null)
        {
            return;
        }

        s_xamlSource.Content = null;
        s_xamlSource.Dispose();
        s_xamlSource = null;
        s_root = null;
        s_colorPicker = null;
        s_reporter?.Write("island-disposed");
    }
}