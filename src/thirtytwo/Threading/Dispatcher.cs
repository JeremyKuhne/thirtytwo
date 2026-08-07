// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using Windows.Support;

namespace Windows.Threading;

/// <summary>
///  Queues work to the UI thread owned by an active <see cref="Application.Run(Window, bool)"/> message loop.
/// </summary>
/// <remarks>
///  <para>
///   Admission has no capacity limit or backpressure. Sustained producers can grow the queue until the dispatcher
///   catches up; queue depth and its high-water mark are available through the dispatcher event source.
///  </para>
/// </remarks>
public sealed class Dispatcher
{
    // Raw HWND discovery maps the window's native owner thread to the dispatcher that currently accepts work.
    private static readonly ConcurrentDictionary<uint, Dispatcher> s_dispatchersByThreadId = [];

    // Protects lifecycle state, registry ownership, queue membership, wake reservation, and operation counters.
    private readonly Lock _lock = new();

    // A callback that returned an incomplete ValueTask is no longer in _queue. Track its work item until the callback
    // finishes so Stop or Fault can fault the operation's public Task if teardown happens first.
    private readonly HashSet<DispatcherWorkItem> _activeAsyncWork = [];

    // Holds immediate work and delayed work whose deadlines have elapsed; canceled entries are skipped when dequeued.
    private readonly Queue<DispatcherWorkItem> _queue = [];

    // Orders delayed work by deadline, then operation ID to preserve admission order for equal deadlines.
    private readonly PriorityQueue<DispatcherWorkItem, (long DueTicks, long Id)> _scheduledWork = new();

    // Cancels the effective tokens supplied to asynchronous callbacks when the dispatcher starts stopping or faults.
    private readonly CancellationTokenSource _shutdownSource = new();

    // Remains usable after shutdown without accessing the retained source through CancellationTokenSource.Token.
    private readonly CancellationToken _shutdownToken;

    // Completes asynchronously after dispatcher-owned native wake resources have been released.
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Captures managed thread identity for CheckAccess and VerifyAccess.
    private readonly Thread _thread = Thread.CurrentThread;

    // Supplies monotonic elapsed time for delayed work and dispatcher timers.
    private readonly TimeProvider _timeProvider;

    // Anchors TimeProvider elapsed-time calculations to dispatcher construction.
    private readonly long _timestampOrigin;

    // Keys HWND discovery and identifies the owner in diagnostics and cross-thread fault signaling.
    private readonly uint _nativeThreadId = PInvoke.GetCurrentThreadId();

    // Delivers immediate and delayed queue-processing signals.
    private readonly IDispatcherWake _wake;

    // All reads and writes are protected by _lock.
    private DispatcherState _state = DispatcherState.Created;

    // Set before posting a queue wake and cleared when ProcessQueue begins handling it. While set, additional
    // enqueues rely on that outstanding wake instead of posting another message.
    private bool _wakeRequested;

    // Tracks ownership of this dispatcher's native-thread registry entry so it is removed exactly once.
    private bool _registered;

    // Records the largest combined immediate, delayed, and active-async operation count observed at admission.
    private int _highWatermark;

    // Counts operations that reached a terminal state.
    private long _completedCount;

    // Generates operation IDs unique within this dispatcher lifetime.
    private long _nextOperationId;

    // Counts operations admitted to either the immediate queue or delayed heap.
    private long _postedCount;

    // Stores the current fatal failure for rethrow at a managed message-loop boundary. Later cancellation or
    // fault-signaling errors are combined with the existing exception in an AggregateException.
    private ExceptionDispatchInfo? _fault;

    /// <summary>
    ///  Initializes a dispatcher owned by the current thread.
    /// </summary>
    internal Dispatcher()
    {
        _timeProvider = TimeProvider.System;
        _timestampOrigin = _timeProvider.GetTimestamp();
        _shutdownToken = _shutdownSource.Token;
        SynchronizationContext = new DispatcherSynchronizationContext(this);

        try
        {
            _wake = new DispatcherWakeWindow(this);
        }
        catch
        {
            _shutdownSource.Dispose();
            throw;
        }
    }

