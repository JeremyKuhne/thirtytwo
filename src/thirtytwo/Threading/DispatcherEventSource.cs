// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace Windows.Threading;

/// <summary>
///  Emits dispatcher lifecycle, queue, timer, and fault events.
/// </summary>
[EventSource(Name = "ThirtyTwo-Dispatcher")]
internal sealed class DispatcherEventSource : EventSource
{
    /// <summary>
    ///  Gets the dispatcher event source.
    /// </summary>
    internal static DispatcherEventSource Log { get; } = new();

    /// <summary>
    ///  Records that a dispatcher started.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    [Event(1, Level = EventLevel.Informational)]
    public void Started(int nativeThreadId) => WriteEvent(1, nativeThreadId);

    /// <summary>
    ///  Records that an operation was posted.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="operationId">The operation identifier.</param>
    /// <param name="queueDepth">The queue depth at admission.</param>
    /// <param name="highWatermark">The highest queue depth observed.</param>
    [Event(2, Level = EventLevel.Verbose)]
    public void OperationPosted(int nativeThreadId, long operationId, int queueDepth, int highWatermark)
        => WriteEvent(2, nativeThreadId, operationId, queueDepth, highWatermark);

    /// <summary>
    ///  Records that an operation started.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="operationId">The operation identifier.</param>
    /// <param name="queueLatencyTicks">The monotonic queue latency in <see cref="TimeSpan"/> ticks.</param>
    [Event(3, Level = EventLevel.Verbose)]
    public void OperationStarted(int nativeThreadId, long operationId, long queueLatencyTicks)
        => WriteEvent(3, nativeThreadId, operationId, queueLatencyTicks);

    /// <summary>
    ///  Records that an operation completed.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="operationId">The operation identifier.</param>
    /// <param name="state">The terminal <see cref="DispatcherWorkItemState"/> value.</param>
    [Event(4, Level = EventLevel.Verbose)]
    public void OperationCompleted(int nativeThreadId, long operationId, int state)
        => WriteEvent(4, nativeThreadId, operationId, state);

    /// <summary>
    ///  Records that a delayed wake was scheduled.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="delayMilliseconds">The requested delay in milliseconds.</param>
    [Event(5, Level = EventLevel.Verbose)]
    public void DelayedWakeScheduled(int nativeThreadId, uint delayMilliseconds)
        => WriteEvent(5, nativeThreadId, delayMilliseconds);

    /// <summary>
    ///  Records that the dispatcher faulted.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="hresult">The fault HRESULT.</param>
    /// <param name="exceptionType">The exception type name.</param>
    [Event(6, Level = EventLevel.Error)]
    public void Faulted(int nativeThreadId, int hresult, string exceptionType)
        => WriteEvent(6, nativeThreadId, hresult, exceptionType);

    /// <summary>
    ///  Records that the dispatcher stopped.
    /// </summary>
    /// <param name="nativeThreadId">The native dispatcher thread identifier.</param>
    /// <param name="highWatermark">The highest queue depth observed.</param>
    /// <param name="postedCount">The number of operations posted.</param>
    /// <param name="completedCount">The number of operations completed.</param>
    [Event(7, Level = EventLevel.Informational)]
    public void Stopped(int nativeThreadId, int highWatermark, long postedCount, long completedCount)
        => WriteEvent(7, nativeThreadId, highWatermark, postedCount, completedCount);
}
