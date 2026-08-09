// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Touki.TestSupport;
using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.WinUI;
using DrawingColor = System.Drawing.Color;
using NativeWindow = Windows.Window;
using XamlHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using XamlVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace IntegrationHost;

internal sealed class ScrollingScenario : IDisposable
{
    private static readonly WindowPositionFlags s_moveFlags =
        WindowPositionFlags.NoSize | WindowPositionFlags.NoZOrder | WindowPositionFlags.NoActivate;

    private static Rectangle InitialViewportBounds { get; } = new(80, 70, 500, 320);

    private static Rectangle MovedViewportBounds { get; } = new(140, 110, 500, 320);

    private static Rectangle InitialContentBounds { get; } = new(0, 0, 900, 600);

    private static Rectangle ScrolledContentBounds { get; } = new(-250, -160, 900, 600);

    private static Rectangle HostBounds { get; } = new(180, 120, 320, 200);

    private static Rectangle FocusBounds { get; } = new(650, 80, 160, 60);

    private readonly NativeWindow _parent;
    private readonly ScenarioReporter _reporter;
    private readonly CustomControl _viewport;
    private readonly CustomControl _content;
    private readonly XamlHostControl _host;
    private readonly TextLabelControl _focusTarget;
    private readonly FrameworkElement _island;
    private readonly DesktopWindowXamlSource _source;
    private readonly HWND _siteBridge;
    private bool _islandLoaded;
    private bool _started;
    private bool _captureScheduled;
    private bool _disposed;

    internal ScrollingScenario(NativeWindow parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;

        Rectangle parentBounds = parent.GetWindowRectangle();
        parent.MoveWindow(new Rectangle(parentBounds.Location, new Size(900, 600)), repaint: false);

        CustomControl? viewport = null;
        CustomControl? content = null;
        XamlHostControl? host = null;
        TextLabelControl? focusTarget = null;
        FrameworkElement? island = null;
        FrameworkElement createdIsland;
        DesktopWindowXamlSource source;
        HWND siteBridge;
        try
        {
            viewport = new(
                bounds: InitialViewportBounds,
                style: WindowStyles.Child | WindowStyles.Visible
                    | WindowStyles.ClipChildren | WindowStyles.ClipSiblings,
                parentWindow: parent,
                backgroundColor: DrawingColor.FromArgb(255, 48, 48, 48));
            content = new(
                bounds: InitialContentBounds,
                style: WindowStyles.Child | WindowStyles.Visible
                    | WindowStyles.ClipChildren | WindowStyles.ClipSiblings,
                parentWindow: viewport,
                backgroundColor: DrawingColor.FromArgb(255, 24, 24, 24));
            host = new(HostBounds, content, () => island = CreateIsland());
            focusTarget = new(
                bounds: FocusBounds,
                textFormat: DrawTextFormat.Center | DrawTextFormat.VerticallyCenter | DrawTextFormat.SingleLine,
                text: "Focus anchor",
                textColor: DrawingColor.White,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.ClipSiblings,
                parentWindow: parent,
                backgroundColor: DrawingColor.SeaGreen,
                features: NativeWindow.Features.EnableDirect2d);
            createdIsland = island ?? throw new InvalidOperationException("The scrolling XAML island was not created.");
            source = GetXamlSource(host);
            siteBridge = GetSiteBridge(source);
        }
        catch
        {
            focusTarget?.Dispose();
            host?.Dispose();
            content?.Dispose();
            viewport?.Dispose();
            throw;
        }

        _viewport = viewport;
        _content = content;
        _host = host;
        _focusTarget = focusTarget;
        _island = createdIsland;
        _source = source;
        _siteBridge = siteBridge;
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
        _island.Loaded -= IslandLoaded;
        _focusTarget.Dispose();
        _host.Dispose();
        _content.Dispose();
        _viewport.Dispose();
        _reporter.Write("scrolling-disposed");
    }

