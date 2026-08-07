// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows.Threading;

/// <summary>
///  Provides a message-only window that wakes the dispatcher queue and timer.
/// </summary>
internal sealed unsafe class DispatcherWakeWindow : WindowClass, IDispatcherWake
{
    private const nuint TimerId = 1;

    private Dispatcher? _dispatcher;

    // Message-only HWND that receives immediate and delayed wake signals.
    private HWND _handle;
    private bool _timerArmed;

    /// <summary>
    ///  Initializes a wake window for the dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to wake.</param>
    internal DispatcherWakeWindow(Dispatcher dispatcher)
        : base(
            className: $"ThirtyTwoDispatcherWindow_{Guid.NewGuid()}",
            classStyle: 0,
            backgroundBrush: HBRUSH.Invalid,
            icon: HICON.Invalid,
            cursor: HCURSOR.Invalid)
    {
        _dispatcher = dispatcher;
        _handle = CreateWindow(
            bounds: new Rectangle(0, 0, 1, 1),
            style: WindowStyles.Overlapped,
            parentWindow: HWND.HWND_MESSAGE);
    }

    /// <inheritdoc/>
    public void Wake()
    {
        PInvoke.PostMessage(_handle, Application.DispatcherWakeMessage, default, default).ThrowLastErrorIfFalse();
    }

    /// <inheritdoc/>
    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if ((uint)message == Application.DispatcherWakeMessage)
        {
            _dispatcher?.ProcessWake();
            return (LRESULT)0;
        }

        if (message == MessageType.Timer && (nuint)wParam == TimerId)
        {
            _dispatcher?.ProcessDelayedWake();
            return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    /// <inheritdoc/>
    public void WakeAfter(uint delayMilliseconds)
    {
        nuint result = PInvoke.SetCoalescableTimer(_handle, TimerId, delayMilliseconds, null, 0);
        if (result == 0)
        {
            Error.GetLastError().ThrowThirtyTwoException();
        }

        _timerArmed = true;
    }

    /// <inheritdoc/>
    public void CancelDelayedWake()
    {
        if (!_timerArmed)
        {
            return;
        }

        if (!PInvoke.KillTimer(_handle, TimerId))
        {
            WIN32_ERROR.NO_ERROR.ThrowIfLastErrorNot();
        }

        _timerArmed = false;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_handle.IsNull)
        {
            CancelDelayedWake();
            PInvoke.DestroyWindow(_handle).ThrowLastErrorIfFalse();
            _handle = HWND.Null;
        }

        _dispatcher = null;
        base.Dispose(disposing);
    }
}
