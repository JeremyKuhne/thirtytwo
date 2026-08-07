// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Repeats on a dispatcher using monotonic deadlines. Missed intervals are skipped rather than replayed.
/// </summary>
public sealed class DispatcherTimer : IDisposable
{
    private readonly Dispatcher _dispatcher;
    private readonly ShutdownRegistration _shutdownRegistration;
    private CancellationTokenSource? _scheduledCancellation;
    private TimeSpan _interval;
    private long _nextDueTicks;
    private bool _disposed;

    /// <summary>
    ///  Initializes a timer owned by a dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher on which ticks run.</param>
    /// <param name="interval">The initial timer interval.</param>
    internal DispatcherTimer(Dispatcher dispatcher, TimeSpan interval)
    {
        _dispatcher = dispatcher;
        Interval = interval;

        // Thread-context shutdown callbacks run before Dispatcher.Stop rejects and detaches delayed work.
        if (ThreadContext.CurrentContext is { } context
            && ReferenceEquals(context.Dispatcher, dispatcher))
        {
            _shutdownRegistration = context.RegisterShutdownCallback(Stop);
        }
    }

    /// <summary>
    ///  Occurs on the dispatcher thread when the interval elapses.
    /// </summary>
    public event EventHandler? Tick;

    /// <summary>
    ///  Gets or sets the positive interval. Changing it while running restarts the deadline from the current time.
    /// </summary>
    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            _dispatcher.VerifyAccess();
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _interval = value;
            if (IsRunning)
            {
                try
                {
                    ScheduleFromNow();
                }
                catch
                {
                    IsRunning = false;
                    throw;
                }
            }
        }
    }

    /// <summary>
    ///  Gets whether the timer is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    ///  Starts the timer. Must be called by the dispatcher thread.
    /// </summary>
    public void Start()
    {
        _dispatcher.VerifyAccess();
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        try
        {
            ScheduleFromNow();
        }
        catch
        {
            IsRunning = false;
            throw;
        }
    }

    /// <summary>
    ///  Stops the timer. Calling this while stopped has no effect. Must be called by the dispatcher thread.
    /// </summary>
    public void Stop()
    {
        _dispatcher.VerifyAccess();

        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        CancelScheduledTick();
    }

    private void ScheduleFromNow()
    {
        CancelScheduledTick();
        _nextDueTicks = AddSaturating(_dispatcher.MonotonicTicks, _interval.Ticks);
        ScheduleNextTick();
    }

    private void ScheduleNextTick()
    {
        long currentTicks = _dispatcher.MonotonicTicks;
        if (_nextDueTicks <= currentTicks)
        {
            // Advance from the prior deadline so handler duration does not accumulate drift. Missed ticks are skipped.
            long missedIntervals = ((currentTicks - _nextDueTicks) / _interval.Ticks) + 1;
            _nextDueTicks = AddSaturating(_nextDueTicks, MultiplySaturating(_interval.Ticks, missedIntervals));
        }

        TimeSpan delay = TimeSpan.FromTicks(_nextDueTicks - currentTicks);
        CancellationTokenSource cancellationSource = new();
        _scheduledCancellation = cancellationSource;

        try
        {
            _dispatcher.PostDelayed(delay, OnTick, cancellationSource.Token);
        }
        catch
        {
            _scheduledCancellation = null;
            cancellationSource.Dispose();
            throw;
        }
    }

    private void OnTick()
    {
        _scheduledCancellation?.Dispose();
        _scheduledCancellation = null;

        if (!IsRunning)
        {
            return;
        }

        try
        {
            Tick?.Invoke(this, EventArgs.Empty);
            if (!IsRunning)
            {
                return;
            }

            if (_scheduledCancellation is not null)
            {
                // The handler changed Interval and already scheduled from the new current time.
                return;
            }

            _nextDueTicks = AddSaturating(_nextDueTicks, _interval.Ticks);
            ScheduleNextTick();
        }
        catch
        {
            IsRunning = false;
            CancelScheduledTick();
            throw;
        }
    }

    private void CancelScheduledTick()
    {
        _scheduledCancellation?.Cancel();
        _scheduledCancellation?.Dispose();
        _scheduledCancellation = null;
    }

    private static long AddSaturating(long left, long right)
        => right > long.MaxValue - left ? long.MaxValue : left + right;

    private static long MultiplySaturating(long left, long right)
    {
        Int128 result = (Int128)left * right;
        return result > long.MaxValue ? long.MaxValue : (long)result;
    }

    /// <summary>
    ///  Stops and releases the timer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _dispatcher.VerifyAccess();
        Stop();
        _shutdownRegistration.Dispose();
        _disposed = true;
    }
}
