// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Threading;

[TestClass]
public class DispatcherWakeTests
{
    [ThreadStatic]
    private static bool t_processingWake;

    [TestMethod]
    public void InvokeAsync_MultipleItems_CoalescesOutstandingWake()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();

        Task[] operations = Enumerable.Range(0, 100)
            .Select(_ => dispatcher.InvokeAsync(static () => { }))
            .ToArray();

        wake.Should().NotBeNull();
        wake!.WakeCount.Should().Be(1);
        wake.MaximumPendingWakes.Should().Be(1);

        while (operations.Any(operation => !operation.IsCompleted))
        {
            wake.DeliverOne();
        }

        Task.WhenAll(operations).GetAwaiter().GetResult();
        wake.WakeCount.Should().Be(operations.Length);
        wake.MaximumPendingWakes.Should().Be(1);
    }

    [TestMethod]
    public void ProcessWake_DelayedWakePending_PreservesDelayedWake()
    {
        ManualTimeProvider timeProvider = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            timeProvider,
            dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        bool delayedRan = false;
        bool immediateRan = false;

        Task delayed = dispatcher.InvokeAsync(TimeSpan.FromSeconds(10), () => delayedRan = true);
        wake!.DeliverOne();
        wake.DelayedWakeDelay.Should().Be(10_000);

        Task immediate = dispatcher.InvokeAsync(() => immediateRan = true);
        wake.DelayedWakeDelay.Should().Be(10_000);
        wake.DeliverOne();

        immediate.GetAwaiter().GetResult();
        immediateRan.Should().BeTrue();
        delayedRan.Should().BeFalse();
        wake.DelayedWakeDelay.Should().Be(10_000);

        timeProvider.Advance(TimeSpan.FromSeconds(10));
        wake.DeliverDelayedWake();

        delayed.GetAwaiter().GetResult();
        delayedRan.Should().BeTrue();
        wake.DelayedWakeDelay.Should().BeNull();
    }

    [STATestMethod]
    public void ProcessQueue_RearmFailure_FaultsRemainingWork()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        using Window window = new(Window.DefaultBounds);
        dispatcher.Start();
        ThreadingTestAccessors.AttachDispatcher(window, dispatcher);
        window.Dispatcher.Should().BeSameAs(dispatcher);

        Task first = dispatcher.InvokeAsync(static () => { });
        Task second = dispatcher.InvokeAsync(static () => { });
        wake!.FailWakes = true;

        wake.DeliverOne();

        first.GetAwaiter().GetResult();
        Action getSecond = () => second.GetAwaiter().GetResult();
        getSecond.Should().Throw<ThirtyTwoException>().WithMessage("Expected wake failure.");
        Action throwFault = dispatcher.ThrowIfFaulted;
        throwFault.Should().Throw<ThirtyTwoException>().WithMessage("Expected wake failure.");
        window.Dispatcher.Should().BeSameAs(dispatcher);
        dispatcher.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        dispatcher.TryPost(static () => { }).Should().BeFalse();
        Dispatcher.FromHandle(window.Handle).Should().BeNull();

        _ = PInvoke.PeekMessage(
            out _,
            HWND.Null,
            Interop.WM_QUIT,
            Interop.WM_QUIT,
            PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);
    }

    [TestMethod]
    public void InvokeAsync_Completion_DoesNotInlineContinuationInWakeProcessor()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        bool continuationRanInsideWake = false;

        Task operation = dispatcher.InvokeAsync(static () => { });
        Task continuation = operation.ContinueWith(
            _ => continuationRanInsideWake = t_processingWake,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        t_processingWake = true;
        try
        {
            wake!.DeliverOne();
        }
        finally
        {
            t_processingWake = false;
        }

        continuation.GetAwaiter().GetResult();

        continuationRanInsideWake.Should().BeFalse();
    }

    [TestMethod]
    public void InvokeAsync_AfterStop_ReturnsFaultedTask()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        dispatcher.Stop();

        Task operation = dispatcher.InvokeAsync(static () => { });

        Action getResult = () => operation.GetAwaiter().GetResult();
        getResult.Should().Throw<ObjectDisposedException>().WithMessage("*Stopped*");
        dispatcher.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        dispatcher.TryPost(static () => { }).Should().BeFalse();
        wake.Should().NotBeNull();
    }

    [TestMethod]
    public void TryPost_RunningDispatcher_ExecutesCallback()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        bool callbackRan = false;

        bool accepted = dispatcher.TryPost(() => callbackRan = true);
        wake!.DeliverOne();

        accepted.Should().BeTrue();
        callbackRan.Should().BeTrue();
    }

    [TestMethod]
    public void BeginShutdown_CancelsTokenBeforeDisposeCompletesTask()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        CancellationTokenSource shutdownSource = ThreadingTestAccessors.GetShutdownSource(dispatcher);
        CancellationToken shutdownToken = dispatcher.ShutdownToken;
        Task completion = dispatcher.Completion;
        dispatcher.Start();

        dispatcher.BeginShutdown();

        shutdownToken.IsCancellationRequested.Should().BeTrue();
        dispatcher.TryPost(static () => { }).Should().BeFalse();
        completion.IsCompleted.Should().BeFalse();

        context.Dispose();

        dispatcher.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        Action getSourceToken = () => _ = shutdownSource.Token;
        getSourceToken.Should().Throw<ObjectDisposedException>();
        bool shutdownObserved = false;
        using CancellationTokenRegistration registration = dispatcher.ShutdownToken.Register(() => shutdownObserved = true);
        shutdownObserved.Should().BeTrue();
        completion.IsCompletedSuccessfully.Should().BeTrue();
    }

    [STATestMethod]
    public void Start_SecondDispatcherFailure_DoesNotUnregisterRunningDispatcher()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        Dispatcher second = ThreadingTestAccessors.CreateDispatcher(
            wakeFactory: candidate => new FakeDispatcherWake(candidate));
        using Window window = new(Window.DefaultBounds);
        dispatcher.Start();
        ThreadingTestAccessors.AttachDispatcher(window, dispatcher);

        try
        {
            Action start = second.Start;
            start.Should().Throw<InvalidOperationException>()
                .WithMessage("The current thread already has a running dispatcher.");
        }
        finally
        {
            second.Dispose();
        }

        window.Dispatcher.Should().BeSameAs(dispatcher);
    }

    [TestMethod]
    public void EventSource_Operation_EmitsLifecycleAndQueueEvents()
    {
        using DispatcherEventListener listener = new();
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        dispatcher.Start();
        Task operation = dispatcher.InvokeAsync(static () => { });

        wake!.DeliverOne();
        operation.GetAwaiter().GetResult();
        dispatcher.Stop();

        listener.EventIds.Should().Contain([1, 2, 3, 4, 7]);
    }

    [TestMethod]
    public void InvokeAsync_WorkerWakeFailure_FaultsTaskAndExitsPump()
    {
        FakeDispatcherWake? wake = null;
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext(
            wakeFactory: dispatcher => wake = new FakeDispatcherWake(dispatcher));
        Dispatcher dispatcher = context.Dispatcher;
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            wake!.FailWakes = true;
            return dispatcher.InvokeAsync(static () => { });
        });

        Action run = context.RunMessageLoop;

        run.Should().Throw<ThirtyTwoException>().WithMessage("Expected wake failure.");
        Task operation = queued.GetAwaiter().GetResult();
        Action getResult = () => operation.GetAwaiter().GetResult();
        getResult.Should().Throw<ThirtyTwoException>().WithMessage("Expected wake failure.");
    }
}
