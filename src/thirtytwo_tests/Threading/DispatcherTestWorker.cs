// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Threading;

internal static class DispatcherTestWorker
{
    private const uint StartMessage = Interop.WM_APP + 102;

    internal static Task Start(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return Start(() =>
        {
            callback();
            return true;
        });
    }

    internal static Task<TResult> Start<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<TResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        MessageFilterRegistration registration = default;
        registration = Application.AddMessageFilter(new StartMessageFilter(() =>
        {
            registration.Dispose();
            started.TrySetResult();
        }));

        if (!PInvoke.PostThreadMessage(dispatcherThreadId, StartMessage, default, default))
        {
            registration.Dispose();
            throw new InvalidOperationException("Failed to post the dispatcher test startup message.");
        }

        Thread worker = new(() =>
        {
            try
            {
                started.Task.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                completion.SetResult(callback());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
                PInvoke.PostThreadMessage(dispatcherThreadId, Interop.WM_QUIT, default, default);
            }
        });

        worker.Start();
        return completion.Task;
    }

    private sealed class StartMessageFilter(Action callback) : IMessageFilter
    {
        public bool PreFilterMessage(ref MSG message)
        {
            if (message.message != StartMessage)
            {
                return false;
            }

            callback();
            return true;
        }
    }
}