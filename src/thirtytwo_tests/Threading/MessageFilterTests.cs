// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Threading;

[TestClass]
public class MessageFilterTests
{
    private const uint TestMessage = Interop.WM_APP + 100;

    [STATestMethod]
    public void AddMessageFilter_SelfRemoval_AppliesToNextMessage()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        List<int> order = [];
        MessageFilterRegistration firstRegistration = default;
        int secondFilterCount = 0;

        Task<Task> setup = DispatcherTestWorker.Start(() => dispatcher.InvokeAsync(() =>
        {
            firstRegistration = Application.AddMessageFilter(new CallbackFilter(message =>
            {
                if (message == TestMessage)
                {
                    order.Add(1);
                    firstRegistration.Dispose();
                }

                return false;
            }));

            _ = Application.AddMessageFilter(new CallbackFilter(message =>
            {
                if (message == TestMessage)
                {
                    order.Add(2);
                    secondFilterCount++;
                    if (secondFilterCount == 2)
                    {
                        PInvoke.PostQuitMessage(0);
                    }
                }

                return false;
            }));

            PInvoke.PostThreadMessage(dispatcherThreadId, TestMessage, default, default);
            PInvoke.PostThreadMessage(dispatcherThreadId, TestMessage, default, default);
        }));

        context.RunMessageLoop();
    setup.GetAwaiter().GetResult().GetAwaiter().GetResult();

        order.Should().Equal(1, 2, 2);
    }

    [STATestMethod]
    public void MessageFilterRegistration_DisposeFromWrongThread_Throws()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        TaskCompletionSource<Exception?> result = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Task> setup = DispatcherTestWorker.Start(() => dispatcher.InvokeAsync(() =>
        {
            MessageFilterRegistration registration =
                Application.AddMessageFilter(new CallbackFilter(static _ => false));

            Thread worker = new(() =>
            {
                try
                {
                    registration.Dispose();
                    result.SetResult(null);
                }
                catch (Exception exception)
                {
                    result.SetResult(exception);
                }
                finally
                {
                    PInvoke.PostThreadMessage(dispatcherThreadId, Interop.WM_QUIT, default, default);
                }
            });

            worker.Start();
        }));

        context.RunMessageLoop();
        setup.GetAwaiter().GetResult().GetAwaiter().GetResult();

        result.Task.GetAwaiter().GetResult().Should().BeOfType<InvalidOperationException>();
    }

    private sealed class CallbackFilter(Func<uint, bool> callback) : IMessageFilter
    {
        public bool PreFilterMessage(ref MSG message) => callback(message.message);
    }
}
