// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Routes synchronization-context callbacks through a dispatcher.
/// </summary>
/// <param name="dispatcher">The dispatcher that owns this context.</param>
internal sealed class DispatcherSynchronizationContext(Dispatcher dispatcher) : SynchronizationContext
{
    /// <inheritdoc/>
    public override SynchronizationContext CreateCopy() => new DispatcherSynchronizationContext(dispatcher);

    /// <inheritdoc/>
    public override void Post(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        dispatcher.Post(callback, state);
    }

    /// <inheritdoc/>
    /// <remarks>
    ///  <para>
    ///   Runs inline on the dispatcher thread. A foreign thread blocks without pumping until the queued callback
    ///   completes; dispatcher-aware callers should prefer
    ///   <see cref="Dispatcher.InvokeAsync(Action, CancellationToken)"/>.
    ///  </para>
    /// </remarks>
    public override void Send(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (dispatcher.CheckAccess())
        {
            callback(state);
            return;
        }

        dispatcher.InvokeAsync(() => callback(state)).GetAwaiter().GetResult();
    }
}
