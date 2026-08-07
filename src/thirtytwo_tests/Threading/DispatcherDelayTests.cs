// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;

namespace Windows.Threading;

[TestClass]
public class DispatcherDelayTests
{
    [STATestMethod]
    public void InvokeAsync_DelayedAction_WaitsForMonotonicDueTime()
    {
        ManualTimeProvider timeProvider = new();
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(timeProvider);
        Dispatcher dispatcher = context.Dispatcher;
        bool callbackRan = false;
        bool incompleteBeforeDue = false;
        TaskCompletionSource<Task> queued = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task worker = DispatcherTestWorker.Start(() =>
        {
            Task delayed = dispatcher.InvokeAsync(TimeSpan.FromSeconds(10), () =>
            {
                callbackRan = true;
                PInvoke.PostQuitMessage(0);
            });

            _ = dispatcher.InvokeAsync(() =>
            {
                incompleteBeforeDue = !delayed.IsCompleted && !callbackRan;
                timeProvider.Advance(TimeSpan.FromSeconds(10));
                _ = dispatcher.InvokeAsync(static () => { });
            });

            queued.SetResult(delayed);
        });

        context.RunMessageLoop();
        worker.GetAwaiter().GetResult();

        queued.Task.GetAwaiter().GetResult().GetAwaiter().GetResult();
        incompleteBeforeDue.Should().BeTrue();
        callbackRan.Should().BeTrue();
    }

    [STATestMethod]
    public void InvokeAsync_EqualDueTimes_PreserveAdmissionOrder()
    {
        ManualTimeProvider timeProvider = new();
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(timeProvider);
        Dispatcher dispatcher = context.Dispatcher;
        List<int> order = [];
        TaskCompletionSource<Task[]> queued = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task worker = DispatcherTestWorker.Start(() =>
        {
            Task first = dispatcher.InvokeAsync(TimeSpan.FromSeconds(10), () => order.Add(1));
            Task second = dispatcher.InvokeAsync(TimeSpan.FromSeconds(10), () =>
            {
                order.Add(2);
                PInvoke.PostQuitMessage(0);
            });

            _ = dispatcher.InvokeAsync(() =>
            {
                timeProvider.Advance(TimeSpan.FromSeconds(10));
                _ = dispatcher.InvokeAsync(static () => { });
            });

            queued.SetResult([first, second]);
        });

        context.RunMessageLoop();
        worker.GetAwaiter().GetResult();

        Task.WhenAll(queued.Task.GetAwaiter().GetResult()).GetAwaiter().GetResult();
        order.Should().Equal(1, 2);
    }

    [STATestMethod]
    public void InvokeAsync_DelayedActionCanceled_ReleasesWithoutRunning()
    {
        ManualTimeProvider timeProvider = new();
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(timeProvider);
        Dispatcher dispatcher = context.Dispatcher;
        using CancellationTokenSource cancellationSource = new();
        bool callbackRan = false;
        TaskCompletionSource<Task> queued = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task worker = DispatcherTestWorker.Start(() =>
        {
            Task delayed = dispatcher.InvokeAsync(
                TimeSpan.FromHours(1),
                () => callbackRan = true,
                cancellationSource.Token);

            _ = dispatcher.InvokeAsync(() =>
            {
                cancellationSource.Cancel();
                PInvoke.PostQuitMessage(0);
            });

            queued.SetResult(delayed);
        });

        context.RunMessageLoop();
        worker.GetAwaiter().GetResult();

        Task delayed = queued.Task.GetAwaiter().GetResult();
        Action getResult = () => delayed.GetAwaiter().GetResult();
        getResult.Should().Throw<OperationCanceledException>();
        callbackRan.Should().BeFalse();
    }

    [STATestMethod]
    public void InvokeAsync_DelayedFunc_ReturnsResult()
    {
        ManualTimeProvider timeProvider = new();
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(timeProvider);
        Dispatcher dispatcher = context.Dispatcher;
        TaskCompletionSource<Task<int>> queued = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task worker = DispatcherTestWorker.Start(() =>
        {
            Task<int> delayed = dispatcher.InvokeAsync(TimeSpan.FromSeconds(1), () => 42);
            _ = dispatcher.InvokeAsync(() =>
            {
                timeProvider.Advance(TimeSpan.FromSeconds(1));
                _ = dispatcher.InvokeAsync(static () => { });
            });

            queued.SetResult(delayed);
            delayed.GetAwaiter().GetResult();
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
        });

        context.RunMessageLoop();
        worker.GetAwaiter().GetResult();

        queued.Task.GetAwaiter().GetResult().GetAwaiter().GetResult().Should().Be(42);
    }

    [TestMethod]
    public void InvokeAsync_NegativeDelay_Throws()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();

        Action action = () => context.Dispatcher.InvokeAsync(TimeSpan.FromTicks(-1), static () => { });

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void InvokeAsync_MaximumDelay_ClampsDelayedWake()
    {
        ManualTimeProvider timeProvider = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            timeProvider,
            dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();

        Task operation = dispatcher.InvokeAsync(TimeSpan.MaxValue, static () => { });
        wake!.DeliverOne();

        operation.IsCompleted.Should().BeFalse();
        wake.DelayedWakeDelay.Should().Be(uint.MaxValue);
    }
}
