// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

/// <summary>
///  Delivers immediate and delayed queue-processing signals to a dispatcher's owning thread.
/// </summary>
/// <remarks>
///  <para>
///   This interface separates scheduling policy from delivery. <see cref="Dispatcher"/> owns queue ordering, wake
///   coalescing, deadline selection, cancellation, faults, and shutdown. An implementation owns only the mechanism and
///   resources used to deliver signals on the dispatcher thread.
///  </para>
///  <para>
///   The default implementation uses a message-only HWND and a USER32 timer. Keeping that mechanism behind this
///   boundary allows another owner-thread signaling mechanism, or a deterministic test transport, without duplicating
///   the dispatcher state machine.
///  </para>
///  <para>
///   The dispatcher requests at most one immediate signal and one delayed signal through a transport instance.
///  </para>
///  <para>
///   Immediate and delayed signals are independent. Delivering or canceling one must not implicitly consume the
///   other.
///  </para>
/// </remarks>
internal interface IDispatcherWake : IDisposable
{
    /// <summary>
    ///  Requests queue processing without an intentional delay.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The implementation schedules delivery on the dispatcher's owning thread and returns without processing
    ///   dispatcher work inline on the calling thread.
    ///  </para>
    ///  <para>
    ///   Any pending delayed wake remains pending. While processing this immediate wake, the dispatcher may explicitly
    ///   replace or cancel the delayed wake after reevaluating delayed work.
    ///  </para>
    /// </remarks>
    void Wake();

    /// <summary>
    ///  Requests queue processing after the specified delay, replacing any pending delayed wake.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Implementations may use a native timer or any equivalent mechanism. The dispatcher calls
    ///   <see cref="CancelDelayedWake"/> after delivery and whenever the pending delayed wake is no longer needed;
    ///   implementations do not call it themselves.
    ///  </para>
    ///  <para>This method does not affect a pending immediate wake.</para>
    /// </remarks>
    /// <param name="delayMilliseconds">The minimum delay in milliseconds.</param>
    void WakeAfter(uint delayMilliseconds);

    /// <summary>
    ///  Cancels the pending delayed wake. Has no effect when none is pending or it has already been delivered.
    /// </summary>
    /// <remarks>
    ///  <para>This method does not affect a pending immediate wake.</para>
    /// </remarks>
    void CancelDelayedWake();
}
