# UI Dispatcher Implementation Plan

## Status and Goal

Design and validate a modern UI dispatcher for `thirtytwo`. The dispatcher must
integrate with the existing USER32 message loop, support `async` code without
depending on a particular control HWND, and give every admitted operation a
deterministic terminal state.

This is an implementation and validation plan, not a committed public API. The
surface remains experimental until the API, shutdown, nested-loop, and stress
gates pass.

The design is informed by the current implementations of WinForms, WPF, and
.NET MAUI as of August 2026. It deliberately keeps their useful behavior while
avoiding compatibility constraints that a new implementation does not need.

## Implementation Status

The core dispatcher is merged on `main` through
[PR #26](https://github.com/JeremyKuhne/thirtytwo/pull/26) (`389e145`) as of
August 2026:

- `ThreadContext` owns the outer message loop, synchronization context, message
  filters, shutdown callbacks, and quit/fault state;
- `Dispatcher` owns a message-only HWND, one-item FIFO turns, Task completion,
  cancellation, execution-context flow, async `ValueTask` callbacks, delayed
  work, and fail-stop wake handling;
- `DispatcherTimer` uses monotonic prior-deadline scheduling and skips missed
  ticks rather than replaying a burst;
- `DispatcherEventSource` reports lifecycle, queue depth/high-water mark,
  operation latency/completion, timer arming, and faults; and
- 55 focused tests cover the dispatcher slice, including stable `Window`
  dispatcher affinity, active raw-child `HWND` discovery, shutdown lifecycle
  signals, plus a real COM file-dialog modal loop. The full Debug and Release
  suites each pass 341 tests with one manual test skipped.

The public surface remains experimental. Product-level `thirtytwo.winui`
integration and performance measurements against the comparison frameworks are
follow-up work in the WinUI hosting roadmap; no performance budget or optimal
drain-batch claim is made here.

## Pre-implementation Baseline

Before PR #26, [`Application.Run`](../src/thirtytwo/Application.cs):

- owned one blocking `GetMessage` loop directly;
- tied loop exit to destruction of one root `Window` through `PostQuitMessage`;
- had no per-thread context, dispatcher, or `SynchronizationContext`;
- preprocessed only messages whose HWND mapped to a managed `Window`;
- treated `GetMessage` failure (`-1`) as a retrieved message because nonzero
  `BOOL` values entered the loop, then attempted to preprocess and dispatch an
  invalid result;
- had no queue admission, cancellation, shutdown, or exception contract; and
- had no focused pump tests beyond incidental sample use.

[`ThreadModalScope`](../src/thirtytwo/Support/ThreadModalScope.cs) disabled and
reenabled visible thread windows, but it did not own or describe a nested
message loop. Native dialogs could pump messages independently while that scope
was active.

The dispatcher therefore could not be added as a helper beside the existing
loop. The owning abstraction first had to become one per-thread `ThreadContext`
that coordinates the pump, dispatcher, synchronization context, filters,
nesting, and shutdown.

## Framework Investigation

The source links below are pinned to the revisions inspected for this plan.

### WinForms

Sources:

- [`Application.ThreadContext`](https://github.com/dotnet/winforms/blob/c59856435bb6fb12cbc329053175d33c037ae3c9/src/System.Windows.Forms/System/Windows/Forms/Application.ThreadContext.cs)
- [`WindowsFormsSynchronizationContext`](https://github.com/dotnet/winforms/blob/c59856435bb6fb12cbc329053175d33c037ae3c9/src/System.Windows.Forms/System/Windows/Forms/WindowsFormsSynchronizationContext.cs)
- [`Control` marshaled invocation](https://github.com/dotnet/winforms/blob/c59856435bb6fb12cbc329053175d33c037ae3c9/src/System.Windows.Forms/System/Windows/Forms/Control.cs)
- [`Control.InvokeAsync`](https://github.com/dotnet/winforms/blob/c59856435bb6fb12cbc329053175d33c037ae3c9/src/System.Windows.Forms/System/Windows/Forms/Control_InvokeAsync.cs)
- [`Control.ThreadMethodEntry`](https://github.com/dotnet/winforms/blob/c59856435bb6fb12cbc329053175d33c037ae3c9/src/System.Windows.Forms/System/Windows/Forms/Control.ThreadMethodEntry.cs)

Implementation findings:

- `Application.ThreadContext` is the per-thread owner for main and modal loops,
  message filters, OLE state, a marshalling control, quit state, and
  `WindowsFormsSynchronizationContext` installation.
- Cross-thread control invocation finds an ancestor control with a live HWND,
  queues a `ThreadMethodEntry`, and posts a registered message to that HWND.
- One callback message drains the entire control callback queue. Work captures
  the caller's `ExecutionContext`, then substitutes the destination thread's
  `SynchronizationContext` while invoking it.
- Synchronous `Invoke` waits on a `ManualResetEvent`. Same-thread synchronous
  calls execute directly to avoid self-deadlock.
- `WindowsFormsSynchronizationContext.Send` delegates to `Control.Invoke`, and
  `Post` delegates to `Control.BeginInvoke`.
- The newer `Control.InvokeAsync` overloads wrap legacy `BeginInvoke` with
  `TaskCompletionSource` using `RunContinuationsAsynchronously`. Dedicated
  overloads accept `Func<CancellationToken, ValueTask>` and await the whole
  callback rather than treating a returned task as a result value.

Useful behavior to retain:

- a real per-thread pump owner;
- execution-context flow with destination synchronization-context replacement;
- Task-based exception propagation for awaitable calls;
- asynchronous continuation completion; and
- separate overloads for synchronous and genuinely asynchronous callbacks.

Weaknesses not to inherit:

- Dispatch is coupled to a user/control HWND and its recreation and disposal
  lifetime. WinForms documents that `InvokeAsync` may never complete if the
  control handle is destroyed before the callback runs.
- A callback message drains the whole queue, so a producer flood can delay
  native input and painting.
- `Invoke` and `SynchronizationContext.Send` can participate in cross-thread
  deadlocks.
- Cancellation semantics differ by timing. A token canceled before entry can
  produce a successfully completed default result, while cancellation after
  admission can complete the returned task before an async callback has stopped.
- Legacy `BeginInvoke` exceptions are routed to `Application.ThreadException`
  rather than an awaiter.
- Passing an async lambda to a synchronous delegate overload can create
  `async void` behavior; passing a `Func<Task>` through a generic synchronous
  overload treats the task as a result unless the async-specific overload is
  selected.

### WPF

Sources:

- [`Dispatcher`](https://github.com/dotnet/wpf/blob/b5d953c77a948fbce344787f7480d419fd853334/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/Dispatcher.cs)
- [`DispatcherOperation`](https://github.com/dotnet/wpf/blob/b5d953c77a948fbce344787f7480d419fd853334/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherOperation.cs)
- [`DispatcherOperationTaskSource`](https://github.com/dotnet/wpf/blob/b5d953c77a948fbce344787f7480d419fd853334/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherOperationTaskSource.cs)
- [`DispatcherSynchronizationContext`](https://github.com/dotnet/wpf/blob/b5d953c77a948fbce344787f7480d419fd853334/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherSynchronizationContext.cs)
- [WPF threading model and reentrancy guidance](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/threading-model)

Implementation findings:

- A `Dispatcher` is bound to a thread and owns a message-only HWND, registered
  wake message, priority queue, timers, nested frames, shutdown state, hooks,
  and a synchronization context.
- Foreground operations post a wake message. Background operations run only
  when USER32 input is not pending, using a short timer when necessary.
- One wake processes one operation and requests another wake if work remains.
- `DispatcherOperation` captures `ExecutionContext`, exposes status, priority,
  abort, events, synchronous wait, a `Task`, and an awaiter.
- Same-thread `DispatcherOperation.Wait` pushes a nested frame rather than
  blocking. WPF explicitly documents the resulting nested-pump and reentrancy
  hazards.
- Shutdown aborts pending operations, destroys the hidden HWND, and leaves a
  terminal dispatcher associated with the thread.
- `DispatcherSynchronizationContext.Post` deliberately uses legacy
  `BeginInvoke` so unobservable callback exceptions reach
  `Dispatcher.UnhandledException`. `Send` uses synchronous `Invoke`.

Useful behavior to retain:

- a dispatcher-owned message-only HWND independent of application windows;
- explicit access checks and thread identity;
- coalesced native wake state;
- native-input awareness;
- deterministic cancellation of pending work during shutdown; and
- explicit nested-frame and shutdown ownership.

Weaknesses not to inherit:

- Ten public priority levels and mutable operation priority create a large
  scheduling contract and allow starvation patterns that are difficult to
  reason about.
- Public operation objects combine queue state, events, abort, synchronous
  waits, awaiters, and result access. This surface is unnecessary when callers
  primarily need TAP completion.
- `DispatcherOperation.Wait` pumps on the UI thread, introducing reentrancy into
  code that appears to wait synchronously.
- `CurrentDispatcher` auto-creates a dispatcher and hidden HWND merely by being
  queried.
- The task source does not request asynchronous continuation execution.
- WPF records that a failed wake `PostMessage` or timer request can leave the
  dispatcher waiting for work that will never be signaled.
- The timer implementation carries explicit tick-count wrap concerns.
- Generic `InvokeAsync(Func<TResult>)` does not model an async delegate as one
  operation; `TResult` can itself be a `Task`, requiring unwrapping by callers.

### .NET MAUI

Sources:

- [`IDispatcher`](https://github.com/dotnet/maui/blob/8266dc21fed86888d6390302a4fcc15db39e25d3/src/Core/src/Dispatching/IDispatcher.cs)
- [`DispatcherExtensions`](https://github.com/dotnet/maui/blob/8266dc21fed86888d6390302a4fcc15db39e25d3/src/Core/src/Dispatching/DispatcherExtensions.cs)
- [Windows backend](https://github.com/dotnet/maui/blob/8266dc21fed86888d6390302a4fcc15db39e25d3/src/Core/src/Dispatching/Dispatcher.Windows.cs)
- [Android backend](https://github.com/dotnet/maui/blob/8266dc21fed86888d6390302a4fcc15db39e25d3/src/Core/src/Dispatching/Dispatcher.Android.cs)
- [Apple backend](https://github.com/dotnet/maui/blob/8266dc21fed86888d6390302a4fcc15db39e25d3/src/Core/src/Dispatching/Dispatcher.iOS.cs)

Implementation findings:

- `IDispatcher` deliberately exposes only `IsDispatchRequired`, boolean
  `Dispatch`, delayed dispatch, and timer creation.
- Windows delegates to `DispatcherQueue.TryEnqueue`, Android to `Handler.Post`,
  and Apple to `DispatchQueue.DispatchAsync`.
- Task-returning extension methods wrap the boolean API with
  `TaskCompletionSource`; async overloads schedule an `async void` action that
  catches callback exceptions.
- Delayed dispatch and cancellation behavior differ by platform backend.

Useful behavior to retain:

- a small access-check and enqueue model;
- async delegate overloads that represent the complete callback; and
- platform dispatchers as adapters rather than UI-object methods.

Weaknesses not to inherit:

- The Task wrappers ignore a `false` result from `Dispatch`; the returned task
  can therefore remain incomplete.
- Their task sources do not request asynchronous continuation execution.
- The contract has no cancellation, shutdown, ordering, exception-policy, or
  queue-fairness semantics.
- Boolean success means different things on each platform, and some backends
  unconditionally return `true` after asking the platform to enqueue.
- Delayed work cannot be canceled through the dispatch call, and timer behavior
  is backend-specific.

## Design Conclusions

| Concern | Decision |
| --- | --- |
| Ownership | Bind one dispatcher to a `ThreadContext`, not to a `Window` or control HWND |
| Creation | Create it explicitly when the outermost `Application.Run` starts; querying `Current` never creates one |
| Wake mechanism | Use one dispatcher-owned message-only HWND and one coalesced registered wake message |
| Primary API | Return `Task`/`Task<T>`; keep queue work-item state internal |
| Async callback | Accept `Func<CancellationToken, ValueTask>` and generic equivalent; complete only after the callback finishes |
| Same-thread async call | Always enqueue; never silently inline `InvokeAsync` |
| Cancellation | Cancel pending work before start; after start cancellation is cooperative and never reports completion early |
| Continuations | Complete task sources with `RunContinuationsAsynchronously` |
| Ordering | FIFO admission; preserve order from each producer and define cross-producer order by queue admission |
| Priorities | Do not expose priorities initially; use bounded FIFO turns to preserve native-message fairness |
| Synchronous invoke | Do not add a public synchronous dispatcher API initially |
| Shutdown | Stop admission and complete every pending or active operation deterministically |
| Nested pumping | Model it explicitly for modal integration; never make Task waiting pump implicitly |
| Wake failure | Fault the dispatcher and all queued work; never leave an admitted task stranded |
| Timers | Add later using monotonic time, one native wake timer, and a due-time heap |
| Testability | Add no test-only production code at any visibility; tests use Touki `TestAccessor` to reach existing product implementation when contract-level testing is insufficient |

## Proposed Surface

Initial experimental surface in `Windows.Threading`:

```csharp
public sealed class Dispatcher
{
    public static Dispatcher? Current { get; }
  public static Dispatcher? FromHandle<T>(T handle) where T : IHandle<HWND>;

  public event EventHandler<DispatcherUnhandledExceptionEventArgs>?
    UnhandledException;

  public CancellationToken ShutdownToken { get; }
  public Task Completion { get; }

    public bool CheckAccess();
    public void VerifyAccess();

  public bool TryPost(Action callback);

  public DispatcherTimer CreateTimer(TimeSpan interval);

    public Task InvokeAsync(
        Action callback,
        CancellationToken cancellationToken = default);

    public Task<TResult> InvokeAsync<TResult>(
        Func<TResult> callback,
        CancellationToken cancellationToken = default);

    public Task InvokeAsync(
      Func<ValueTask> callback,
      CancellationToken cancellationToken = default);

    public Task<TResult> InvokeAsync<TResult>(
      Func<ValueTask<TResult>> callback,
      CancellationToken cancellationToken = default);

    public Task InvokeAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default);

    public Task<TResult> InvokeAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default);

    // Each callback shape also has a TimeSpan-delay overload.
}

public partial class Window
{
  public Dispatcher Dispatcher { get; }
}

public sealed class DispatcherUnhandledExceptionEventArgs : EventArgs
{
    public Exception Exception { get; }
    public bool Handled { get; set; }
}
```

Surface constraints:

- `Dispatcher.Current` returns `null` before an outer pump starts and after it
  ends. It never creates native resources.
- `Dispatcher.FromHandle(...)` resolves the active dispatcher from the native thread that owns an `HWND`; it accepts
  both raw handles and `IHandle<HWND>` wrappers without requiring a managed `Window` registration.
- `Window.Dispatcher` is a stable affinity reference after `Application.Run` associates the window. It remains available
  after shutdown and handle destruction; access before association throws. A later run can associate a surviving window
  with its fresh dispatcher only after the prior dispatcher completes. The `Application.Run(Func<Window>)` overload
  starts the dispatcher before constructing the root window.
- `Dispatcher.Current` and `Dispatcher.FromHandle(...)` remain nullable, active-only discovery APIs. Their results are
  snapshots and are never used as admission checks.
- `TryPost` atomically admits best-effort fire-and-forget work or returns `false` after admission closes.
- `InvokeAsync` validates arguments synchronously, but reports cancellation,
  rejection, callback exceptions, and shutdown through the returned task.
- A token already canceled at entry returns a canceled task carrying that
  token. It does not return a successful default value.
- The async callback overload starts the callback on the dispatcher thread and
  completes only after its returned `ValueTask` fully completes, including all
  nested awaits and continuations. Once the callback yields, the dispatcher
  remains free to process other messages and work.
- Callback code that captures the dispatcher synchronization context resumes on
  the UI thread. `ConfigureAwait(false)` remains an explicit opt-out by the
  callback author.
- Parameterless `Func<ValueTask>` overloads ensure natural parameterless async
  lambdas bind to a full-lifetime Task operation rather than `Action`/`async
  void`; compile-time and runtime tests pin that overload selection.
- Return `Task`, not `ValueTask`. Every cross-thread operation already needs a
  queue record and completion state, while `Task` supports multiple awaits,
  combinators, and safer consumption. `ValueTask` is used for callback inputs so
  callbacks that complete synchronously need not allocate a `Task`.
- Delayed overloads accept `TimeSpan` before the callback and use the same Task,
  cancellation, exception, and FIFO admission contracts after their due time.
- Do not expose `DispatcherOperation`. Add an operation handle later
  only if measured product scenarios require mutable priority, status queries,
  or explicit abort beyond `CancellationToken`.
- Keep `DispatcherSynchronizationContext` internal initially. Its installed
  instance remains visible as `SynchronizationContext.Current`, but callers do
  not need to construct it.
- Calling `InvokeAsync` from the dispatcher thread is valid and still queues.
  Awaiting the returned task yields control so the pump can run the callback;
  synchronously calling `Wait`, `Result`, or `GetResult` on that thread can
  deadlock and is unsupported.

## Behavioral Contract

### Admission and ordering

- Every accepted operation receives a diagnostic ID that is unique within that
  dispatcher lifetime.
- FIFO order is based on admission under the queue lock. Each producer's order
  is preserved; simultaneous producers have no ordering guarantee before
  admission.
- `InvokeAsync` always queues, including from the dispatcher thread. This gives
  it one cancellation and ordering model and prevents accidental recursive
  execution.
- A rejected operation is never inserted into the queue and the returned task
  is already terminal.

### Cancellation

- Check an already-canceled token before capturing `ExecutionContext` or taking
  the queue lock. Return a pre-canceled task; the callback is never admitted or
  started.
- After admission, cancellation and dequeue compete for one state transition
  under the queue lock: `Queued -> Canceled` or `Queued -> Running`. If
  cancellation wins, the callback never starts. If dequeue wins, cancellation
  is cooperative. The callback never has an ambiguous state in which both
  transitions succeeded.
- Cancellation that wins while queued releases captured callback state and
  completes the task as canceled with the caller's token.
- The queue may remove the item or skip a tombstone, but canceled closures must
  not remain retained until an unrelated future wake.
- Once an operation enters `Running`, cancellation cannot claim that execution
  has stopped. Synchronous callbacks run to completion. Async callbacks receive
  an effective token linked to caller cancellation and dispatcher shutdown and
  must cooperate.
- A running async callback is reported canceled only when it completes by
  throwing `OperationCanceledException` for its effective token. If it ignores
  cancellation and succeeds, its task succeeds.
- Cancellation registrations never capture a synchronization context and are
  disposed on every terminal path.

### Exceptions

- Exceptions from Task-returning operations are captured in their tasks and do
  not also reach a global dispatcher exception event.
- Exceptions from `SynchronizationContext.Post`, `async void`, or future
  fire-and-forget framework callbacks have no awaiter. Catch them in the queue
  processor and raise `Dispatcher.UnhandledException` on the UI thread.
- If that policy does not mark the exception handled, store its
  `ExceptionDispatchInfo`, request loop exit, perform deterministic cleanup, and
  rethrow from `Application.Run`. Do not throw across the native window-proc
  boundary.
- A wake failure is infrastructure failure, not a callback exception. Capture
  the last error immediately, transition the dispatcher to `Faulted`, detach all
  queued work under the lock, and complete it outside the lock with one
  diagnostic `Win32Exception`. Store that exception for `Application.Run` to
  rethrow after cleanup. Do not attempt a partial rollback that could leave
  older work without a viable wake.

### Execution and synchronization context

- Capture `ExecutionContext` at admission unless flow is suppressed.
- Before the callback starts, run it under that execution context with the
  destination `DispatcherSynchronizationContext` installed in place of the
  producer's context. A synchronous callback observes that context directly.
  An async callback captures it at each ordinary await; after the initial call
  returns its `ValueTask`, the queue processor restores the prior context.
- Complete the operation outside the queue lock and use
  `TaskCreationOptions.RunContinuationsAsynchronously`, so completion never
  invokes arbitrary caller continuations inline inside the wake-window
  procedure. This option does not choose the continuation destination: an
  awaiter may capture its own synchronization context, while `ContinueWith`
  follows its selected task scheduler.
- `DispatcherSynchronizationContext.Post` queues a fire-and-forget work item.
- `Send` runs inline only on the dispatcher thread. A cross-thread `Send` uses
  the same queue and blocks the caller without pumping. It exists for
  `SynchronizationContext` compatibility and is not the recommended API.
- Do not override `SynchronizationContext.Wait` to introduce hidden pumping.

### Shutdown

Use explicit states:

```text
Created -> Running -> Stopping -> Stopped
                   \-> Faulted
```

- `Created` exists only while `Application.Run` is initializing on its owning
  thread. Public acquisition begins at `Running`.
- Entering `Stopping` rejects new public work and cancels a dispatcher-lifetime
  token passed to active async callbacks.
- `ShutdownToken` exposes that admission-closure signal to producers. It is advisory because a token check can race
  shutdown; `InvokeAsync` and `TryPost` decide admission under the dispatcher lock.
- `Completion` completes after dispatcher-owned wake resources are released. It signals teardown completion rather than
  whether the message loop exited successfully.
- Synchronous shutdown registrations run on the UI thread in reverse
  registration order, matching dependency teardown order.
- Pending work that did not start and active async operations that did not
  finish fault with `ObjectDisposedException` for the dispatcher. Shutdown is
  not mislabeled as caller-requested cancellation.
- An internal observer continues to observe any callback task that outlives the
  dispatcher so late exceptions are observed. Its attempt to complete an
  already terminal operation is a no-op. A dispatcher synchronization-context
  post after `Stopped` fails rather than running a continuation on the wrong
  thread; the implementation spike must pin the runtime behavior of an async
  method whose captured continuation is rejected during shutdown.
- Destroy the wake HWND only after pending work is terminal and shutdown
  registrations have run. Then restore the previous synchronization context,
  clear thread-local ownership, and surface any fatal pump exception.
- A later `Application.Run` on the same thread creates a fresh context and
  dispatcher; a stopped dispatcher is never revived.

Tests must pin `ObjectDisposedException` for admission after `Stopping`, queued
work rejected by shutdown, and active async work detached during shutdown. The
message must include the dispatcher state, owning native thread ID, and
operation ID when one exists.

## Internal Architecture

### `ThreadContext`

Add an internal per-thread owner under `src/thirtytwo/Threading`:

```text
ThreadContext
  owning managed Thread and native thread ID
  outer message loop
  Dispatcher
  previous and installed SynchronizationContext
  ordered message filters
  shutdown registrations
  quit, fault, and modal state
```

Store only the owning context in `[ThreadStatic]` state. Cross-thread discovery
uses captured `Dispatcher` references, not a process-wide search by recyclable
thread ID.

`Application.Run` delegates pump ownership to `ThreadContext.Run`, while its
existing overloads and root-window disposal behavior remain compatible.

### Wake window and queue

- Create one message-only HWND owned by `ThreadContext`; do not reuse a user
  `Window`, parking window, or WinUI HWND.
- Register one private wake message. Keep at most one outstanding wake request
  regardless of queued operation count.
- Enqueue under a lock, transition `Idle` to `WakeRequested`, then call
  `PostMessage`. If posting fails, capture the last error, reacquire the lock,
  transition to `Faulted`, detach all queued work, and complete it outside the
  lock with the same `Win32Exception`.
- On wake, process a bounded number of items or bounded elapsed work, selected
  from measurement rather than an invented budget. If work remains, post one
  replacement wake before returning to USER32.
- Never hold the queue lock while invoking callbacks, completing tasks, raising
  diagnostics, or posting the native message.
- A producer racing the drain-to-empty transition must either observe an
  outstanding wake or post a new one. Stress tests must mutate this transition
  to prove no lost wake.

The core invariant while `Running` is: a nonempty queue has wake state
`WakeRequested` or `Processing`. A successful `PostMessage` that the OS never
delivers cannot be distinguished synchronously; queue-age diagnostics and the
out-of-process watchdog detect that stall. Deterministic state-machine tests
must cover resetting to `Idle` before a producer race and omitting the rearm
after a bounded drain, and each mutation must strand a sentinel task.

This combines WinForms' wake coalescing opportunity with WPF's native-input
fairness while avoiding a native message per operation and avoiding an
unbounded drain in one window procedure.

### Work items

Use internal work-item types, not a public operation hierarchy:

```text
DispatcherWorkItem
  state: Queued, Running, Succeeded, Canceled, Faulted, Rejected
  callback and captured ExecutionContext
  caller and dispatcher-lifetime cancellation
  TaskCompletionSource with RunContinuationsAsynchronously
  diagnostic ID and timestamps
```

Specialize generic result storage without using `DynamicInvoke`. The async
variant invokes the callback on the UI thread, then observes its `ValueTask`
with a non-`async void` helper using `ConfigureAwait(false)`. Completion races
use `TrySet*` and one atomic terminal-state transition.

### Message pump

The outer loop owns the processing path. Any future framework-owned nested loop
must reuse that path rather than duplicate message handling:

1. Check the thread-context quit flag before blocking and unwind without calling
  `GetMessage` when exit has been requested.
2. Call `GetMessage` and handle all three results: positive means a message,
  zero means `WM_QUIT`, and `-1` captures `Marshal.GetLastPInvokeError()` in a
  `Win32Exception`. A failure enters the same fault cleanup path and is
  rethrown from `Application.Run`; it is never preprocessed or dispatched.
3. Recheck the quit flag before dispatching a positive result, then run ordered
  global message filters.
4. Run managed target preprocessing when the HWND maps to a `Window`.
5. Call `TranslateMessage` and `DispatchMessage` when unhandled.
6. Check stored fatal dispatcher or callback exceptions at a managed boundary.

The dispatcher wake HWND is processed through normal dispatch, so work also
runs while native OLE or common-dialog modal loops dispatch window messages.
That behavior is necessary for COM and WinUI integration but is reentrant and
must be documented.

Do not expose a general public equivalent of WPF `PushFrame` initially, and do
not add an internal frame API solely for tests. Add an owned nested-loop
contract only with a production framework-dialog consumer, then test through
that integration. Native OLE and common-dialog loops already process dispatcher
work because they dispatch the message-only wake HWND.

### Delayed work and timers

Timers are a follow-up after immediate dispatch is correct:

- use `TimeProvider.GetTimestamp` or an equivalent monotonic source, never
  wrapping wall-clock or tick-count arithmetic;
- maintain a due-time min-heap rather than scan every timer;
- arm one native timer for the earliest due item;
- make delayed operations cancelable and release their callback immediately;
- enqueue due callbacks into the normal bounded dispatcher queue; and
- define repeating timers from the previous scheduled deadline, with an
  explicit missed-tick policy, rather than accumulating callback duration drift
  accidentally.

## Known-Weakness Matrix

| Existing weakness | New implementation response |
| --- | --- |
| WinForms work tied to a control handle | Dispatcher-owned message-only HWND survives user-window recreation |
| WinForms callback message drains all work | Bounded drain and one coalesced replacement wake |
| WinForms operation may be stranded by handle destruction | Thread-context lifetime plus terminal completion of every admitted task |
| WinForms pre-cancellation can return a successful default | Return a task canceled with the supplied token |
| Cancellation can complete while callback still runs | Before-start cancellation only; after-start cancellation is cooperative |
| Sync `Invoke` and pumped waits deadlock or reenter | No initial public sync invoke and no implicit pumping wait |
| Async lambda binds to sync delegate | Async-specific token/`ValueTask` overload plus compile-time binding tests or analyzer |
| WPF public priority complexity and starvation | FIFO initial contract; add scheduling lanes only with measured need and aging |
| WPF public operation-object complexity | Internal work item and ordinary TAP result |
| WPF `TaskCompletionSource` continuations can inline | `RunContinuationsAsynchronously` on every operation |
| WPF wake failure can strand the queue | Fail-stop the dispatcher and fault all queued work with diagnostics |
| WPF nested `Wait` pumps | Never expose an operation wait that pushes a frame |
| WPF dispatcher query auto-creates resources | Nullable, non-creating `Dispatcher.Current` |
| WPF timer wrap and linear scan | Monotonic time plus due-time heap |
| MAUI Task wrappers ignore failed enqueue | Admission failure always completes or rejects the returned task |
| MAUI backend semantics differ | One Windows-specific contract owned by `ThreadContext` |
| Common unbounded producer pressure | Queue depth/high-water diagnostics, bounded drain, cancellation cleanup, and stress measurement before choosing capacity policy |

## Validation Plan

### Deterministic unit tests

Construct the normal production dispatcher and use Touki `TestAccessor` from
the test assembly to replace its existing private time and wake dependencies.
Do not add production constructors, factories, overloads, or injection hooks
for deterministic tests:

- access checks on owner and foreign threads;
- `Current` before, during, and after a pump;
- `Window.Dispatcher` throws before association, resolves during `Application.Run`, and retains the same dispatcher after
  shutdown and handle destruction; active-only `Dispatcher.FromHandle(...)` returns `null` outside the running lifetime;
- cross-thread lookup from a top-level `Window`, child `Window`, and their raw HWNDs resolves the same dispatcher;
- destroyed HWNDs, foreign UI threads, faulted dispatchers, and failed duplicate startup do not leave stale discovery
  entries;
- same-thread calls queue rather than inline;
- same-thread `await InvokeAsync(...)` completes, while a synchronous wait is
  documented as unsupported and demonstrated to stall without inlining;
- FIFO and per-producer order under concurrent admission;
- one native wake for many queued operations;
- producer versus drain-to-empty races with no lost wake;
- wake-post failure before admission and while rearming remaining work;
- pre-canceled, canceled while queued, canceled after start, and canceled after
  completion;
- synchronous and async callback results and exceptions;
- async callback completion represents the full `ValueTask`;
- queue interleaving while an async callback is suspended;
- `AsyncLocal` flow and `ExecutionContext.SuppressFlow`;
- destination synchronization context replaces the producer context;
- continuations do not run inline in the wake processor;
- shutdown during queued, running synchronous, and suspended async work;
- atomic `TryPost` acceptance before shutdown and rejection after shutdown;
- `ShutdownToken` cancellation before `Completion`, with completion after resource teardown;
- every admitted task reaches exactly one terminal state;
- fire-and-forget exception handled and unhandled paths;
- repeated outer runs create independent dispatchers; and
- cancellation registrations and captured closures are released.

Mutation checks must include removing the replacement wake, changing
pre-cancellation to successful completion, completing a task before an async
callback finishes, and removing `RunContinuationsAsynchronously`. Each mutation
must make a focused test fail.

A lost-wake mutation is specifically one that leaves queued work while the
dispatcher is `Running` with wake state `Idle`, or leaves work after a bounded
drain without requesting another wake. Deterministic fake-backend tests assert
the invariant at each transition; the real USER32 watchdog covers a posted wake
that is delayed or never delivered.

### Real USER32 integration tests

Run each pump scenario in an out-of-process STA child with a watchdog:

- post before the pump blocks and wake it through the message-only HWND;
- dispatch while ordinary input and paint messages are pending;
- sustained producer load does not prevent a sentinel native message from
  being processed;
- root-window handle recreation does not affect queued work;
- root-window destruction initiates deterministic dispatcher shutdown;
- native common dialog or OLE modal loop continues to process dispatcher work;
- framework-owned nested loop exits without consuming quit for its parent;
- a framework exit request made while an external modal pump owns the thread is
  honored when control returns, even if that pump consumed `WM_QUIT`;
- `GetMessage` error injection reaches the caller after cleanup;
- a callback exception never crosses the native window-proc boundary; and
- WinUI `ContentPreTranslateMessage` and `DispatcherQueueController` coexist
  with the core queue and shut down in the recorded order.

### Performance and pressure measurements

Measure before setting budgets or capacity limits:

- enqueue cost from owner and foreign threads;
- allocation per `Action`, result, and async callback operation;
- cold and warm wake latency;
- native input latency under 1, 10, and multiple concurrent producers;
- queue memory and closure retention under canceled work;
- throughput and fairness for one-item, bounded-batch, and time-bounded drains;
- shutdown time with empty, queued, and suspended async work; and
- comparison with raw `PostMessage`, WinForms `Control.InvokeAsync`, WPF
  `Dispatcher.InvokeAsync`, and Windows App SDK `DispatcherQueue.TryEnqueue` on
  the same machine.

Do not publish a millisecond budget or queue capacity until these measurements
and a representative product scenario identify one.

## Implementation Sequence

### 1. `dispatcher-thread-context` - implemented

Deliverables:

- internal `ThreadContext` and outer-loop ownership;
- move the existing pump behind one processing method;
- correct `GetMessage` error handling;
- explicit outer-loop quit and repeated-run state; and
- focused pump and modal-state tests with no dispatcher queue yet.

Exit gate:

- existing samples retain behavior;
- main loops cannot nest accidentally;
- framework-posted quit is removed when the outer loop unwinds;
- previous synchronization context is restored on every exit; and
- no WinUI dependency enters core.

### 2. `dispatcher-task-queue` - implemented

Deliverables:

- dispatcher-owned message-only HWND;
- FIFO queue, coalesced wake state, and bounded drain;
- Task-returning synchronous callback overloads;
- access checks, diagnostics IDs, and wake-failure handling; and
- queue race and continuation-scheduling tests.

Exit gate:

- no admitted task can be stranded by a wake failure or HWND lifecycle;
- every callback runs at most once;
- native sentinel messages make progress under producer load; and
- task continuations never run under the queue lock or wake window procedure.

### 3. `dispatcher-async-context` - implemented

Deliverables:

- async `ValueTask` callback overloads;
- cancellation state machine and dispatcher lifetime token;
- execution-context flow;
- internal `DispatcherSynchronizationContext`; and
- handled/unhandled fire-and-forget exception policy.

Exit gate:

- callback tasks represent full async completion;
- cancellation after start never claims execution already stopped;
- `async` continuations resume on the UI thread unless explicitly opted out;
- shutdown completes all operation tasks; and
- compile-time tests or an analyzer address accidental `async void` overload
  binding before API stabilization.

### 4. `dispatcher-modal-shutdown` - core implemented

Deliverables:

- shutdown registrations and reverse-order teardown;
- native modal-loop dispatcher integration;
- message-filter integration;
- out-of-process common-dialog/OLE scenarios; and
- WinUI queue/filter integration against the same context.

Exit gate:

- native modal loops continue dispatching core dispatcher work;
- dispatcher work remains available through supported native modal loops;
- no callback runs after terminal shutdown;
- late async callback faults are observed; and
- WinUI cleanup runs before core wake-window destruction.

The common-dialog modal scenario is automated. An owned nested-frame contract
is deferred until a production component needs it. The WinUI-specific bullets
move with `thirtytwo.winui` and do not block review of the dependency-free core.

### 5. `dispatcher-timers-observability` - implementation complete, measurement pending

Deliverables:

- monotonic delayed work and timer heap;
- cancellation and missed-tick semantics;
- queue depth, high-water, latency, wake-failure, and shutdown tracing; and
- measured fairness and pressure report.

Exit gate:

- timer tests cover clock advancement, cancellation, repeating drift, and
  shutdown;
- no wrapping arithmetic or linear timer scan remains;
- diagnostics identify stalled or flooded queues without retaining callbacks;
  and
- evidence supports retaining FIFO only or adding a small scheduling-lane
  contract with starvation prevention.

## Completion Criteria

The dispatcher is ready for API review only when evidence answers all of these:

1. Can any thread enqueue work without depending on a user HWND?
2. Does every admitted Task complete exactly once under success, exception,
   cancellation, wake failure, and shutdown?
3. Can async callbacks yield without blocking native input, and does their
   returned Task represent their entire lifetime?
4. Are execution and synchronization contexts correct without running caller
   continuations inside the native wake callback?
5. Are same-thread ordering, cross-thread ordering, and cancellation races
   documented and pinned?
6. Do native and framework-owned nested loops preserve required dispatch while
   making reentrancy explicit?
7. Can the outer pump shut down and run again without stale TLS, HWNDs,
   registrations, or callbacks?
8. Does WinUI integration use the core dispatcher without coupling core to the
   Windows App SDK?
9. Is queue fairness acceptable under measured producer pressure without a
   speculative public priority system or arbitrary capacity limit?

No completion claim may rely only on a build, one successful callback, or
process exit cleaning up retained state.
