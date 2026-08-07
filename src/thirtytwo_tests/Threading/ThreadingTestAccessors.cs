// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

internal static class ThreadingTestAccessors
{
    private delegate void RunMessageLoopDelegate(Action? initialize);

    private static readonly Func<ThreadContext> s_createThreadContext =
        typeof(ThreadContext).TestAccessor.CreateDelegate<Func<ThreadContext>>("Create");

    internal static ThreadContext CreateThreadContext(
        TimeProvider? timeProvider = null,
        Func<Dispatcher, IDispatcherWake>? wakeFactory = null)
    {
        ThreadContext context = s_createThreadContext();

        try
        {
            ConfigureDispatcher(context.Dispatcher, timeProvider, wakeFactory);
            return context;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    internal static Dispatcher CreateDispatcher(
        TimeProvider? timeProvider = null,
        Func<Dispatcher, IDispatcherWake>? wakeFactory = null)
    {
        Dispatcher dispatcher = new();

        try
        {
            ConfigureDispatcher(dispatcher, timeProvider, wakeFactory);
            return dispatcher;
        }
        catch
        {
            dispatcher.Dispose();
            throw;
        }
    }

    internal static void AttachDispatcher(Window window, Dispatcher dispatcher)
        => window.TestAccessor.CreateDelegate<Action<Dispatcher>>("AttachDispatcher")(dispatcher);

    internal static void RunMessageLoop(this ThreadContext context)
        => context.TestAccessor.CreateDelegate<RunMessageLoopDelegate>("RunMessageLoop")(null);

    internal static void RequestExit(this ThreadContext context)
        => context.TestAccessor.CreateDelegate<Action>("RequestExit")();

    private static void ConfigureDispatcher(
        Dispatcher dispatcher,
        TimeProvider? timeProvider,
        Func<Dispatcher, IDispatcherWake>? wakeFactory)
    {
        dynamic accessor = dispatcher.TestAccessor.Dynamic;

        if (timeProvider is not null)
        {
            accessor._timeProvider = timeProvider;
            accessor._timestampOrigin = timeProvider.GetTimestamp();
        }

        if (wakeFactory is not null)
        {
            IDispatcherWake replacement = wakeFactory(dispatcher);
            IDispatcherWake original = accessor._wake;
            accessor._wake = replacement;
            original.Dispose();
        }
    }
}