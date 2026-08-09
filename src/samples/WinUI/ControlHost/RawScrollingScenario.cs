// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal sealed unsafe class RawScrollingScenario : IDisposable
{
    private static readonly SET_WINDOW_POS_FLAGS s_moveFlags =
        SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;

    internal static Rectangle InitialViewportBounds { get; } = new(80, 70, 500, 320);

    internal static Rectangle MovedViewportBounds { get; } = new(140, 110, 500, 320);

    internal static Rectangle InitialContentBounds { get; } = new(0, 0, 900, 600);

    internal static Rectangle ScrolledContentBounds { get; } = new(-250, -160, 900, 600);

    internal static Rectangle HostBounds { get; } = new(180, 120, 320, 200);

    internal static Rectangle FocusBounds { get; } = new(650, 80, 160, 60);

    private readonly HWND _parent;
    private readonly ScenarioReporter _reporter;
    private readonly RawScrollingScene _scene;
    private readonly DesktopWindowXamlSource _source;
    private readonly FrameworkElement _island;
    private DispatcherQueueTimer? _captureTimer;
    private bool _islandLoaded;
    private bool _started;
    private bool _captureScheduled;
    private bool _disposed;

    internal RawScrollingScenario(HWND parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;
        ResizeParent();
        _scene = RawScrollingScene.Create(parent);
        _source = _scene.Host.Source;
        _island = _scene.Host.Content;
        _island.Loaded += IslandLoaded;
    }

    internal void Start()
    {
        _started = true;
        TryScheduleCapture();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_captureTimer is not null)
        {
            _captureTimer.Stop();
            _captureTimer.Tick -= CaptureTimerTick;
            _captureTimer = null;
        }

        _island.Loaded -= IslandLoaded;
        _scene.Dispose();
        _reporter.Write("scrolling-disposed");
    }

    private void IslandLoaded(object sender, RoutedEventArgs eventArgs)
    {
        _island.Loaded -= IslandLoaded;
        _islandLoaded = true;
        TryScheduleCapture();
    }

    private void TryScheduleCapture()
    {
        if (!_started || !_islandLoaded || _captureScheduled || _disposed)
        {
            return;
        }

        RunMovementStages();
        _captureScheduled = true;
        _captureTimer = _island.DispatcherQueue.CreateTimer();
        _captureTimer.Interval = TimeSpan.FromMilliseconds(100);
        _captureTimer.IsRepeating = false;
        _captureTimer.Tick += CaptureTimerTick;
        _captureTimer.Start();
    }

    private void CaptureTimerTick(DispatcherQueueTimer sender, object arguments)
    {
        sender.Stop();
        sender.Tick -= CaptureTimerTick;
        _captureTimer = null;
        ReportCaptureReadySafely();
    }

    private void RunMovementStages()
    {
        _ = PInvoke.SetFocus(_scene.FocusTarget);
        ReportObservation(
            "scroll-initial-verified",
            new(80, 70, 0, 0, 180, 120, 320, 200, 0, 0, 320, 200, true, true));

        MoveWindow(_scene.Content, ScrolledContentBounds.Location);
        ReportObservation(
            "scroll-content-translated",
            new(80, 70, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true));

        MoveWindow(_scene.Viewport, MovedViewportBounds.Location);
        ReportObservation(
            "scroll-viewport-moved",
            new(140, 110, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true));

        Rectangle viewport = GetWindowRectangle(_scene.Viewport);
        Rectangle host = GetWindowRectangle(_scene.Host.Handle);
        Ensure(host.Left < viewport.Left && host.Top < viewport.Top, "The translated host did not cross both viewport edges.");
        Ensure(host.Right > viewport.Left && host.Bottom > viewport.Top, "The translated host was completely outside the viewport.");
        _reporter.Write("scroll-bounds-synchronized", _parent);
    }

    private void ReportObservation(string eventName, ScrollingObservation expected)
    {
        ScrollingObservation actual = Observe();
        Ensure(actual == expected, $"{eventName} geometry differed. Expected {expected}; actual {actual}.");
        _reporter.Write(eventName, _parent, JsonSerializer.Serialize(actual));
    }

    private ScrollingObservation Observe()
    {
        Rectangle viewport = GetWindowRectangle(_scene.Viewport);
        Rectangle content = GetWindowRectangle(_scene.Content);
        Rectangle host = GetWindowRectangle(_scene.Host.Handle);
        Rectangle site = GetWindowRectangle(_scene.Host.SiteBridge);
        Size hostSize = GetClientSize(_scene.Host.Handle);
        Size siteSize = GetClientSize(_scene.Host.SiteBridge);
        Point viewportInParent = ScreenToClient(_parent, viewport.Location);
        Point contentInViewport = ScreenToClient(_scene.Viewport, content.Location);
        Point hostInViewport = ScreenToClient(_scene.Viewport, host.Location);
        Point hostClientOrigin = ClientToScreen(_scene.Host.Handle, Point.Empty);

        return new(
            viewportInParent.X,
            viewportInParent.Y,
            contentInViewport.X,
            contentInViewport.Y,
            hostInViewport.X,
            hostInViewport.Y,
            hostSize.Width,
            hostSize.Height,
            site.X - hostClientOrigin.X,
            site.Y - hostClientOrigin.Y,
            siteSize.Width,
            siteSize.Height,
            ReferenceEquals(_scene.Host.Source, _source),
            PInvoke.GetFocus() == _scene.FocusTarget);
    }

    private void ReportCaptureReady()
    {
        if (_disposed)
        {
            return;
        }

        Dictionary<string, ScrollingSample> samples = new()
        {
            ["hostVisible"] = CreateSample(_scene.Host.Handle, new Point(100, 70)),
            ["hostClippedLeft"] = CreateSample(_scene.Host.Handle, new Point(40, 70)),
            ["hostClippedTop"] = CreateSample(_scene.Host.Handle, new Point(100, 20)),
            ["contentExposed"] = CreateSample(_scene.Viewport, new Point(400, 280)),
            ["focusTarget"] = CreateSample(_scene.FocusTarget, new Point(20, 20))
        };

        UpdateWindow(_scene.FocusTarget);
        UpdateWindow(_scene.Content);
        UpdateWindow(_scene.Viewport);
        UpdateWindow(_parent);
        _reporter.Write("capture-ready", _parent, JsonSerializer.Serialize(samples));
    }

    private void ReportCaptureReadySafely()
    {
        try
        {
            ReportCaptureReady();
        }
        catch (Exception exception)
        {
            _reporter.Write("capture-failed", _parent, exception.ToString());
            _ = PInvoke.PostMessage(_parent, Interop.WM_CLOSE, default, default);
        }
    }

    private ScrollingSample CreateSample(HWND window, Point clientPoint)
    {
        Point screenPoint = ClientToScreen(window, clientPoint);
        Rectangle parentBounds = GetWindowRectangle(_parent);
        return new(screenPoint.X - parentBounds.X, screenPoint.Y - parentBounds.Y);
    }

    private void ResizeParent()
    {
        Rectangle bounds = GetWindowRectangle(_parent);
        if (!PInvoke.MoveWindow(_parent, bounds.X, bounds.Y, 900, 600, true))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    private static void MoveWindow(HWND window, Point location)
    {
        if (!PInvoke.SetWindowPos(window, HWND.Null, location.X, location.Y, 0, 0, s_moveFlags))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    private static Rectangle GetWindowRectangle(HWND window)
    {
        if (!PInvoke.GetWindowRect(window, out RECT bounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return bounds;
    }

    private static Size GetClientSize(HWND window)
    {
        if (!PInvoke.GetClientRect(window, out RECT bounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return ((Rectangle)bounds).Size;
    }

    private static Point ScreenToClient(HWND window, Point point)
    {
        if (!PInvoke.ScreenToClient(window, &point))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return point;
    }

    private static Point ClientToScreen(HWND window, Point point)
    {
        if (!PInvoke.ClientToScreen(window, &point))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return point;
    }

    private static void UpdateWindow(HWND window)
    {
        if (!PInvoke.UpdateWindow(window))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}