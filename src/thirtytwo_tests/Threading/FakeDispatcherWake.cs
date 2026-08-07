// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

using Windows.Support;
using Windows.Win32.Foundation;

internal sealed class FakeDispatcherWake(Dispatcher dispatcher) : IDispatcherWake
{
    private int _pendingWakes;

    internal bool FailWakes { get; set; }

    internal int MaximumPendingWakes { get; private set; }

    internal int WakeCount { get; private set; }

    internal uint? DelayedWakeDelay { get; private set; }

    public void Wake()
    {
        if (FailWakes)
        {
            throw new ThirtyTwoException(WIN32_ERROR.ERROR_NOT_ENOUGH_QUOTA, "Expected wake failure.");
        }

        WakeCount++;
        _pendingWakes++;
        MaximumPendingWakes = Math.Max(MaximumPendingWakes, _pendingWakes);
    }

    public void WakeAfter(uint delayMilliseconds)
    {
        DelayedWakeDelay = delayMilliseconds;
    }

    public void CancelDelayedWake()
    {
        DelayedWakeDelay = null;
    }

    internal void DeliverOne()
    {
        if (_pendingWakes == 0)
        {
            throw new InvalidOperationException("No dispatcher wake is pending.");
        }

        _pendingWakes--;
        dispatcher.ProcessWake();
    }

    internal void DeliverDelayedWake()
    {
        if (DelayedWakeDelay is null)
        {
            throw new InvalidOperationException("No delayed dispatcher wake is pending.");
        }

        dispatcher.ProcessDelayedWake();
    }

    public void Dispose()
    {
    }
}
