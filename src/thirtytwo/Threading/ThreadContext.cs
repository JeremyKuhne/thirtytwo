// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.ExceptionServices;
using Windows.Support;

namespace Windows.Threading;

/// <summary>
///  Owns the message pump, dispatcher, message filters, and shutdown callbacks for one UI thread.
/// </summary>
/// <remarks>
///  <para>
///   A thread can have at most one context. <see cref="Application.Run(Window, bool)"/> creates and disposes the
///   context that owns its outer message loop.
///  </para>
/// </remarks>
internal sealed unsafe class ThreadContext : DisposableBase
{
    // Associates one context with its owner thread from successful construction until disposal.
    [ThreadStatic]
    private static ThreadContext? t_current;

    // Retains message filters in registration order; message traversal reads the stable snapshot below.
    private readonly List<MessageFilterEntry> _messageFilters = [];

    // Retains shutdown callbacks in registration order so teardown can invoke them in reverse order.
    private readonly List<ShutdownCallbackEntry> _shutdownCallbacks = [];

    // Captures the managed owner thread used to reject cross-thread context mutation.
    private readonly Thread _thread = Thread.CurrentThread;

    // Indicates whether _messageFilterSnapshot still represents the current registration list.
    private bool _filterSnapshotValid;

    // Tracks only a WM_QUIT posted by RequestExit so an unconsumed framework message can be removed before reuse.
    private bool _ownedQuitMessagePending;

    // Requests that the outer loop stop before blocking for or dispatching another message.
    private bool _quitRequested;

    // Spans message pumping and shutdown callbacks, preventing a nested run and exposing the current dispatcher.
    private bool _running;

    // Records the first RunMessageLoop entry so this context cannot own a second outer loop.
    private bool _hasRun;

    // Becomes true before callbacks are detached, freezing the callback set for shutdown.
    private bool _shutdownStarted;

    // Generates identifiers shared by filter and shutdown registrations within this context.
    private long _nextRegistrationId;

    // Provides a stable filter traversal when a callback adds or removes registrations.
    private MessageFilterEntry[] _messageFilterSnapshot = [];

    private ThreadContext()
    {
        t_current = this;

        try
        {
            Dispatcher = new Dispatcher();
        }
        catch
        {
            t_current = null;
            throw;
        }
    }

    /// <summary>
    ///  Gets the dispatcher owned by this context.
    /// </summary>
    internal Dispatcher Dispatcher { get; }

    /// <summary>
    ///  Gets the non-disposed context associated with the current thread, or <see langword="null"/>.
    /// </summary>
    internal static ThreadContext? CurrentContext => t_current is { Disposed: false } context
        ? context
        : null;

    /// <summary>
    ///  Gets the dispatcher for the running context on the current thread, or <see langword="null"/>.
    /// </summary>
    internal static Dispatcher? CurrentDispatcher => t_current is { _running: true, _shutdownStarted: false } context
        ? context.Dispatcher
        : null;

    /// <summary>
    ///  Creates the context for the current thread.
    /// </summary>
    /// <returns>The newly created thread context.</returns>
    /// <exception cref="InvalidOperationException">The current thread already has a context.</exception>
    internal static ThreadContext Create()
    {
        if (t_current is not null)
        {
            throw new InvalidOperationException("A message loop is already running on this thread.");
        }

        return new ThreadContext();
    }

