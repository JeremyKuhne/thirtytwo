// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents a synchronous function that returns a result.
/// </summary>
/// <typeparam name="TResult">The function result type.</typeparam>
internal sealed class DispatcherFuncWorkItem<TResult> : DispatcherWorkItem
{
    private readonly TaskCompletionSource<TResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Func<TResult>? _callback;

    /// <summary>
    ///  Initializes a synchronous function work item.
    /// </summary>
    /// <param name="dispatcher">The dispatcher that owns the operation.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="callback">The function to invoke.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    internal DispatcherFuncWorkItem(
        Dispatcher dispatcher,
        long id,
        Func<TResult> callback,
        CancellationToken cancellationToken)
        : base(dispatcher, id, cancellationToken)
    {
        _callback = callback;
    }

    /// <inheritdoc/>
    internal override Task Task => _completion.Task;

    /// <inheritdoc/>
    protected override void InvokeCore()
    {
        Func<TResult> callback = _callback!;
        _callback = null;

        try
        {
            TResult result = callback();
            if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Succeeded))
            {
                CompleteCancellationRegistration();
                _completion.TrySetResult(result);
            }
        }
        catch (Exception exception)
        {
            if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Faulted))
            {
                CompleteFaulted(exception);
            }
        }
    }

    /// <inheritdoc/>
    internal override void CompleteCanceled()
    {
        CompleteCancellationRegistration();
        _completion.TrySetCanceled(CancellationToken);
    }

    /// <inheritdoc/>
    internal override void CompleteFaulted(Exception exception)
    {
        CompleteCancellationRegistration();
        _completion.TrySetException(exception);
    }

    /// <inheritdoc/>
    protected override void ReleaseCallback() => _callback = null;

    /// <summary>
    ///  Gets the task that contains the operation result.
    /// </summary>
    /// <returns>The typed operation task.</returns>
    internal Task<TResult> GetTask() => _completion.Task;
}