    private static Border CreateIsland()
        => new()
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue),
            Child = new TextBlock
            {
                Text = "Translated XAML island",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = XamlHorizontalAlignment.Center,
                VerticalAlignment = XamlVerticalAlignment.Center
            }
        };

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
        _ = _parent.Dispatcher.InvokeAsync(TimeSpan.FromMilliseconds(100), ReportCaptureReadySafely);
    }

    private void RunMovementStages()
    {
        _ = _focusTarget.SetFocus();
        ReportObservation(
            "scroll-initial-verified",
            new(80, 70, 0, 0, 180, 120, 320, 200, 0, 0, 320, 200, true, true));

        MoveWindow(_content, ScrolledContentBounds.Location);
        ReportObservation(
            "scroll-content-translated",
            new(80, 70, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true));

        MoveWindow(_viewport, MovedViewportBounds.Location);
        ReportObservation(
            "scroll-viewport-moved",
            new(140, 110, -250, -160, -70, -40, 320, 200, 0, 0, 320, 200, true, true));

        Rectangle viewport = _viewport.GetWindowRectangle();
        Rectangle host = _host.GetWindowRectangle();
        Ensure(host.Left < viewport.Left && host.Top < viewport.Top, "The translated host did not cross both viewport edges.");
        Ensure(host.Right > viewport.Left && host.Bottom > viewport.Top, "The translated host was completely outside the viewport.");
        _reporter.Write("scroll-bounds-synchronized", _parent.Handle);
    }

    private void ReportObservation(string eventName, ScrollingObservation expected)
    {
        ScrollingObservation actual = Observe();
        Ensure(actual == expected, $"{eventName} geometry differed. Expected {expected}; actual {actual}.");
        _reporter.Write(eventName, _parent.Handle, JsonSerializer.Serialize(actual));
    }

    private ScrollingObservation Observe()
    {
        Rectangle viewport = _viewport.GetWindowRectangle();
        Rectangle content = _content.GetWindowRectangle();
        Rectangle host = _host.GetWindowRectangle();
        Rectangle site = _siteBridge.GetWindowRectangle();
        Size hostSize = _host.GetClientRectangle().Size;
        Size siteSize = _siteBridge.GetClientRectangle().Size;
        Point viewportInParent = ScreenToClient(_parent, viewport.Location);
        Point contentInViewport = ScreenToClient(_viewport, content.Location);
        Point hostInViewport = ScreenToClient(_viewport, host.Location);
        Point hostClientOrigin = ClientToScreen(_host, Point.Empty);

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
            ReferenceEquals(GetXamlSource(_host), _source) && GetSiteBridge(_source) == _siteBridge,
            PInvoke.GetFocus() == _focusTarget.Handle);
    }

    private void ReportCaptureReady()
    {
        if (_disposed)
        {
            return;
        }

        Dictionary<string, ScrollingSample> samples = new()
        {
            ["hostVisible"] = CreateSample(_host, new Point(100, 70)),
            ["hostClippedLeft"] = CreateSample(_host, new Point(40, 70)),
            ["hostClippedTop"] = CreateSample(_host, new Point(100, 20)),
            ["contentExposed"] = CreateSample(_viewport, new Point(400, 280)),
            ["focusTarget"] = CreateSample(_focusTarget, new Point(20, 20))
        };

        _focusTarget.UpdateWindow();
        _content.UpdateWindow();
        _viewport.UpdateWindow();
        _parent.UpdateWindow();
        _reporter.Write(
            "capture-ready",
            _parent.Handle,
            JsonSerializer.Serialize(samples));
    }

    private void ReportCaptureReadySafely()
    {
        try
        {
            ReportCaptureReady();
        }
        catch (Exception exception)
        {
            _reporter.Write("capture-failed", _parent.Handle, exception.ToString());
            _parent.PostMessage(MessageType.Close);
        }
    }

    private ScrollingSample CreateSample(NativeWindow window, Point clientPoint)
    {
        Point screenPoint = ClientToScreen(window, clientPoint);
        Rectangle parentBounds = _parent.GetWindowRectangle();
        return new(screenPoint.X - parentBounds.X, screenPoint.Y - parentBounds.Y);
    }

    private static void MoveWindow(NativeWindow window, Point location)
        => window.SetWindowPosition(
            WindowZOrder.Top,
            new Rectangle(location, Size.Empty),
            s_moveFlags);

    private static Point ScreenToClient(NativeWindow window, Point point)
    {
        if (!window.ScreenToClient(ref point))
        {
            throw new InvalidOperationException("Could not map a scrolling observation to client coordinates.");
        }

        return point;
    }

    private static Point ClientToScreen(NativeWindow window, Point point)
    {
        if (!window.ClientToScreen(ref point))
        {
            throw new InvalidOperationException("Could not map a scrolling sample to screen coordinates.");
        }

        return point;
    }

    private static DesktopWindowXamlSource GetXamlSource(XamlHostControl host)
        => host.TestAccessor.Dynamic._xamlSource
            ?? throw new InvalidOperationException("The host has no XAML source.");

    private static HWND GetSiteBridge(DesktopWindowXamlSource source)
        => (HWND)Win32Interop.GetWindowFromWindowId(source.SiteBridge.WindowId);

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}