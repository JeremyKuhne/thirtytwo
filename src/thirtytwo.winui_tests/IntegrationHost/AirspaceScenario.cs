// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows;
using Windows.Win32;
using Windows.WinUI;
using DrawingColor = System.Drawing.Color;
using NativeWindow = Windows.Window;
using WinUIColor = Windows.UI.Color;
using XamlHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using XamlVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace IntegrationHost;

internal sealed class AirspaceScenario : IDisposable
{
    private static readonly WindowPositionFlags s_zOrderFlags =
        WindowPositionFlags.NoMove | WindowPositionFlags.NoSize | WindowPositionFlags.NoActivate;

    private readonly NativeWindow _parent;
    private readonly ScenarioReporter _reporter;
    private readonly CustomControl _viewport;
    private readonly XamlHostControl _hostUnderNative;
    private readonly TextLabelControl _nativeAbove;
    private readonly TextLabelControl _nativeUnder;
    private readonly XamlHostControl _hostAboveNative;
    private readonly XamlHostControl _clippedHost;
    private readonly FrameworkElement[] _islands;
    private int _loadedIslandCount;
    private bool _started;
    private bool _captureScheduled;
    private bool _disposed;

    internal AirspaceScenario(NativeWindow parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;

        Rectangle windowBounds = parent.GetWindowRectangle();
        parent.MoveWindow(new Rectangle(windowBounds.X, windowBounds.Y, 900, 600), repaint: false);

        CustomControl? viewport = null;
        XamlHostControl? hostUnderNative = null;
        TextLabelControl? nativeAbove = null;
        TextLabelControl? nativeUnder = null;
        XamlHostControl? hostAboveNative = null;
        XamlHostControl? clippedHost = null;
        FrameworkElement[] islands;
        try
        {
            viewport = new(
                bounds: new Rectangle(40, 40, 800, 460),
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.ClipChildren | WindowStyles.ClipSiblings,
                parentWindow: parent,
                backgroundColor: DrawingColor.FromArgb(255, 24, 24, 24));

            FrameworkElement? underNativeContent = null;
            hostUnderNative = new(new Rectangle(40, 40, 300, 200), viewport, () =>
            {
                underNativeContent = CreateIsland("XAML below native", Microsoft.UI.Colors.RoyalBlue);
                return underNativeContent;
            });

            nativeAbove = CreateNativeLabel(
                viewport,
                new Rectangle(160, 100, 220, 100),
                "Native above XAML",
                DrawingColor.Magenta);

            nativeUnder = CreateNativeLabel(
                viewport,
                new Rectangle(420, 40, 300, 200),
                "Native below XAML",
                DrawingColor.SeaGreen);

            FrameworkElement? aboveNativeContent = null;
            hostAboveNative = new(new Rectangle(540, 100, 220, 100), viewport, () =>
            {
                aboveNativeContent = CreateIsland("XAML above native", Microsoft.UI.Colors.DarkOrange);
                return aboveNativeContent;
            });

            FrameworkElement? clippedContent = null;
            clippedHost = new(new Rectangle(-100, 330, 300, 180), viewport, () =>
            {
                clippedContent = CreateIsland("Negative X, clipped by parent", Microsoft.UI.Colors.Purple);
                return clippedContent;
            });

            islands =
            [
                underNativeContent ?? throw new InvalidOperationException("The lower XAML island was not created."),
                aboveNativeContent ?? throw new InvalidOperationException("The upper XAML island was not created."),
                clippedContent ?? throw new InvalidOperationException("The clipped XAML island was not created.")
            ];
        }
        catch
        {
            clippedHost?.Dispose();
            hostAboveNative?.Dispose();
            nativeUnder?.Dispose();
            nativeAbove?.Dispose();
            hostUnderNative?.Dispose();
            viewport?.Dispose();
            throw;
        }

        _viewport = viewport;
        _hostUnderNative = hostUnderNative;
        _nativeAbove = nativeAbove;
        _nativeUnder = nativeUnder;
        _hostAboveNative = hostAboveNative;
        _clippedHost = clippedHost;
        _islands = islands;

        foreach (FrameworkElement island in _islands)
        {
            island.Loaded += IslandLoaded;
        }
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
        foreach (FrameworkElement island in _islands)
        {
            island.Loaded -= IslandLoaded;
        }

        _clippedHost.Dispose();
        _hostAboveNative.Dispose();
        _nativeUnder.Dispose();
        _nativeAbove.Dispose();
        _hostUnderNative.Dispose();
        _viewport.Dispose();
        _reporter.Write("airspace-disposed");
    }

