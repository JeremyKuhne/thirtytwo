// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents one dispatcher operation and its execution, cancellation, and completion state.
/// </summary>
internal abstract class DispatcherWorkItem
{
    private readonly Lock _cancellationLock = new();
    private CancellationTokenRegistration? _cancellationRegistration;
    private ExecutionContext? _executionContext;
    private bool _cancellationRegistrationCompleted;

    /// <summary>
    ///  Initializes a dispatcher work item.
    /// </summary>
    /// <param name="dispatcher">The dispatcher that owns the operation.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    protected DispatcherWorkItem(Dispatcher dispatcher, long id, CancellationToken cancellationToken)
    {
        Dispatcher = dispatcher;
        Id = id;
        CancellationToken = cancellationToken;
        _executionContext = ExecutionContext.Capture();
    }

    /// <summary>
    ///  Gets the dispatcher that owns this operation.
    /// </summary>
    internal Dispatcher Dispatcher { get; }

    /// <summary>
    ///  Gets the operation identifier.
    /// </summary>
    internal long Id { get; }

    /// <summary>
    ///  Gets or sets the monotonic timestamp recorded when the operation was admitted.
    /// </summary>
    internal long AdmittedTicks { get; set; }

    /// <summary>
    ///  Gets or sets the queue depth recorded when the operation was admitted.
    /// </summary>
    internal int QueueDepthAtAdmission { get; set; }

    /// <summary>
    ///  Gets the caller's cancellation token.
    /// </summary>
    internal CancellationToken CancellationToken { get; }

    /// <summary>
    ///  Gets or sets the operation state.
    /// </summary>
    internal DispatcherWorkItemState State { get; set; }

    /// <summary>
    ///  Gets the task that represents operation completion.
    /// </summary>
    internal abstract Task Task { get; }

    /// <summary>
    ///  Registers cancellation of an operation that has not started.
    /// </summary>
    internal void RegisterCancellation()
    {
        if (!CancellationToken.CanBeCanceled)
        {
            return;
        }

        CancellationTokenRegistration registration = CancellationToken.Register(
            static state => ((DispatcherWorkItem)state!).Dispatcher.Cancel((DispatcherWorkItem)state),
            this,
            useSynchronizationContext: false);

        bool disposeRegistration;
        lock (_cancellationLock)
        {
            // Register can invoke Cancel synchronously before returning. Completion and registration therefore use
            // this handoff: whichever publishes second disposes the registration, always outside the lock.
            disposeRegistration = _cancellationRegistrationCompleted;
            if (!disposeRegistration)
            {
                Debug.Assert(_cancellationRegistration is null);
                _cancellationRegistration = registration;
            }
        }

        if (disposeRegistration)
        {
            registration.Dispose();
        }
    }

    /// <summary>
    ///  Closes the cancellation-registration lifecycle and disposes a registration that has already been published.
    /// </summary>
    protected void CompleteCancellationRegistration()
    {
        if (!CancellationToken.CanBeCanceled)
        {
            return;
        }

        CancellationTokenRegistration? registration;

        lock (_cancellationLock)
        {
            _cancellationRegistrationCompleted = true;
            registration = _cancellationRegistration;
            _cancellationRegistration = null;
        }

        registration?.Dispose();
    }

    /// <summary>
    ///  Invokes the operation under its captured execution context and the dispatcher synchronization context.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The captured execution context preserves caller state such as <see cref="AsyncLocal{T}"/> values. The
    ///   dispatcher synchronization context is then installed inside it so ordinary awaits resume on the UI thread.
    ///  </para>
    /// </remarks>
    internal void Invoke()
    {
        ExecutionContext? executionContext = _executionContext;
        _executionContext = null;

        if (executionContext is null)
        {
            InvokeWithDispatcherContext(this);
        }
        else
        {
            ExecutionContext.Run(
                executionContext,
                static state => InvokeWithDispatcherContext((DispatcherWorkItem)state!),
                this);
        }

        static void InvokeWithDispatcherContext(DispatcherWorkItem workItem)
        {
            SynchronizationContext? previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(workItem.Dispatcher.SynchronizationContext);

            try
            {
                workItem.InvokeCore();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }

    /// <summary>
    ///  Invokes the operation callback and completes the operation.
    /// </summary>
    protected abstract void InvokeCore();

    /// <summary>
    ///  Completes the operation as canceled.
    /// </summary>
    internal abstract void CompleteCanceled();

    /// <summary>
    ///  Completes the operation as faulted.
    /// </summary>
    /// <param name="exception">The exception that faulted the operation.</param>
    internal abstract void CompleteFaulted(Exception exception);

    /// <summary>
    ///  Releases references that are no longer needed after the operation becomes terminal.
    /// </summary>
    internal void Release()
    {
        _executionContext = null;
        ReleaseCallback();
    }

    /// <summary>
    ///  Releases the operation callback.
    /// </summary>
    protected abstract void ReleaseCallback();
}
