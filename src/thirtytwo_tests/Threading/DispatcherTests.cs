// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Threading;

[TestClass]
public class DispatcherTests
{
    private const uint FairnessMessage = Interop.WM_APP + 101;

    [STATestMethod]
    public void InvokeAsync_Actions_ExecuteInFifoOrder()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        List<int> order = [];
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        uint callbackThreadId = 0;
        Dispatcher? callbackDispatcher = null;

        Task<Task[]> queued = QueueFromWorker(
            dispatcher,
            () => order.Add(1),
            () => order.Add(2),
            () =>
            {
                order.Add(3);
                callbackThreadId = PInvoke.GetCurrentThreadId();
                callbackDispatcher = Dispatcher.Current;
                PInvoke.PostQuitMessage(0);
            });

        context.RunMessageLoop();
    Task[] operations = queued.GetAwaiter().GetResult();
        Task.WhenAll(operations).GetAwaiter().GetResult();

        order.Should().Equal(1, 2, 3);
        callbackThreadId.Should().Be(dispatcherThreadId);
        callbackDispatcher.Should().BeSameAs(dispatcher);
        Dispatcher.Current.Should().BeNull();
    }

    [STATestMethod]
    public void InvokeAsync_ResultAndException_CompleteTasks()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        Task<(Task<int> Result, Task Fault)> queued = DispatcherTestWorker.Start(() =>
        {
            Task<int> result = dispatcher.InvokeAsync(() => 42);
            Task fault = dispatcher.InvokeAsync(() => throw new InvalidOperationException("Expected"));
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return (result, fault);
        });

        context.RunMessageLoop();

        (Task<int> result, Task fault) = queued.GetAwaiter().GetResult();
        result.GetAwaiter().GetResult().Should().Be(42);
        Action getFault = () => fault.GetAwaiter().GetResult();
        getFault.Should().Throw<InvalidOperationException>().WithMessage("Expected");
    }

    [STATestMethod]
    public void InvokeAsync_PreCanceled_DoesNotRunCallback()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        using CancellationTokenSource cancellationSource = new();
        cancellationSource.Cancel();
        bool callbackRan = false;
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            Task canceled = dispatcher.InvokeAsync(() => callbackRan = true, cancellationSource.Token);
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return canceled;
        });

        context.RunMessageLoop();

        Task canceled = queued.GetAwaiter().GetResult();
        Action getResult = () => canceled.GetAwaiter().GetResult();
        getResult.Should().Throw<OperationCanceledException>();
        callbackRan.Should().BeFalse();
    }

    [STATestMethod]
    public void InvokeAsync_FromDispatcherThread_QueuesCallback()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        bool nestedRan = false;
        bool nestedCompletedInline = true;
        Task? nestedOperation = null;

        Task<Task[]> queued = QueueFromWorker(
            dispatcher,
            () =>
            {
                nestedOperation = dispatcher.InvokeAsync(() =>
                {
                    nestedRan = true;
                    PInvoke.PostQuitMessage(0);
                });

                nestedCompletedInline = nestedOperation.IsCompleted;
            });

        context.RunMessageLoop();
    Task[] operations = queued.GetAwaiter().GetResult();
        Task.WhenAll(operations).GetAwaiter().GetResult();
        nestedOperation!.GetAwaiter().GetResult();

        nestedCompletedInline.Should().BeFalse();
        nestedRan.Should().BeTrue();
    }

    [STATestMethod]
    public void InvokeAsync_Callback_FlowsExecutionContextAndInstallsDispatcherContext()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        AsyncLocal<string?> contextValue = new();
        string? observedValue = null;
        SynchronizationContext? observedSynchronizationContext = null;
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            contextValue.Value = "Producer";
            return dispatcher.InvokeAsync(() =>
            {
                observedValue = contextValue.Value;
                observedSynchronizationContext = SynchronizationContext.Current;
                PInvoke.PostQuitMessage(0);
            });
        });

        context.RunMessageLoop();
        queued.GetAwaiter().GetResult().GetAwaiter().GetResult();

        observedValue.Should().Be("Producer");
        observedSynchronizationContext.Should().BeOfType<DispatcherSynchronizationContext>();
    }

    [STATestMethod]
    public void SynchronizationContext_Post_HandledExceptionContinuesPump()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        Exception? observedException = null;

        Task<Task[]> queued = QueueFromWorker(
            dispatcher,
            () =>
            {
                dispatcher.UnhandledException += (_, arguments) =>
                {
                    observedException = arguments.Exception;
                    arguments.Handled = true;
                };

                SynchronizationContext.Current!.Post(
                    static _ => throw new InvalidOperationException("Expected"),
                    null);

                SynchronizationContext.Current.Post(static _ => PInvoke.PostQuitMessage(0), null);
            });

        context.RunMessageLoop();
        Task.WhenAll(queued.GetAwaiter().GetResult()).GetAwaiter().GetResult();

        observedException.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("Expected");
    }

    [STATestMethod]
    public void InvokeAsync_AsyncCallback_RepresentsFullLifetimeAndResumesOnDispatcher()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        using ManualResetEventSlim interleavedQueued = new(initialState: false);
        Dispatcher dispatcher = context.Dispatcher;
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        uint resumedThreadId = 0;
        bool interleavedCallbackRan = false;
        Task<(Task<int> Async, Task Interleaved)> queued = DispatcherTestWorker.Start(() =>
        {
            Task<int> asyncOperation = dispatcher.InvokeAsync<int>(async _ =>
            {
                interleavedQueued.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                await Task.Yield();
                resumedThreadId = PInvoke.GetCurrentThreadId();
                interleavedCallbackRan.Should().BeTrue();
                return 42;
            });

            Task interleaved = dispatcher.InvokeAsync(() => interleavedCallbackRan = true);
            interleavedQueued.Set();
            asyncOperation.GetAwaiter().GetResult();
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return (asyncOperation, interleaved);
        });

        context.RunMessageLoop();

        (Task<int> asyncOperation, Task interleaved) = queued.GetAwaiter().GetResult();
        interleaved.GetAwaiter().GetResult();
        asyncOperation.GetAwaiter().GetResult().Should().Be(42);
        resumedThreadId.Should().Be(dispatcherThreadId);
    }

    [STATestMethod]
    public void InvokeAsync_ParameterlessAsyncLambda_RepresentsFullLifetime()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        bool resumed = false;
        Task<Task<int>> queued = DispatcherTestWorker.Start(() =>
        {
            Task<int> operation = dispatcher.InvokeAsync(async () =>
            {
                await Task.Yield();
                resumed = true;
                return 42;
            });

            operation.GetAwaiter().GetResult();
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return operation;
        });

        context.RunMessageLoop();

        queued.GetAwaiter().GetResult().GetAwaiter().GetResult().Should().Be(42);
        resumed.Should().BeTrue();
    }

    [STATestMethod]
    public void InvokeAsync_AsyncCallback_FinallyRunsBeforeTaskCompletes()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        bool finallyRan = false;
        bool finallyObservedAtCompletion = false;
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            Task operation = dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await Task.Yield();
                }
                finally
                {
                    finallyRan = true;
                }
            });

            operation.GetAwaiter().GetResult();
            finallyObservedAtCompletion = finallyRan;
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return operation;
        });

        context.RunMessageLoop();
        queued.GetAwaiter().GetResult().GetAwaiter().GetResult();

        finallyObservedAtCompletion.Should().BeTrue();
    }

    [STATestMethod]
    public void InvokeAsync_CanceledWhileQueued_DoesNotRunCallback()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        using CancellationTokenSource cancellationSource = new();
        bool callbackRan = false;
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            _ = dispatcher.InvokeAsync(cancellationSource.Cancel);
            Task canceled = dispatcher.InvokeAsync(() => callbackRan = true, cancellationSource.Token);
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return canceled;
        });

        context.RunMessageLoop();

        Task canceled = queued.GetAwaiter().GetResult();
        Action getResult = () => canceled.GetAwaiter().GetResult();
        getResult.Should().Throw<OperationCanceledException>();
        callbackRan.Should().BeFalse();
    }

    [STATestMethod]
    public void InvokeAsync_RunningAsyncCallback_CancellationIsCooperative()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        using CancellationTokenSource cancellationSource = new();
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            Task canceled = dispatcher.InvokeAsync(
                async cancellationToken =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                },
                cancellationSource.Token);

            _ = dispatcher.InvokeAsync(cancellationSource.Cancel);

            try
            {
                canceled.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            }

            return canceled;
        });

        context.RunMessageLoop();

        Task canceled = queued.GetAwaiter().GetResult();
        Action getResult = () => canceled.GetAwaiter().GetResult();
        getResult.Should().Throw<OperationCanceledException>();
    }

    [STATestMethod]
    public void InvokeAsync_RunningAsyncCallback_HandlesCancellationAndSucceeds()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        using CancellationTokenSource cancellationSource = new();
        Task<Task<int>> queued = DispatcherTestWorker.Start(() =>
        {
            Task<int> operation = dispatcher.InvokeAsync(
                async cancellationToken =>
                {
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    return 42;
                },
                cancellationSource.Token);

            _ = dispatcher.InvokeAsync(cancellationSource.Cancel);
            operation.GetAwaiter().GetResult();
            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return operation;
        });

        context.RunMessageLoop();

        queued.GetAwaiter().GetResult().GetAwaiter().GetResult().Should().Be(42);
    }

    [STATestMethod]
    public void RunMessageLoop_SuspendedAsyncCallback_ShutdownFaultsOperation()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        TaskCompletionSource releaseCallback = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource callbackFinished = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            Task operation = dispatcher.InvokeAsync(async _ =>
            {
                await releaseCallback.Task.ConfigureAwait(false);
                callbackFinished.SetResult();
            });

            _ = dispatcher.InvokeAsync(() => PInvoke.PostQuitMessage(0));
            return operation;
        });

        context.RunMessageLoop();

        Task operation = queued.GetAwaiter().GetResult();
        Action getResult = () => operation.GetAwaiter().GetResult();
        getResult.Should().Throw<ObjectDisposedException>().WithMessage("*Operation 1*");

        releaseCallback.SetResult();
        callbackFinished.Task.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
    }

    [STATestMethod]
    public void InvokeAsync_ProducerBurst_AllowsNativeMessageBetweenItems()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        using ManualResetEventSlim burstReady = new(initialState: false);
        Dispatcher dispatcher = context.Dispatcher;
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        int callbacksRun = 0;
        int callbacksAtNativeMessage = -1;
        using MessageFilterRegistration fairnessRegistration = Application.AddMessageFilter(
            new FairnessFilter(burstReady, () => callbacksAtNativeMessage = callbacksRun));
        Task<Task[]> queued = DispatcherTestWorker.Start(() =>
        {
            Task[] operations = Enumerable.Range(0, 100)
                .Select(index => dispatcher.InvokeAsync(() =>
                {
                    callbacksRun++;
                    if (index == 99)
                    {
                        PInvoke.PostQuitMessage(0);
                    }
                }))
                .ToArray();

            bool posted = PInvoke.PostThreadMessage(dispatcherThreadId, FairnessMessage, default, default);
            burstReady.Set();
            posted.Should().BeTrue();
            return operations;
        });

        context.RunMessageLoop();
        Task.WhenAll(queued.GetAwaiter().GetResult()).GetAwaiter().GetResult();

        callbacksRun.Should().Be(100);
        callbacksAtNativeMessage.Should().Be(1);
    }

    [STATestMethod]
    public void SynchronizationContext_Post_UnhandledException_ExitsPumpAndRethrows()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        Task<Task[]> queued = QueueFromWorker(dispatcher, () =>
            SynchronizationContext.Current!.Post(
                static _ => throw new InvalidOperationException("Expected"),
                null));

        Action run = context.RunMessageLoop;

        run.Should().Throw<InvalidOperationException>().WithMessage("Expected");
        Task.WhenAll(queued.GetAwaiter().GetResult()).GetAwaiter().GetResult();
    }

    [STATestMethod]
    public void InvokeAsync_SuppressedExecutionContext_DoesNotFlowAsyncLocal()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        AsyncLocal<string?> contextValue = new();
        string? observedValue = "NotRun";
        Task<Task> queued = DispatcherTestWorker.Start(() =>
        {
            contextValue.Value = "Producer";

            using (ExecutionContext.SuppressFlow())
            {
                return dispatcher.InvokeAsync(() =>
                {
                    observedValue = contextValue.Value;
                    PInvoke.PostQuitMessage(0);
                });
            }
        });

        context.RunMessageLoop();
        queued.GetAwaiter().GetResult().GetAwaiter().GetResult();

        observedValue.Should().BeNull();
    }

    private sealed class FairnessFilter(ManualResetEventSlim burstReady, Action callback) : IMessageFilter
    {
        private bool _firstWakeObserved;

        public bool PreFilterMessage(ref MSG message)
        {
            if (!_firstWakeObserved && message.message == Application.DispatcherWakeMessage)
            {
                _firstWakeObserved = true;
                burstReady.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
                return false;
            }

            if (message.message != FairnessMessage)
            {
                return false;
            }

            callback();
            return true;
        }
    }

    private static Task<Task[]> QueueFromWorker(Dispatcher dispatcher, params Action[] callbacks)
        => DispatcherTestWorker.Start(
            () => callbacks.Select(callback => dispatcher.InvokeAsync(callback)).ToArray());
}
