// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Represents an awaitable or fire-and-forget synchronous action.
/// </summary>
internal sealed class DispatcherActionWorkItem : DispatcherWorkItem
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly bool _fireAndForget;
    private Action? _callback;

    /// <summary>
    ///  Initializes a synchronous action work item.
    /// </summary>
    /// <param name="dispatcher">The dispatcher that owns the operation.</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="callback">The callback to invoke.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <param name="fireAndForget">Whether callback exceptions use the dispatcher exception policy.</param>
    internal DispatcherActionWorkItem(
        Dispatcher dispatcher,
        long id,
        Action callback,
        CancellationToken cancellationToken,
        bool fireAndForget = false)
        : base(dispatcher, id, cancellationToken)
    {
        _callback = callback;
        _fireAndForget = fireAndForget;
    }

    /// <inheritdoc/>
    internal override Task Task => _completion.Task;

    /// <inheritdoc/>
    protected override void InvokeCore()
    {
        Action callback = _callback!;
        _callback = null;

        try
        {
            callback();
            if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Succeeded))
            {
                CompleteCancellationRegistration();
                _completion.TrySetResult();
            }
        }
        catch (Exception exception)
        {
            if (Dispatcher.TryTransitionToTerminal(this, DispatcherWorkItemState.Faulted))
            {
                if (_fireAndForget)
                {
                    CompleteCancellationRegistration();
                    _completion.TrySetResult();
                    Dispatcher.ReportUnhandledException(exception);
                }
                else
                {
                    CompleteFaulted(exception);
                }
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
        if (_fireAndForget)
        {
            _completion.TrySetResult();
        }
        else
        {
            _completion.TrySetException(exception);
        }
    }

    /// <inheritdoc/>
    protected override void ReleaseCallback() => _callback = null;
}
