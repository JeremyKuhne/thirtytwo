// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Dialogs;
using Windows.Messages;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.Threading;

[TestClass]
public unsafe class DispatcherModalTests
{
    [STATestMethod]
    public void InvokeAsync_FileDialogModalLoop_ExecutesCallback()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        using Window window = new(Window.DefaultBounds);
        using FileOpenDialog dialog = new(window);
        bool callbackRan = false;
        bool callbackQueued = false;
        Timer? watchdog = null;

        Task<Task> queued = DispatcherTestWorker.Start(() => dispatcher.InvokeAsync(() =>
        {
            EnterIdleHandler.Attach(window, (bool isDialog, HWND dialogWindow) =>
            {
                if (callbackQueued)
                {
                    return;
                }

                callbackQueued = true;
                watchdog = new Timer(
                    static state =>
                    {
                        HWND hwnd = (HWND)(nint)state!;
                        _ = PInvoke.PostMessage(hwnd, Interop.WM_CLOSE, default, default);
                    },
                    (nint)dialogWindow.Value,
                    TimeSpan.FromSeconds(5),
                    Timeout.InfiniteTimeSpan);

                _ = dispatcher.InvokeAsync(() =>
                {
                    callbackRan = true;
                    dialogWindow.SendMessage(MessageType.Close);
                });
            });

            dialog.ShowDialog().Should().BeFalse();
            watchdog?.Dispose();
            context.RequestExit();
        }));

        context.RunMessageLoop();
        queued.GetAwaiter().GetResult().GetAwaiter().GetResult();

        callbackRan.Should().BeTrue();
    }
}