    /// <summary>
    ///  Gets the dispatcher for the running message loop on the current thread, or <see langword="null"/>.
    /// </summary>
    public static Dispatcher? Current => ThreadContext.CurrentDispatcher;

    /// <summary>
    ///  Gets the active dispatcher for the thread that owns the given window handle.
    /// </summary>
    /// <typeparam name="T">A type that exposes an <see cref="HWND"/>.</typeparam>
    /// <param name="handle">The window handle or wrapper to inspect.</param>
    /// <returns>
    ///  The owning thread's active dispatcher, or <see langword="null"/> when the handle is null or invalid, or its
    ///  thread does not have a running dispatcher.
    /// </returns>
    /// <remarks>
    ///  <para>
    ///   This method does not create a dispatcher and may be called from any thread. The result is a snapshot: shutdown
    ///   can begin after this method returns, in which case a subsequent operation is rejected through its task.
    ///  </para>
    /// </remarks>
    public static unsafe Dispatcher? FromHandle<T>(T handle)
        where T : IHandle<HWND>
    {
        if (handle is null)
        {
            return null;
        }

        HWND windowHandle = handle.Handle;
        if (windowHandle.IsNull)
        {
            return null;
        }

        uint nativeThreadId = PInvoke.GetWindowThreadProcessId(windowHandle, null);
        return nativeThreadId != 0
            && s_dispatchersByThreadId.TryGetValue(nativeThreadId, out Dispatcher? dispatcher)
                ? dispatcher
                : null;
    }

    /// <summary>
    ///  Occurs when fire-and-forget work throws. Unhandled exceptions terminate the message loop.
    /// </summary>
    public event EventHandler<DispatcherUnhandledExceptionEventArgs>? UnhandledException;

    /// <summary>
    ///  Gets a token that is canceled when the dispatcher stops accepting work or faults.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The token lets producers stop proactively. Cancellation is advisory; queue admission remains authoritative
    ///   because shutdown can race a token check.
    ///  </para>
    /// </remarks>
    public CancellationToken ShutdownToken => _shutdownToken;

    /// <summary>
    ///  Gets a task that completes after dispatcher shutdown releases its native wake resources.
    /// </summary>
    public Task Completion => _completion.Task;

    /// <summary>
    ///  Gets the synchronization context installed while this dispatcher runs.
    /// </summary>
    internal DispatcherSynchronizationContext SynchronizationContext { get; }

    /// <summary>
    ///  Gets elapsed monotonic time since this dispatcher was constructed, in <see cref="TimeSpan"/> ticks.
    /// </summary>
    internal long MonotonicTicks => GetMonotonicTicks();

    /// <summary>
    ///  Returns whether the calling thread owns this dispatcher.
    /// </summary>
    public bool CheckAccess() => ReferenceEquals(Thread.CurrentThread, _thread);

    /// <summary>
    ///  Throws when the calling thread does not own this dispatcher.
    /// </summary>
    public void VerifyAccess()
    {
        if (!CheckAccess())
        {
            throw new InvalidOperationException("The calling thread does not own this dispatcher.");
        }
    }

    /// <summary>
    ///  Creates a repeating timer whose ticks run on this dispatcher. Must be called by the owning thread.
    /// </summary>
    public DispatcherTimer CreateTimer(TimeSpan interval)
    {
        VerifyAccess();
        return new DispatcherTimer(this, interval);
    }

    /// <summary>
    ///  Queues a synchronous callback. The callback is deferred even when called by the owning thread.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Cancellation prevents a queued callback from starting but does not interrupt one already running.
    ///  </para>
    /// </remarks>
    public Task InvokeAsync(Action callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return Enqueue(callback, fireAndForget: false, cancellationToken);
    }

    /// <summary>
    ///  Attempts to queue a fire-and-forget callback.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <returns><see langword="true"/> when the callback was admitted; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///  <para>
    ///   Admission and shutdown are synchronized, so the result is not a state pre-check. An admitted callback can
    ///   still be discarded if shutdown begins before it runs. Callback exceptions use <see cref="UnhandledException"/>.
    ///  </para>
    /// </remarks>
    public bool TryPost(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        DispatcherActionWorkItem workItem;
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                return false;
            }

