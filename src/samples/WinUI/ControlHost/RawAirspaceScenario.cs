// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ControlHost;

internal sealed unsafe class RawAirspaceScenario : IDisposable
{
    private static readonly SET_WINDOW_POS_FLAGS s_zOrderFlags =
        SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;

    private readonly HWND _parent;
    private readonly ScenarioReporter _reporter;
    private readonly RawAirspaceScene _scene;
    private readonly FrameworkElement[] _islands;
    private DispatcherQueueTimer? _captureTimer;
    private int _loadedIslandCount;
    private bool _started;
    private bool _captureScheduled;
    private bool _disposed;

    internal RawAirspaceScenario(HWND parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;
        _scene = RawAirspaceScene.Create(parent);
        _islands = [_scene.HostUnderNative.Content, _scene.HostAboveNative.Content, _scene.ClippedHost.Content];

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
        if (_captureTimer is not null)
        {
            _captureTimer.Stop();
            _captureTimer.Tick -= CaptureTimerTick;
            _captureTimer = null;
        }

        foreach (FrameworkElement island in _islands)
        {
            island.Loaded -= IslandLoaded;
        }

        _scene.Dispose();
        _reporter.Write("airspace-disposed");
    }

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
        _captureTimer = _islands[0].DispatcherQueue.CreateTimer();
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

    private void ArrangeAndVerifyZOrder()
    {
        SetWindowPosition(_scene.ClippedHost.Handle, HWND.HWND_BOTTOM);
        SetWindowPosition(_scene.HostUnderNative.Handle, HWND.HWND_TOP);
        SetWindowPosition(_scene.NativeUnder, HWND.HWND_TOP);
        SetWindowPosition(_scene.HostAboveNative.Handle, HWND.HWND_TOP);
        _ = PInvoke.SetFocus(_scene.NativeAbove);
        SetWindowPosition(_scene.NativeAbove, HWND.HWND_TOP);

        Ensure(PInvoke.GetWindow(_scene.Viewport, GET_WINDOW_CMD.GW_CHILD) == _scene.NativeAbove, "Native overlay was not topmost.");
        Ensure(PInvoke.GetWindow(_scene.NativeAbove, GET_WINDOW_CMD.GW_HWNDNEXT) == _scene.HostAboveNative.Handle, "Upper XAML island was not second in z-order.");
        Ensure(PInvoke.GetWindow(_scene.HostAboveNative.Handle, GET_WINDOW_CMD.GW_HWNDNEXT) == _scene.NativeUnder, "Native underlay was not below the upper XAML island.");
        Ensure(PInvoke.GetWindow(_scene.NativeUnder, GET_WINDOW_CMD.GW_HWNDNEXT) == _scene.HostUnderNative.Handle, "Lower XAML island was not below the native underlay.");
        Ensure(PInvoke.GetWindow(_scene.HostUnderNative.Handle, GET_WINDOW_CMD.GW_HWNDNEXT) == _scene.ClippedHost.Handle, "Clipped XAML island was not bottommost.");
        Ensure(PInvoke.GetWindow(_scene.ClippedHost.Handle, GET_WINDOW_CMD.GW_HWNDNEXT).IsNull, "Unexpected sibling followed the clipped XAML island.");
        Ensure(PInvoke.GetFocus() == _scene.NativeAbove, "No-activate z-order changes moved keyboard focus.");

        RECT clippedBounds;
        RECT viewportBounds;
        if (!PInvoke.GetWindowRect(_scene.ClippedHost.Handle, &clippedBounds)
            || !PInvoke.GetWindowRect(_scene.Viewport, &viewportBounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        Ensure(clippedBounds.left < viewportBounds.left, "Clipped host did not use a negative parent-client X coordinate.");
        _reporter.Write("airspace-zorder-verified", _parent);
    }

    private void ReportCaptureReady()
    {
        if (_disposed)
        {
            return;
        }

        Dictionary<string, AirspaceSample> samples = new()
        {
            ["nativeAbove"] = CreateSample(_scene.NativeAbove, new Point(20, 20)),
            ["xamlAbove"] = CreateSample(_scene.HostAboveNative.Handle, new Point(20, 20)),
            ["nativeUnderExposed"] = CreateSample(_scene.NativeUnder, new Point(20, 20)),
            ["xamlUnderExposed"] = CreateSample(_scene.HostUnderNative.Handle, new Point(20, 20)),
            ["clippedVisible"] = CreateSample(_scene.ClippedHost.Handle, new Point(120, 20)),
            ["clippedHidden"] = CreateSample(_scene.ClippedHost.Handle, new Point(70, 20))
        };

        UpdateWindow(_scene.NativeUnder);
        UpdateWindow(_scene.NativeAbove);
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

    private AirspaceSample CreateSample(HWND window, Point clientPoint)
    {
        Point screenPoint = clientPoint;
        if (!PInvoke.ClientToScreen(window, &screenPoint))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        RECT parentBounds;
        if (!PInvoke.GetWindowRect(_parent, &parentBounds))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }

        return new(screenPoint.X - parentBounds.left, screenPoint.Y - parentBounds.top);
    }

    private static void SetWindowPosition(HWND window, HWND insertAfter)
    {
        if (!PInvoke.SetWindowPos(window, insertAfter, 0, 0, 0, 0, s_zOrderFlags))
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
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