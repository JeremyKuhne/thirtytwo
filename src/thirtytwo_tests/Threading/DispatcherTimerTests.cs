// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

using Windows.Win32;

[TestClass]
public class DispatcherTimerTests
{
    [TestMethod]
    public void Tick_MissedIntervals_SkipsToNextDeadline()
    {
        ManualTimeProvider timeProvider = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            timeProvider,
            dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        using DispatcherTimer timer = dispatcher.CreateTimer(TimeSpan.FromSeconds(10));
        int tickCount = 0;
        timer.Tick += (_, _) =>
        {
            tickCount++;
            if (tickCount == 1)
            {
                timeProvider.Advance(TimeSpan.FromSeconds(35));
            }
            else
            {
                timer.Stop();
            }
        };

        timer.Start();
        wake!.DeliverOne();
        wake.DelayedWakeDelay.Should().Be(10_000);

        timeProvider.Advance(TimeSpan.FromSeconds(10));
        wake.DeliverDelayedWake();
        wake.DeliverOne();
        wake.DelayedWakeDelay.Should().Be(5_000);
        tickCount.Should().Be(1);

        timeProvider.Advance(TimeSpan.FromSeconds(4));
        _ = dispatcher.InvokeAsync(static () => { });
        wake.DeliverOne();
        wake.DelayedWakeDelay.Should().Be(1_000);
        tickCount.Should().Be(1);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        wake.DeliverDelayedWake();
        tickCount.Should().Be(2);
        timer.IsRunning.Should().BeFalse();
    }

    [TestMethod]
    public void Tick_IntervalChangedInHandler_SchedulesOnceFromChange()
    {
        ManualTimeProvider timeProvider = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            timeProvider,
            dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        using DispatcherTimer timer = dispatcher.CreateTimer(TimeSpan.FromSeconds(10));
        int tickCount = 0;
        timer.Tick += (_, _) =>
        {
            tickCount++;
            timer.Interval = TimeSpan.FromSeconds(5);
        };

        timer.Start();
        wake!.DeliverOne();
        timeProvider.Advance(TimeSpan.FromSeconds(10));
        wake.DeliverDelayedWake();
        wake.DeliverOne();

        tickCount.Should().Be(1);
        wake.DelayedWakeDelay.Should().Be(5_000);
        timer.Stop();
    }

    [TestMethod]
    public void Tick_HandledException_RoutesToDispatcher()
    {
        ManualTimeProvider timeProvider = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            timeProvider,
            dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        using DispatcherTimer timer = dispatcher.CreateTimer(TimeSpan.FromSeconds(1));
        Exception? observed = null;
        dispatcher.UnhandledException += (_, arguments) =>
        {
            observed = arguments.Exception;
            arguments.Handled = true;
        };
        timer.Tick += (_, _) => throw new InvalidOperationException("Expected");

        timer.Start();
        wake!.DeliverOne();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        wake.DeliverDelayedWake();

        observed.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("Expected");
        timer.IsRunning.Should().BeFalse();
    }

    [STATestMethod]
    public void RunMessageLoop_RunningTimer_StopsDuringShutdown()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        DispatcherTimer? timer = null;
        Task<Task> queued = DispatcherTestWorker.Start(() => dispatcher.InvokeAsync(() =>
        {
            timer = dispatcher.CreateTimer(TimeSpan.FromHours(1));
            timer.Start();
            context.RequestExit();
        }));

        context.RunMessageLoop();
        queued.GetAwaiter().GetResult().GetAwaiter().GetResult();

        timer.Should().NotBeNull();
        timer!.IsRunning.Should().BeFalse();
        timer.Dispose();
    }
}
