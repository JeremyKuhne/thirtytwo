using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MinimalWinUIHost;

internal static unsafe class Program
{
    private const string WindowClassName = "MinimalWinUIHostWindow";

    private static DesktopWindowXamlSource? s_xamlSource;

    [STAThread]
    private static int Main()
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new InvalidOperationException("WinUI hosting requires an STA thread.");
        }

        DispatcherQueueController dispatcher = DispatcherQueueController.CreateOnCurrentThread();

        try
        {
            using XamlApplication application = new();
            int result = RunMessageLoop();
            GC.KeepAlive(application);
            return result;
        }
        finally
        {
            dispatcher.ShutdownQueue();
        }
    }

    private static int RunMessageLoop()
    {
        HWND window = CreateHostWindow();
        try
        {
            InitializeIsland(window);
            PInvoke.ShowWindow(window, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
            if (!PInvoke.UpdateWindow(window))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
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
                    return (int)message.wParam.Value;
                }

                if (WindowsAppSdkInterop.ContentPreTranslateMessage(&message) != 0)
                {
                    continue;
                }

                _ = PInvoke.TranslateMessage(message);
                _ = PInvoke.DispatchMessage(message);
            }
        }
        finally
        {
            DisposeIsland();
            if (PInvoke.IsWindow(window))
            {
                _ = PInvoke.DestroyWindow(window);
            }
        }
    }

    private static HWND CreateHostWindow()
    {
        HMODULE module = PInvoke.GetModuleHandle((PCWSTR)null);
        if (module.IsNull)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        fixed (char* className = WindowClassName)
        {
            WNDCLASSEXW windowClass = new()
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                lpfnWndProc = &HostWindowProcedure,
                hInstance = module,
                hbrBackground = (HBRUSH)(nint)((int)SYS_COLOR_INDEX.COLOR_WINDOW + 1),
                lpszClassName = className
            };

            var atom = PInvoke.RegisterClassEx(&windowClass);
            if (atom == 0)
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            fixed (char* title = "Minimal WinUI 3 island host")
            {
                HWND window = PInvoke.CreateWindowEx(
                    WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,
                    className,
                    title,
                    WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                    PInvoke.CW_USEDEFAULT,
                    PInvoke.CW_USEDEFAULT,
                    800,
                    500,
                    HWND.Null,
                    HMENU.Null,
                    module,
                    null);

                return window.IsNull
                    ? throw new Win32Exception(Marshal.GetLastPInvokeError())
                    : window;
            }
        }
    }

    private static void InitializeIsland(HWND window)
    {
        DesktopWindowXamlSource source = new();
        try
        {
            source.Initialize(Win32Interop.GetWindowIdFromWindow((nint)window.Value));
            source.ShouldConstrainPopupsToWorkArea = true;

            StackPanel panel = new()
            {
                Padding = new Thickness(24),
                // Win32 applications are always light mode by default.
                RequestedTheme = ElementTheme.Light,
                Spacing = 12
            };

            panel.Children.Add(new TextBlock
            {
                Text = "WinUI 3 hosted by a raw Win32 HWND",
                FontSize = 24
            });

            panel.Children.Add(new TextBox
            {
                Header = "Keyboard input",
                PlaceholderText = "Type here"
            });

            panel.Children.Add(new Button { Content = "WinUI button" });

            source.Content = panel;
            ResizeSiteBridge(source, window);
            s_xamlSource = source;
        }
        catch
        {
            source.Content = null;
            source.Dispose();
            throw;
        }
    }

    private static void ResizeSiteBridge(DesktopWindowXamlSource source, HWND window)
    {
        if (!PInvoke.GetClientRect(window, out RECT bounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        source.SiteBridge?.MoveAndResize(new RectInt32(
            0,
            0,
            bounds.right - bounds.left,
            bounds.bottom - bounds.top));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static LRESULT HostWindowProcedure(HWND window, uint message, WPARAM wParam, LPARAM lParam)
    {
        try
        {
            switch (message)
            {
                case PInvoke.WM_SIZE:
                    if (s_xamlSource is { } source)
                    {
                        nuint packedSize = (nuint)lParam.Value;
                        source.SiteBridge?.MoveAndResize(new RectInt32(
                            0,
                            0,
                            (ushort)packedSize,
                            (ushort)(packedSize >> 16)));
                    }

                    return (LRESULT)0;
                case PInvoke.WM_SETFOCUS:
                    _ = s_xamlSource?.NavigateFocus(
                        new XamlSourceFocusNavigationRequest(XamlSourceFocusNavigationReason.First));
                    return (LRESULT)0;
                case PInvoke.WM_CLOSE:
                    _ = PInvoke.DestroyWindow(window);
                    return (LRESULT)0;
                case PInvoke.WM_DESTROY:
                    DisposeIsland();
                    PInvoke.PostQuitMessage(0);
                    return (LRESULT)0;
            }
        }
        catch
        {
            DisposeIsland();
            PInvoke.PostQuitMessage(1);
        }

        return PInvoke.DefWindowProc(window, message, wParam, lParam);
    }

    private static void DisposeIsland()
    {
        DesktopWindowXamlSource? source = s_xamlSource;
        s_xamlSource = null;
        if (source is null)
        {
            return;
        }

        source.Content = null;
        source.Dispose();
    }
}