    private static TextLabelControl CreateNativeLabel(
        CustomControl viewport,
        Rectangle bounds,
        string text,
        DrawingColor backgroundColor)
        => new(
            bounds: bounds,
            textFormat: DrawTextFormat.Center | DrawTextFormat.VerticallyCenter | DrawTextFormat.SingleLine,
            text: text,
            textColor: DrawingColor.White,
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.ClipSiblings,
            parentWindow: viewport,
            backgroundColor: backgroundColor,
            features: NativeWindow.Features.EnableDirect2d);

    private static Border CreateIsland(string text, WinUIColor color)
        => new()
        {
            Background = new SolidColorBrush(color),
            Child = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = XamlHorizontalAlignment.Center,
                VerticalAlignment = XamlVerticalAlignment.Center
            }
        };

    private void IslandLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (sender is FrameworkElement island)
        {
            island.Loaded -= IslandLoaded;
        }

        _loadedIslandCount++;
        TryScheduleCapture();
    }

    private void TryScheduleCapture()
    {
        if (!_started || _loadedIslandCount != _islands.Length || _captureScheduled || _disposed)
        {
            return;
        }

        ArrangeAndVerifyZOrder();
        _captureScheduled = true;

        // Let the XAML compositor commit the loaded solid-color islands before the parent captures the desktop.
        _ = _parent.Dispatcher.InvokeAsync(TimeSpan.FromMilliseconds(100), ReportCaptureReadySafely);
    }

    private void ArrangeAndVerifyZOrder()
    {
        _clippedHost.SetWindowPosition(WindowZOrder.Bottom, default, s_zOrderFlags);
        _hostUnderNative.SetWindowPosition(WindowZOrder.Top, default, s_zOrderFlags);
        _nativeUnder.SetWindowPosition(WindowZOrder.Top, default, s_zOrderFlags);
        _hostAboveNative.SetWindowPosition(WindowZOrder.Top, default, s_zOrderFlags);
        _ = _nativeAbove.SetFocus();
        _nativeAbove.SetWindowPosition(WindowZOrder.Top, default, s_zOrderFlags);

        Ensure(_viewport.GetRelatedWindow(WindowRelationship.Child) == _nativeAbove.Handle, "Native overlay was not topmost.");
        Ensure(_nativeAbove.GetRelatedWindow(WindowRelationship.Next) == _hostAboveNative.Handle, "Upper XAML island was not second in z-order.");
        Ensure(_hostAboveNative.GetRelatedWindow(WindowRelationship.Next) == _nativeUnder.Handle, "Native underlay was not below the upper XAML island.");
        Ensure(_nativeUnder.GetRelatedWindow(WindowRelationship.Next) == _hostUnderNative.Handle, "Lower XAML island was not below the native underlay.");
        Ensure(_hostUnderNative.GetRelatedWindow(WindowRelationship.Next) == _clippedHost.Handle, "Clipped XAML island was not bottommost.");
        Ensure(_clippedHost.GetRelatedWindow(WindowRelationship.Next).IsNull, "Unexpected sibling followed the clipped XAML island.");
        Ensure(PInvoke.GetFocus() == _nativeAbove.Handle, "NoActivate z-order changes moved keyboard focus.");
        Ensure(_clippedHost.GetWindowRectangle().Left < _viewport.GetWindowRectangle().Left, "Clipped host did not use a negative parent-client X coordinate.");
        _reporter.Write("airspace-zorder-verified", _parent.Handle);
    }

    private void ReportCaptureReady()
    {
        if (_disposed)
        {
            return;
        }

        Dictionary<string, AirspaceSample> samples = new()
        {
            ["nativeAbove"] = CreateSample(_nativeAbove, new Point(20, 20)),
            ["xamlAbove"] = CreateSample(_hostAboveNative, new Point(20, 20)),
            ["nativeUnderExposed"] = CreateSample(_nativeUnder, new Point(20, 20)),
            ["xamlUnderExposed"] = CreateSample(_hostUnderNative, new Point(20, 20)),
            ["clippedVisible"] = CreateSample(_clippedHost, new Point(120, 20)),
            ["clippedHidden"] = CreateSample(_clippedHost, new Point(70, 20))
        };

        _nativeUnder.UpdateWindow();
        _nativeAbove.UpdateWindow();
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

    private AirspaceSample CreateSample(NativeWindow window, Point clientPoint)
    {
        Point screenPoint = clientPoint;
        if (!window.ClientToScreen(ref screenPoint))
        {
            throw new InvalidOperationException("Could not map an airspace sample point to screen coordinates.");
        }

        Rectangle parentBounds = _parent.GetWindowRectangle();
        return new(screenPoint.X - parentBounds.X, screenPoint.Y - parentBounds.Y);
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
