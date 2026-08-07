# Dispatching to the UI Thread

Each window and control belongs to the thread that created it, called the UI
thread. Windows runs input, painting, and lifetime message handlers on that
thread. Requiring all access to UI objects on that same thread is their
synchronization contract: message handlers and dispatched callbacks are
serialized, so UI state is not changed concurrently by a worker thread while a
message handler is actively using it. Worker threads must queue UI access to
the owning thread rather than access UI objects directly.

The UI thread must also return to its message loop promptly. Slow or blocking
work prevents it from processing input and painting, making the application
appear frozen. Run such work elsewhere, then queue only the UI update back.

Thirtytwo provides one `Dispatcher` for each active `Application.Run` message
loop. It performs that handoff by queueing a callback on the UI thread. A
window retains its dispatcher as a stable affinity reference; whether the
dispatcher still accepts work is decided atomically when work is submitted.

## In this guide

- **[Basic usage](#basic-usage):** [get a dispatcher](#get-a-dispatcher),
  [queue a synchronous callback](#queue-a-synchronous-callback),
  [queue an asynchronous callback](#queue-an-asynchronous-callback), and
  [return a value](#return-a-value).
- **[Recipes](#recipes):** [cancel work](#cancel-work),
    [post a best-effort update](#post-a-best-effort-update),
  [schedule delayed work](#schedule-delayed-work),
  [run a repeating timer](#run-a-repeating-timer),
    and [handle fire-and-forget exceptions](#handle-fire-and-forget-exceptions).
- **[Optimizations](#optimizations):**
  [skip dispatch when already on the UI thread](#skip-dispatch-when-already-on-the-ui-thread),
  [enforce UI-thread access](#enforce-ui-thread-access), and
  [reduce queue pressure](#reduce-queue-pressure).
- **[Shutdown behavior](#shutdown-behavior):** [developer summary](#developer-summary),
    [lifecycle signals](#lifecycle-signals), [submission races](#submission-during-shutdown),
    and [outstanding work](#work-outstanding-at-shutdown).

## Basic usage

### Get a dispatcher

A window retains the dispatcher for its owning UI thread. The property may be
read from any thread and does not become `null` when shutdown starts:

```csharp
using Windows.Threading;

Dispatcher dispatcher = window.Dispatcher;
```

`Application.Run` associates existing windows before showing the root window.
Its factory overload also makes the dispatcher available while the root window
is being constructed. Reading `Window.Dispatcher` before either association
throws `InvalidOperationException`.

When code has no window, `Dispatcher.Current` discovers the active dispatcher
for the calling thread:

```csharp
Dispatcher? dispatcher = Dispatcher.Current;
```

Code that has only a raw `HWND` can discover its owning thread's active
dispatcher:

```csharp
Dispatcher? dispatcher = Dispatcher.FromHandle(windowHandle);
```

`Dispatcher.Current` and `Dispatcher.FromHandle` are optional discovery APIs.
They return `null` before the message loop starts or after admission closes;
`FromHandle` also returns `null` for a destroyed or invalid handle. Never use a
successful lookup as a shutdown check. A captured dispatcher remains safe to
call, and submission reports whether shutdown won the race.

### Queue a synchronous callback

Use `InvokeAsync(Action)` for work whose callback completes synchronously.
The call queues the callback and returns immediately; await its task to observe
completion and exceptions:

```csharp
await dispatcher.InvokeAsync(
    () => window.SetWindowText("Ready"));
```

`InvokeAsync` always queues the callback, even when called by the dispatcher
thread. Immediate callbacks run in FIFO admission order. Do not synchronously
wait on the returned task from the dispatcher thread: the queued callback
cannot run while that thread is blocked. Use `await` instead.

### Queue an asynchronous callback

Use an async callback overload when the UI operation itself must await other
work. The returned task represents the callback's complete lifetime, and an
ordinary `await` inside the callback resumes on the dispatcher thread:

```csharp
await dispatcher.InvokeAsync(async effectiveToken =>
{
    await Task.Delay(TimeSpan.FromSeconds(1), effectiveToken);
    window.SetWindowText("Async work complete");
});
```

The effective token observes dispatcher shutdown. When the caller also supplies
a cancellation token, it observes caller cancellation as well.

Exceptions from synchronous and asynchronous callbacks are stored on the
returned task and re-thrown by `await`.

### Return a value

Use a generic overload to return a synchronous or asynchronous result from the
UI thread:

```csharp
int clientWidth = await dispatcher.InvokeAsync(
    () => window.GetClientRectangle().Width);
```

```csharp
string title = await dispatcher.InvokeAsync(async effectiveToken =>
{
    await Task.Delay(TimeSpan.FromMilliseconds(100), effectiveToken);
    return window.GetWindowText();
});
```

## Recipes

### Cancel work

Pass a cancellation token as the final argument:

```csharp
using CancellationTokenSource cancellationSource = new();

Task operation = dispatcher.InvokeAsync(
    async effectiveToken =>
    {
        await Task.Delay(TimeSpan.FromSeconds(10), effectiveToken);
        window.SetWindowText("Finished");
    },
    cancellationSource.Token);

cancellationSource.Cancel();
await operation;
```

Cancellation before a callback starts prevents it from running. After an async
callback starts, cancellation is cooperative: the task does not complete until
the callback exits. A running synchronous callback is not interrupted.

### Post a best-effort update

Use `TryPost` when an update may be dropped during shutdown and no caller needs
its completion or result:

```csharp
bool accepted = window.Dispatcher.TryPost(
    () => window.SetWindowText("Background update"));

if (!accepted)
{
    // Dispatcher shutdown already closed admission. The update was not queued.
}
```

The Boolean is the atomic admission result, not a dispatcher-state pre-check.
`true` means the callback entered the queue, although shutdown may still discard
it before execution. Callback exceptions are reported through
`UnhandledException`. Use `InvokeAsync` instead when completion matters.

### Schedule delayed work

Pass a `TimeSpan` before the callback to make it eligible after a monotonic
delay:

```csharp
await dispatcher.InvokeAsync(
    TimeSpan.FromMilliseconds(250),
    () => window.SetWindowText("Delay complete"));
```

Synchronous, result-returning, and asynchronous callback shapes have delayed
overloads. Delayed callbacks are ordered by deadline and then by admission
order when their deadlines are equal. The ordinary cancellation, completion,
and exception rules apply.

### Run a repeating timer

Create, start, stop, and dispose a `DispatcherTimer` on its dispatcher thread.
Retain the timer for as long as it should run:

```csharp
private DispatcherTimer? _clockTimer;

private void StartClock(Window window)
{
    Dispatcher dispatcher = window.Dispatcher;

    _clockTimer = dispatcher.CreateTimer(TimeSpan.FromSeconds(1));
    _clockTimer.Tick += (_, _) =>
        window.SetWindowText(DateTime.Now.ToLongTimeString());
    _clockTimer.Start();
}

private void StopClock()
{
    _clockTimer?.Dispose();
    _clockTimer = null;
}
```

Changing `Interval` while the timer is running restarts its deadline from the
current time. A timer skips missed intervals rather than replaying a burst. It
is stopped during application shutdown, but code should still dispose it when
its owner no longer needs it.

### Handle fire-and-forget exceptions

Exceptions from `InvokeAsync` belong to the returned task. The
`UnhandledException` event is for fire-and-forget work, such as a callback
posted through the installed `SynchronizationContext`:

```csharp
dispatcher.UnhandledException += (_, arguments) =>
{
    System.Diagnostics.Debug.WriteLine(arguments.Exception);
    arguments.Handled = true;
};
```

Set `Handled` only when the application can continue safely. An unhandled
exception stops the dispatcher and is re-thrown from `Application.Run` after
cleanup.

## Optimizations

### Skip dispatch when already on the UI thread

Use `CheckAccess` when a method can safely run inline on the UI thread and only
needs dispatching for callers on other threads:

```csharp
static async Task SetTitleAsync(Window window, string title)
{
    Dispatcher dispatcher = window.Dispatcher;

    if (dispatcher.CheckAccess())
    {
        window.SetWindowText(title);
        return;
    }

    await dispatcher.InvokeAsync(() => window.SetWindowText(title));
}
```

Running inline avoids one queue operation, but it also changes ordering and
reentrancy compared with always calling `InvokeAsync`. Use this optimization
only when immediate execution on the UI thread is part of the method's
contract.

### Enforce UI-thread access

Use `VerifyAccess` at the start of a method that may only be called by the
dispatcher thread:

```csharp
private void UpdateUiState()
{
    _dispatcher.VerifyAccess();
    // Read or update UI-bound state.
}
```

It returns normally on the dispatcher thread and throws
`InvalidOperationException` from another thread. This check exposes an invalid
caller early; it does not marshal the call.

### Reduce queue pressure

The dispatcher queue has no fixed capacity or backpressure. When a producer can
outpace the UI thread:

- combine related UI changes into one callback;
- avoid posting an update when a newer one replaces it;
- throttle high-frequency producers; and
- await operations when later production depends on their completion.

These measures bound retained callback state and leave the UI thread time to
process input, painting, and other native messages.

## Shutdown behavior

### Developer summary

For most dispatched code, these are the rules that matter:

- Keep the dispatcher reference. Do not check whether it is active before
    submitting work; submission itself safely resolves a race with shutdown.
- Await every `InvokeAsync` task. If shutdown prevents the callback from
    completing, the task faults with `ObjectDisposedException`.
- Use `TryPost` only when it is acceptable for the callback never to run. A
    `false` result means shutdown rejected it; even an accepted callback may be
    discarded by later teardown.
- Once a synchronous callback starts, it runs to completion. Keep it short so
    the UI remains responsive.
- Async callbacks should honor their effective cancellation token. Do not rely
    on code after an `await`, including a UI-thread `finally`, running during
    shutdown. Leave UI state consistent before yielding and put required lifetime
    cleanup in the object that owns the resource.

The remaining sections define the lifecycle and race behavior behind these
rules.

Shutdown starts when the `Application.Run` message loop exits or the dispatcher
faults. At that point the dispatcher stops accepting work and is removed from
`Dispatcher.Current` and `Dispatcher.FromHandle` discovery. A previously
associated `Window.Dispatcher` still returns the same dispatcher, including
after the window's handle is destroyed. If a window survives into a later
`Application.Run` on the same thread, that run associates it with its fresh
dispatcher after the previous dispatcher's `Completion` task has completed.

### Lifecycle signals

Use the dispatcher's two stable lifetime signals for coordination:

```csharp
CancellationToken shutdownToken = dispatcher.ShutdownToken;
Task completion = dispatcher.Completion;
```

- `ShutdownToken` is canceled when admission closes. Background producers can
    use it to stop generating updates, and asynchronous dispatcher callbacks
    receive an effective token that includes this signal.
- `Completion` completes after dispatcher shutdown releases its native wake
    resources. It signals that teardown finished; it does not report whether the
    message loop ended successfully.

A token check can race shutdown, so it is advisory rather than permission to
enqueue. Submit the operation and use the returned task or Boolean as the
authoritative result.

### Submission during shutdown

`InvokeAsync` and `TryPost` synchronize admission with shutdown under the same
lock:

- If submission wins, the operation is admitted. An `InvokeAsync` task tracks
    what subsequently happens. `TryPost` returns `true`.
- If shutdown wins, `InvokeAsync` returns an already-faulted task containing an
    `ObjectDisposedException`. `TryPost` returns `false`.

Do not write `if (!dispatcher.ShutdownToken.IsCancellationRequested)` or check
some other status before submitting work; shutdown can begin immediately after
that check.

### Work outstanding at shutdown

- A synchronous callback already running on the UI thread finishes before
    normal shutdown can proceed.
- An asynchronous callback is asked to stop through its effective cancellation
    token. If it finishes before final teardown, its task records its actual
    result, exception, or cooperative cancellation.
- Queued, delayed, or still-active asynchronous work detached by final teardown
    faults with `ObjectDisposedException`. A late async completion is observed but
    cannot change that terminal task.
- Fire-and-forget work discarded during shutdown has no completion result.

Always observe tasks returned by `InvokeAsync`. Use `ShutdownToken` to stop
producers promptly and `Completion` when an external coordinator must wait for
the dispatcher to release its resources.