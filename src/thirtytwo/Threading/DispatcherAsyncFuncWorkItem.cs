// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents an asynchronous function that receives an effective cancellation token and returns a result.
/// </summary>
/// <typeparam name="TResult">The function result type.</typeparam>
internal sealed class DispatcherAsyncFuncWorkItem<TResult> : DispatcherWorkItem
{
    private readonly TaskCompletionSource<TResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource? _linkedCancellationSource;
    private readonly CancellationToken _effectiveCancellationToken;
    private Func<CancellationToken, ValueTask<TResult>>? _callback;
    private bool _observerOwnsLinkedCancellationSource;

    /// <summary>
    ///  Initializes an asynchronous function work item.
    /// </summary>
    /// <param name="dispatcher">The dispatcher that owns the operation.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="callback">The asynchronous function to invoke.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    internal DispatcherAsyncFuncWorkItem(
        Dispatcher dispatcher,
        long id,
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken)
        : base(dispatcher, id, cancellationToken)
    {
        _callback = callback;

        // Running callbacks observe both caller cancellation and dispatcher shutdown. Avoid allocating a linked
        // source when only shutdown can cancel the callback.
        if (cancellationToken.CanBeCanceled)
        {
            _linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                dispatcher.ShutdownToken);
            _effectiveCancellationToken = _linkedCancellationSource.Token;
        }
        else
        {
            _effectiveCancellationToken = dispatcher.ShutdownToken;
        }
    }

    /// <inheritdoc/>
    internal override Task Task => _completion.Task;

    /// <summary>
    ///  Gets the task that contains the operation result.
    /// </summary>
    /// <returns>The typed operation task.</returns>
    internal Task<TResult> GetTask() => _completion.Task;

    /// <inheritdoc/>
    protected override void InvokeCore()
    {
        Func<CancellationToken, ValueTask<TResult>> callback = _callback!;
        _callback = null;

        ValueTask<TResult> callbackTask;
        try
        {
            callbackTask = callback(_effectiveCancellationToken);
        }
        catch (Exception exception)
        {
            CompleteFromException(exception);
            DisposeLinkedCancellationSource();
            return;
        }

        if (callbackTask.IsCompleted)
        {
            try
            {
                CompleteSucceeded(callbackTask.GetAwaiter().GetResult());
            }
            catch (Exception exception)
            {
                CompleteFromException(exception);
            }
            finally
            {
                DisposeLinkedCancellationSource();
            }

            return;
        }

        // An incomplete callback may outlive dispatcher shutdown. Its observer retains and ultimately disposes the
        // linked source even if shutdown makes the public Task terminal first.
        _observerOwnsLinkedCancellationSource = true;
        Dispatcher.MarkAsyncPending(this);
        _ = ObserveAsync(callbackTask);
    }

    private async Task ObserveAsync(ValueTask<TResult> callbackTask)
    {
        try
        {
            TResult result = await callbackTask.ConfigureAwait(false);
            CompleteSucceeded(result);
        }
        catch (Exception exception)
        {
            CompleteFromException(exception);
        }
        finally
        {
            Dispatcher.RemoveAsyncPending(this);
            DisposeLinkedCancellationSource();
        }
    }

    private void CompleteSucceeded(TResult result)
    {
        if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Succeeded))
        {
            CompleteCancellationRegistration();
            _completion.TrySetResult(result);
        }
    }

    private void CompleteFromException(Exception exception)
    {
        if (exception is OperationCanceledException canceled
            && canceled.CancellationToken == _effectiveCancellationToken)
        {
            if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Canceled))
            {
                CompleteCancellationRegistration();

                // Prefer the caller's token when it initiated cancellation; otherwise report dispatcher shutdown.
                CancellationToken token = CancellationToken.IsCancellationRequested
                    ? CancellationToken
                    : _effectiveCancellationToken;
                _completion.TrySetCanceled(token);
            }

            return;
        }

        if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Faulted))
        {
            CompleteFaulted(exception);
        }
    }

    /// <inheritdoc/>
    internal override void CompleteCanceled()
    {
        CompleteCancellationRegistration();
        DisposeLinkedCancellationSource();
        _completion.TrySetCanceled(CancellationToken);
    }

    /// <inheritdoc/>
    internal override void CompleteFaulted(Exception exception)
    {
        CompleteCancellationRegistration();

        // ObserveAsync still uses the effective token until the callback itself exits.
        if (!_observerOwnsLinkedCancellationSource)
        {
            DisposeLinkedCancellationSource();
        }

        _completion.TrySetException(exception);
    }

    /// <inheritdoc/>
    protected override void ReleaseCallback() => _callback = null;

    private void DisposeLinkedCancellationSource() => _linkedCancellationSource?.Dispose();
}