    /// <summary>
    ///  Starts the dispatcher, invokes initialization, and runs the outer message loop until quit, dispatcher fault,
    ///  or message retrieval failure.
    /// </summary>
    /// <param name="initialize">Initialization to run before the message loop starts blocking.</param>
    internal void RunMessageLoop(Action? initialize)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_running)
        {
            throw new InvalidOperationException("A message loop is already running on this thread.");
        }

        if (_hasRun)
        {
            throw new InvalidOperationException("This thread context has already run its message loop.");
        }

        _hasRun = true;
        _running = true;
        SynchronizationContext? previousSynchronizationContext = SynchronizationContext.Current;

        // Preserve pump and teardown failures independently so cleanup never hides the original failure.
        ExceptionDispatchInfo? pumpFailure = null;
        Exception? cleanupFailure = null;

        try
        {
            Dispatcher.Start();
            SynchronizationContext.SetSynchronizationContext(Dispatcher.SynchronizationContext);
            Window.AttachDispatcherToCurrentThread(Dispatcher);
            initialize?.Invoke();
            RunMessageLoopCore();
        }
        catch (Exception exception)
        {
            pumpFailure = ExceptionDispatchInfo.Capture(exception);
        }
        finally
        {
            _shutdownStarted = true;

            try
            {
                // Reject new work before callbacks tear down components that may own queued dispatcher work.
                Dispatcher.BeginShutdown();
            }
            catch (Exception exception)
            {
                cleanupFailure = exception;
            }

            try
            {
                RunShutdownCallbacks();
            }
            catch (Exception exception)
            {
                cleanupFailure = cleanupFailure is null
                    ? exception
                    : new AggregateException(cleanupFailure, exception);
            }

            try
            {
                Dispatcher.Stop();
            }
            catch (Exception exception)
            {
                cleanupFailure = cleanupFailure is null
                    ? exception
                    : new AggregateException(cleanupFailure, exception);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext);
                _running = false;
            }
        }

        if (pumpFailure is not null)
        {
            if (cleanupFailure is not null)
            {
                throw new AggregateException(pumpFailure.SourceException, cleanupFailure);
            }

            pumpFailure.Throw();
        }

        if (cleanupFailure is not null)
        {
            ExceptionDispatchInfo.Capture(cleanupFailure).Throw();
        }

        void RunShutdownCallbacks()
        {
            ShutdownCallbackEntry[] callbacks = [.. _shutdownCallbacks];
            _shutdownCallbacks.Clear();
            List<Exception>? exceptions = null;

            for (int index = callbacks.Length - 1; index >= 0; index--)
            {
                try
                {
                    callbacks[index].Callback();
                }
                catch (Exception exception)
                {
                    (exceptions ??= []).Add(exception);
                }
            }

            if (exceptions is not null)
            {
                throw new AggregateException(exceptions);
            }
        }

        void RunMessageLoopCore()
        {
            try
            {
                while (!_quitRequested)
                {
                    BOOL result = PInvoke.GetMessage(out MSG message, HWND.Null, 0, 0);
                    if ((int)result == -1)
                    {
                        Error.GetLastError().ThrowThirtyTwoException();
                    }

                    if (!result)
                    {
                        _quitRequested = true;
                        _ownedQuitMessagePending = false;
                        Dispatcher.ThrowIfFaulted();
                        break;
                    }

                    if (_quitRequested)
                    {
                        break;
                    }

                    if (PreFilterMessage(ref message))
                    {
                        continue;
                    }

                    if (Window.FromHandle(message.hwnd) is { } target && target.PreProcessMessage(ref message))
                    {
                        continue;
                    }

                    PInvoke.TranslateMessage(&message);
                    PInvoke.DispatchMessage(&message);
                    Dispatcher.ThrowIfFaulted();
                }
            }
            finally
            {
                if (_quitRequested && _ownedQuitMessagePending)
                {
                    // RequestExit usually makes the loop stop before GetMessage consumes our WM_QUIT. Remove it so a
                    // later Application.Run on this thread does not inherit a stale quit message.
                    _ = PInvoke.PeekMessage(
                        out _,
                        HWND.Null,
                        Interop.WM_QUIT,
                        Interop.WM_QUIT,
                        PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);

                    _ownedQuitMessagePending = false;
                }
            }

            bool PreFilterMessage(ref MSG message)
            {
                if (!_filterSnapshotValid)
                {
                    _messageFilterSnapshot = [.. _messageFilters];
                    _filterSnapshotValid = true;
                }

                // The array isolates this traversal. Filter changes invalidate only the next message's snapshot.
                foreach (MessageFilterEntry entry in _messageFilterSnapshot)
                {
                    if (entry.Filter.PreFilterMessage(ref message))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    /// <summary>
    ///  Requests exit from the message loop and posts one <c>WM_QUIT</c> wakeup.
    /// </summary>
    /// <remarks>
    ///  <para>This method must be called by the owning thread.</para>
    /// </remarks>
    private void RequestExit()
    {
        VerifyAccess();

        if (_quitRequested)
        {
            return;
        }

        _quitRequested = true;
        PInvoke.PostQuitMessage(0);
        _ownedQuitMessagePending = true;
    }

    /// <summary>
    ///  Requests exit from the current thread's context, or posts <c>WM_QUIT</c> when no context is running.
    /// </summary>
    internal static void RequestExitCurrentThread()
    {
        if (t_current is { _running: true } context)
        {
            context.RequestExit();
        }
        else
        {
            PInvoke.PostQuitMessage(0);
        }
    }

    /// <summary>
    ///  Adds a message filter to the owning thread in registration order.
    /// </summary>
    /// <param name="filter">The filter to invoke before managed window lookup and native dispatch.</param>
    /// <returns>A registration that removes the filter when disposed.</returns>
    internal MessageFilterRegistration AddMessageFilter(IMessageFilter filter)
    {
        VerifyAccess();
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_shutdownStarted || (_hasRun && !_running))
        {
            throw new InvalidOperationException("Thread context shutdown has started.");
        }

        long id = ++_nextRegistrationId;
        _messageFilters.Add(new(id, filter));
        _filterSnapshotValid = false;
        return new MessageFilterRegistration(this, id);
    }

    /// <summary>
    ///  Removes a message filter by registration identifier.
    /// </summary>
    /// <param name="id">The registration identifier.</param>
    internal void RemoveMessageFilter(long id)
    {
        VerifyAccess();

        if (Disposed)
        {
            return;
        }

        int index = _messageFilters.FindIndex(entry => entry.Id == id);
        if (index >= 0)
        {
            _messageFilters.RemoveAt(index);
            _filterSnapshotValid = false;
        }
    }

    /// <summary>
    ///  Registers synchronous cleanup to run in reverse registration order before the dispatcher stops.
    /// </summary>
    /// <param name="callback">The cleanup callback to run on the owning thread.</param>
    /// <returns>A registration that removes the callback when disposed before shutdown.</returns>
    internal ShutdownRegistration RegisterShutdownCallback(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        VerifyAccess();

        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_shutdownStarted || (_hasRun && !_running))
        {
            throw new InvalidOperationException("Thread context shutdown has started.");
        }

        long id = ++_nextRegistrationId;
        _shutdownCallbacks.Add(new(id, callback));
        return new ShutdownRegistration(this, id);
    }

    /// <summary>
    ///  Removes a shutdown callback by registration identifier when shutdown has not started.
    /// </summary>
    /// <param name="id">The registration identifier.</param>
    internal void RemoveShutdownCallback(long id)
    {
        VerifyAccess();

        if (Disposed || _shutdownStarted)
        {
            return;
        }

        int index = _shutdownCallbacks.FindIndex(entry => entry.Id == id);
        if (index >= 0)
        {
            _shutdownCallbacks.RemoveAt(index);
        }
    }

    private void VerifyAccess()
    {
        if (!ReferenceEquals(Thread.CurrentThread, _thread))
        {
            throw new InvalidOperationException("The calling thread does not own this thread context.");
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The message loop is still running.</exception>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (_running)
        {
            throw new InvalidOperationException("The thread context cannot be disposed while its message loop is running.");
        }

        try
        {
            Dispatcher.Dispose();
        }
        finally
        {
            if (ReferenceEquals(t_current, this))
            {
                t_current = null;
            }
        }
    }
}