            workItem = new DispatcherActionWorkItem(
                this,
                ++_nextOperationId,
                callback,
                CancellationToken.None,
                fireAndForget: true)
            {
                State = DispatcherWorkItemState.Queued
            };

            _queue.Enqueue(workItem);
            CaptureAdmissionMetricsLocked(workItem);
        }

        PublishAdmission(workItem);
        return true;
    }

    /// <summary>
    ///  Queues a synchronous callback for deferred execution.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="fireAndForget">
    ///  <see langword="true"/> to report callback exceptions through <see cref="UnhandledException"/> instead of
    ///  storing them on the returned task; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="cancellationToken">The token that can prevent the callback from starting.</param>
    /// <returns>A task that tracks the queued work.</returns>
    private Task Enqueue(Action callback, bool fireAndForget, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        DispatcherActionWorkItem workItem;
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                return Task.FromException(CreateUnavailableException(_state));
            }

            workItem = new DispatcherActionWorkItem(
                this,
                ++_nextOperationId,
                callback,
                cancellationToken,
                fireAndForget)
            {
                State = DispatcherWorkItemState.Queued
            };

            _queue.Enqueue(workItem);
            CaptureAdmissionMetricsLocked(workItem);
        }

        PublishAdmission(workItem);
        return workItem.Task;
    }

    /// <summary>
    ///  Queues a fire-and-forget <see cref="SynchronizationContext"/> callback for deferred execution.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    internal void Post(SendOrPostCallback callback, object? state)
    {
        Task task = Enqueue(() => callback(state), fireAndForget: true, CancellationToken.None);
        if (task.IsFaulted)
        {
            task.GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///  Queues a fire-and-forget callback after a monotonic delay.
    /// </summary>
    /// <param name="delay">The delay before the callback becomes eligible to run.</param>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="cancellationToken">The token that can prevent the callback from starting.</param>
    internal void PostDelayed(TimeSpan delay, Action callback, CancellationToken cancellationToken)
    {
        if (!TrySchedule(
            delay,
            id => new DispatcherActionWorkItem(
                this,
                id,
                callback,
                cancellationToken,
                fireAndForget: true),
            out DispatcherActionWorkItem? workItem,
            out ObjectDisposedException? exception))
        {
            throw exception;
        }

        PublishAdmission(workItem);
    }

    /// <summary>
    ///  Queues a synchronous function and returns its result.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }

        DispatcherFuncWorkItem<TResult> workItem;
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                return Task.FromException<TResult>(CreateUnavailableException(_state));
            }

            workItem = new DispatcherFuncWorkItem<TResult>(this, ++_nextOperationId, callback, cancellationToken)
            {
                State = DispatcherWorkItemState.Queued
            };

            _queue.Enqueue(workItem);
            CaptureAdmissionMetricsLocked(workItem);
        }

        PublishAdmission(workItem);
        return workItem.GetTask();
    }

    /// <summary>
    ///  Queues an asynchronous callback and completes after its full <see cref="ValueTask"/> lifetime.
    /// </summary>
    public Task InvokeAsync(Func<ValueTask> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return InvokeAsync(_ => callback(), cancellationToken);
    }

    /// <summary>
    ///  Queues an asynchronous function and completes after its full <see cref="ValueTask{TResult}"/> lifetime.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(
        Func<ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return InvokeAsync(_ => callback(), cancellationToken);
    }

    /// <summary>
    ///  Queues an asynchronous callback with an effective token linked to caller cancellation and dispatcher shutdown.
    /// </summary>
    public Task InvokeAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        DispatcherAsyncActionWorkItem workItem;
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                return Task.FromException(CreateUnavailableException(_state));
            }

            workItem = new DispatcherAsyncActionWorkItem(
                this,
                ++_nextOperationId,
                callback,
                cancellationToken)
            {
                State = DispatcherWorkItemState.Queued
            };

            _queue.Enqueue(workItem);
            CaptureAdmissionMetricsLocked(workItem);
        }

        PublishAdmission(workItem);
        return workItem.Task;
    }

    /// <summary>
    ///  Queues an asynchronous function with an effective token linked to caller cancellation and dispatcher shutdown.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }

        DispatcherAsyncFuncWorkItem<TResult> workItem;
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                return Task.FromException<TResult>(CreateUnavailableException(_state));
            }

            workItem = new DispatcherAsyncFuncWorkItem<TResult>(
                this,
                ++_nextOperationId,
                callback,
                cancellationToken)
            {
                State = DispatcherWorkItemState.Queued
            };

            _queue.Enqueue(workItem);
            CaptureAdmissionMetricsLocked(workItem);
        }

        PublishAdmission(workItem);
        return workItem.GetTask();
    }

    /// <summary>
    ///  Queues a synchronous callback after a monotonic delay.
    /// </summary>
    public Task InvokeAsync(
        TimeSpan delay,
        Action callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ValidateDelay(delay);

        if (delay == TimeSpan.Zero)
        {
            return InvokeAsync(callback, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (!TrySchedule(
            delay,
            id => new DispatcherActionWorkItem(this, id, callback, cancellationToken),
            out DispatcherActionWorkItem? workItem,
            out ObjectDisposedException? exception))
        {
            return Task.FromException(exception);
        }

        PublishAdmission(workItem);
        return workItem.Task;
    }

    /// <summary>
    ///  Queues a synchronous function after a monotonic delay.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(
        TimeSpan delay,
        Func<TResult> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ValidateDelay(delay);

        if (delay == TimeSpan.Zero)
        {
            return InvokeAsync(callback, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }

        if (!TrySchedule(
            delay,
            id => new DispatcherFuncWorkItem<TResult>(this, id, callback, cancellationToken),
            out DispatcherFuncWorkItem<TResult>? workItem,
            out ObjectDisposedException? exception))
        {
            return Task.FromException<TResult>(exception);
        }

        PublishAdmission(workItem);
        return workItem.GetTask();
    }

    /// <summary>
    ///  Queues an asynchronous callback after a monotonic delay.
    /// </summary>
    public Task InvokeAsync(
        TimeSpan delay,
        Func<ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return InvokeAsync(delay, _ => callback(), cancellationToken);
    }

    /// <summary>
    ///  Queues an asynchronous function after a monotonic delay.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(
        TimeSpan delay,
        Func<ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return InvokeAsync(delay, _ => callback(), cancellationToken);
    }

    /// <summary>
    ///  Queues an asynchronous callback with an effective cancellation token after a monotonic delay.
    /// </summary>
    public Task InvokeAsync(
        TimeSpan delay,
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ValidateDelay(delay);

        if (delay == TimeSpan.Zero)
        {
            return InvokeAsync(callback, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (!TrySchedule(
            delay,
            id => new DispatcherAsyncActionWorkItem(this, id, callback, cancellationToken),
            out DispatcherAsyncActionWorkItem? workItem,
            out ObjectDisposedException? exception))
        {
            return Task.FromException(exception);
        }

        PublishAdmission(workItem);
        return workItem.Task;
    }

    /// <summary>
    ///  Queues an asynchronous function with an effective cancellation token after a monotonic delay.
    /// </summary>
    public Task<TResult> InvokeAsync<TResult>(
        TimeSpan delay,
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ValidateDelay(delay);

        if (delay == TimeSpan.Zero)
        {
            return InvokeAsync(callback, cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }

        if (!TrySchedule(
            delay,
            id => new DispatcherAsyncFuncWorkItem<TResult>(this, id, callback, cancellationToken),
            out DispatcherAsyncFuncWorkItem<TResult>? workItem,
            out ObjectDisposedException? exception))
        {
            return Task.FromException<TResult>(exception);
        }

        PublishAdmission(workItem);
        return workItem.GetTask();
    }

    /// <summary>
    ///  Transitions the dispatcher from created to running.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///  The calling thread does not own the dispatcher, or the dispatcher is not in the created state.
    /// </exception>
    internal void Start()
    {
        VerifyAccess();

        lock (_lock)
        {
            if (_state != DispatcherState.Created)
            {
                throw new InvalidOperationException($"The dispatcher cannot start from state '{_state}'.");
            }

            if (!s_dispatchersByThreadId.TryAdd(_nativeThreadId, this))
            {
                throw new InvalidOperationException("The current thread already has a running dispatcher.");
            }

            _registered = true;
            _state = DispatcherState.Running;
        }

        DispatcherEventSource.Log.Started((int)_nativeThreadId);
    }

    /// <summary>
    ///  Stops accepting work by transitioning a running dispatcher to stopping.
    /// </summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own the dispatcher.</exception>
    internal void BeginShutdown()
    {
        VerifyAccess();

        bool unregister = false;
        bool cancel = false;
        lock (_lock)
        {
            if (_state == DispatcherState.Running)
            {
                _state = DispatcherState.Stopping;
                unregister = _registered;
                _registered = false;
                cancel = true;
            }
        }

        if (unregister)
        {
            Unregister();
        }

        if (cancel)
        {
            _shutdownSource.Cancel();
        }
    }

    /// <summary>
    ///  Stops the dispatcher and faults all queued, scheduled, and active asynchronous work that remains.
    /// </summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own the dispatcher.</exception>
    internal void Stop()
    {
        VerifyAccess();

        List<DispatcherWorkItem> pending = [];
        bool unregister;
        DispatcherState terminalState;
        lock (_lock)
        {
            if (_state == DispatcherState.Stopped)
            {
                return;
            }

            unregister = _registered;
            _registered = false;

            if (_state != DispatcherState.Faulted)
            {
                _state = DispatcherState.Stopping;
            }

            _wakeRequested = false;
            DetachPendingWorkLocked(pending);

            if (_state != DispatcherState.Faulted)
            {
                _state = DispatcherState.Stopped;
            }

            terminalState = _state;
        }

        if (unregister)
        {
            Unregister();
        }

        foreach (DispatcherWorkItem workItem in pending)
        {
            workItem.CompleteFaulted(CreateUnavailableException(terminalState, workItem.Id));
            EmitOperationCompleted(workItem);
        }

        Exception? cleanupException = null;

        try
        {
            _wake.CancelDelayedWake();
        }
        catch (Exception cancelWakeException)
        {
            cleanupException = cancelWakeException;
        }

        try
        {
            _shutdownSource.Cancel();
        }
        catch (Exception cancellationException)
        {
            cleanupException = cleanupException is null
                ? cancellationException
                : new AggregateException(cleanupException, cancellationException);
        }

        DispatcherEventSource.Log.Stopped(
            (int)_nativeThreadId,
            _highWatermark,
            _postedCount,
            _completedCount);

        if (cleanupException is not null)
        {
            ExceptionDispatchInfo.Capture(cleanupException).Throw();
        }
    }

    /// <summary>
    ///  Cancels a work item if it has not started.
    /// </summary>
    /// <param name="workItem">The queued work item to cancel.</param>
    internal void Cancel(DispatcherWorkItem workItem)
    {
        lock (_lock)
        {
            if (workItem.State != DispatcherWorkItemState.Queued)
            {
                return;
            }

            workItem.State = DispatcherWorkItemState.Canceled;
            _completedCount++;
            workItem.Release();
        }

        workItem.CompleteCanceled();
        EmitOperationCompleted(workItem);

        // Let the pump discard the canceled tombstone and reschedule the next delayed wake.
        RequestWake();
    }

    /// <summary>
    ///  Transitions a running work item to a terminal state and records its completion.
    /// </summary>
    /// <param name="workItem">The running work item to transition.</param>
    /// <param name="terminalState">The terminal state to assign.</param>
    /// <returns><see langword="true"/> if the transition was applied.</returns>
    internal bool TryTransitionToTerminal(DispatcherWorkItem workItem, DispatcherWorkItemState terminalState)
    {
        lock (_lock)
        {
            if (workItem.State != DispatcherWorkItemState.Running)
            {
                return false;
            }

            workItem.State = terminalState;
            _completedCount++;
            _activeAsyncWork.Remove(workItem);
        }

        EmitOperationCompleted(workItem);
        return true;
    }

    /// <summary>
    ///  Tracks a work item whose asynchronous callback has not completed.
    /// </summary>
    /// <param name="workItem">The running asynchronous work item.</param>
    internal void MarkAsyncPending(DispatcherWorkItem workItem)
    {
        lock (_lock)
        {
            if (workItem.State == DispatcherWorkItemState.Running)
            {
                _activeAsyncWork.Add(workItem);
            }
        }
    }

    /// <summary>
    ///  Stops tracking an asynchronous work item.
    /// </summary>
    /// <param name="workItem">The asynchronous work item to remove.</param>
    internal void RemoveAsyncPending(DispatcherWorkItem workItem)
    {
        lock (_lock)
        {
            _activeAsyncWork.Remove(workItem);
        }
    }

    /// <summary>
    ///  Promotes due scheduled work and invokes at most one queued work item.
    /// </summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own the dispatcher.</exception>
    private void ProcessQueue()
    {
        VerifyAccess();

        PromoteScheduledWork();

        DispatcherWorkItem? workItem = null;
        lock (_lock)
        {
            // Accept the current wake before dequeueing so producers racing this turn can request the next one.
            _wakeRequested = false;

            while (_queue.TryDequeue(out DispatcherWorkItem? candidate))
            {
                if (candidate.State == DispatcherWorkItemState.Queued)
                {
                    candidate.State = DispatcherWorkItemState.Running;
                    workItem = candidate;
                    break;
                }
            }
        }

        if (workItem is not null)
        {
            DispatcherEventSource.Log.OperationStarted(
                (int)_nativeThreadId,
                workItem.Id,
                Math.Max(0, GetMonotonicTicks() - workItem.AdmittedTicks));
        }

        workItem?.Invoke();

        // One operation per wake yields back to USER32 between callbacks; post another turn only when work remains.
        RearmWakeForQueuedWork();
    }

    /// <summary>
    ///  Processes a dispatcher wake message and faults the dispatcher if processing fails.
    /// </summary>
    internal void ProcessWake()
    {
        try
        {
            ProcessQueue();
        }
        catch (Exception exception)
        {
            Fault(exception);
        }
    }

    /// <summary>
    ///  Cancels the delivered delayed wake, processes due work, and faults the dispatcher if processing fails.
    /// </summary>
    internal void ProcessDelayedWake()
    {
        try
        {
            _wake.CancelDelayedWake();
            ProcessQueue();
        }
        catch (Exception exception)
        {
            Fault(exception);
        }
    }

    /// <summary>
    ///  Rethrows the exception that faulted the dispatcher, if any.
    /// </summary>
    internal void ThrowIfFaulted() => Volatile.Read(ref _fault)?.Throw();

    /// <summary>
    ///  Raises <see cref="UnhandledException"/> for a fire-and-forget callback and faults the dispatcher if unhandled.
    /// </summary>
    /// <param name="exception">The callback exception to report.</param>
    internal void ReportUnhandledException(Exception exception)
    {
        DispatcherUnhandledExceptionEventArgs arguments = new(exception);

        try
        {
            UnhandledException?.Invoke(this, arguments);
        }
        catch (Exception handlerException)
        {
            Fault(new AggregateException(exception, handlerException));
            return;
        }

        if (!arguments.Handled)
        {
            Fault(exception);
        }
    }

    private void RearmWakeForQueuedWork()
    {
        bool shouldPost;
        lock (_lock)
        {
            shouldPost = _state == DispatcherState.Running && _queue.Count > 0 && !_wakeRequested;
            if (shouldPost)
            {
                _wakeRequested = true;
            }
        }

        if (shouldPost)
        {
            PostWake();
        }
    }

    private void RequestWake()
    {
        bool shouldPost;
        lock (_lock)
        {
            shouldPost = _state == DispatcherState.Running && !_wakeRequested;
            if (shouldPost)
            {
                _wakeRequested = true;
            }
        }

        if (shouldPost)
        {
            PostWake();
        }
    }

    private bool TrySchedule<TWorkItem>(
        TimeSpan delay,
        Func<long, TWorkItem> createWorkItem,
        [NotNullWhen(true)] out TWorkItem? workItem,
        [NotNullWhen(false)] out ObjectDisposedException? exception)
        where TWorkItem : DispatcherWorkItem
    {
        lock (_lock)
        {
            if (_state != DispatcherState.Running)
            {
                workItem = null;
                exception = CreateUnavailableException(_state);
                return false;
            }

            long id = ++_nextOperationId;
            workItem = createWorkItem(id);
            workItem.State = DispatcherWorkItemState.Queued;
            _scheduledWork.Enqueue(workItem, (GetDueTicks(delay), id));
            CaptureAdmissionMetricsLocked(workItem);
            exception = null;
            return true;
        }
    }

    private void PromoteScheduledWork()
    {
        long currentTicks = GetMonotonicTicks();
        long? nextDueTicks = null;

        lock (_lock)
        {
            while (_scheduledWork.TryPeek(out DispatcherWorkItem? workItem, out (long DueTicks, long Id) priority))
            {
                // Cancellation leaves a tombstone in the priority queue; remove it lazily to avoid a linear search.
                if (workItem.State != DispatcherWorkItemState.Queued)
                {
                    _scheduledWork.Dequeue();
                    continue;
                }

                if (priority.DueTicks > currentTicks)
                {
                    nextDueTicks = priority.DueTicks;
                    break;
                }

                _scheduledWork.Dequeue();
                _queue.Enqueue(workItem);
            }
        }

        if (nextDueTicks is long dueTicks)
        {
            uint delayMilliseconds = GetWakeDelayMilliseconds(dueTicks - currentTicks);
            _wake.WakeAfter(delayMilliseconds);
            DispatcherEventSource.Log.DelayedWakeScheduled((int)_nativeThreadId, delayMilliseconds);
        }
        else
        {
            _wake.CancelDelayedWake();
        }
    }

    private long GetDueTicks(TimeSpan delay)
    {
        long currentTicks = GetMonotonicTicks();

        // Saturation keeps very long delays ordered without allowing signed overflow to make them immediately due.
        return delay.Ticks > long.MaxValue - currentTicks
            ? long.MaxValue
            : currentTicks + delay.Ticks;
    }

    private long GetMonotonicTicks() => _timeProvider.GetElapsedTime(_timestampOrigin).Ticks;

    private static uint GetWakeDelayMilliseconds(long remainingTicks)
    {
        Debug.Assert(remainingTicks > 0);

        // WakeAfter accepts whole milliseconds. Round up so a callback is never promoted before its deadline.
        long milliseconds = remainingTicks / TimeSpan.TicksPerMillisecond;
        if (remainingTicks % TimeSpan.TicksPerMillisecond != 0)
        {
            milliseconds++;
        }

        return (uint)Math.Clamp(milliseconds, 1, uint.MaxValue);
    }

    private static void ValidateDelay(TimeSpan delay) => ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);

    private void PostWake()
    {
        try
        {
            _wake.Wake();
        }
        catch (Exception exception)
        {
            Fault(exception);
        }
    }

    private void CaptureAdmissionMetricsLocked(DispatcherWorkItem workItem)
    {
        workItem.AdmittedTicks = GetMonotonicTicks();
        workItem.QueueDepthAtAdmission = _queue.Count + _scheduledWork.Count + _activeAsyncWork.Count;
        _highWatermark = Math.Max(_highWatermark, workItem.QueueDepthAtAdmission);
        _postedCount++;
    }

    private void EmitOperationCompleted(DispatcherWorkItem workItem)
        => DispatcherEventSource.Log.OperationCompleted(
            (int)_nativeThreadId,
            workItem.Id,
            (int)workItem.State);

    private void PublishAdmission(DispatcherWorkItem workItem)
    {
        // Register can cancel synchronously, so publish admission before a cancellation completion event can fire.
        DispatcherEventSource.Log.OperationPosted(
            (int)_nativeThreadId,
            workItem.Id,
            workItem.QueueDepthAtAdmission,
            Volatile.Read(ref _highWatermark));
        workItem.RegisterCancellation();

        // Delayed admissions also wake immediately so ProcessQueue can schedule the earliest delayed wake.
        RequestWake();
    }

    private void Fault(Exception exception)
    {
        List<DispatcherWorkItem> pending = [];
        bool unregister;

        lock (_lock)
        {
            if (_state == DispatcherState.Faulted)
            {
                return;
            }

            _state = DispatcherState.Faulted;
            _wakeRequested = false;
            unregister = _registered;
            _registered = false;
            Volatile.Write(ref _fault, ExceptionDispatchInfo.Capture(exception));
            DetachPendingWorkLocked(pending);
        }

        if (unregister)
        {
            Unregister();
        }

        foreach (DispatcherWorkItem workItem in pending)
        {
            workItem.CompleteFaulted(exception);
            EmitOperationCompleted(workItem);
        }

        DispatcherEventSource.Log.Faulted(
            (int)_nativeThreadId,
            exception.HResult,
            exception.GetType().FullName ?? exception.GetType().Name);

        try
        {
            _shutdownSource.Cancel();
        }
        catch (Exception cancellationException)
        {
            Volatile.Write(
                ref _fault,
                ExceptionDispatchInfo.Capture(new AggregateException(exception, cancellationException)));
        }
        finally
        {
            SignalFaultToOwner();
        }
    }

    // Detach under the queue lock, then complete tasks outside it so arbitrary continuations cannot run under _lock.
    private void DetachPendingWorkLocked(List<DispatcherWorkItem> pending)
    {
        while (_queue.TryDequeue(out DispatcherWorkItem? workItem))
        {
            if (workItem.State == DispatcherWorkItemState.Queued)
            {
                Detach(workItem);
            }
        }

        while (_scheduledWork.TryDequeue(out DispatcherWorkItem? workItem, out _))
        {
            if (workItem.State == DispatcherWorkItemState.Queued)
            {
                Detach(workItem);
            }
        }

        foreach (DispatcherWorkItem workItem in _activeAsyncWork)
        {
            if (workItem.State == DispatcherWorkItemState.Running)
            {
                Detach(workItem);
            }
        }

        _activeAsyncWork.Clear();

        void Detach(DispatcherWorkItem workItem)
        {
            workItem.State = DispatcherWorkItemState.Faulted;
            _completedCount++;
            workItem.Release();
            pending.Add(workItem);
        }
    }

    private void SignalFaultToOwner()
    {
        // Owner-thread faults return to a managed pump boundary, which calls ThrowIfFaulted directly.
        if (CheckAccess())
        {
            return;
        }

        // A foreign thread must wake GetMessage. WM_QUIT is independent of the wake HWND and starts pump teardown.
        if (PInvoke.PostThreadMessage(_nativeThreadId, Interop.WM_QUIT, default, default))
        {
            return;
        }

        Exception postThreadException =
            new ThirtyTwoException(Error.GetLastError(), "Failed to post WM_QUIT to the dispatcher thread.");

        try
        {
            // Preserve the PostThreadMessage failure, but use the ordinary wake HWND as a best-effort fallback.
            _wake.Wake();
            AddFaultException(postThreadException);
        }
        catch (Exception wakeException)
        {
            AddFaultException(new AggregateException(postThreadException, wakeException));
        }
    }

    private void AddFaultException(Exception exception)
    {
        ExceptionDispatchInfo? existing = Volatile.Read(ref _fault);
        Exception combined = existing is null
            ? exception
            : new AggregateException(existing.SourceException, exception);
        Volatile.Write(ref _fault, ExceptionDispatchInfo.Capture(combined));
    }

    private void Unregister()
    {
        bool removed = s_dispatchersByThreadId.TryRemove(_nativeThreadId, out Dispatcher? dispatcher);
        Debug.Assert(removed && ReferenceEquals(dispatcher, this));
    }

    private ObjectDisposedException CreateUnavailableException(DispatcherState state, long? operationId = null)
    {
        string operation = operationId is long id ? $" Operation {id}." : string.Empty;
        return new ObjectDisposedException(
            nameof(Dispatcher),
            $"Dispatcher on native thread {_nativeThreadId} is in state '{state}'.{operation}");
    }

    /// <summary>
    ///  Stops the dispatcher and releases its wake transport and shutdown token source.
    /// </summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own the dispatcher.</exception>
    internal void Dispose()
    {
        VerifyAccess();
        Exception? cleanupException = null;

        try
        {
            Stop();
        }
        catch (Exception stopException)
        {
            cleanupException = stopException;
        }

        try
        {
            _wake.Dispose();
        }
        catch (Exception wakeException)
        {
            cleanupException = cleanupException is null
                ? wakeException
                : new AggregateException(cleanupException, wakeException);
        }
        finally
        {
            _completion.TrySetResult();
        }

        if (cleanupException is not null)
        {
            ExceptionDispatchInfo.Capture(cleanupException).Throw();
        }
    }
}
