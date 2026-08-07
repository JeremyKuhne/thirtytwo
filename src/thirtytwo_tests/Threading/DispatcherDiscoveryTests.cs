// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.Threading;

[TestClass]
public class DispatcherDiscoveryTests
{
    [STATestMethod]
    public void WindowDispatcher_BeforeAssociation_Throws()
    {
        using Window window = new(Window.DefaultBounds);

        Action getDispatcher = () => _ = window.Dispatcher;

        getDispatcher.Should().Throw<InvalidOperationException>()
            .WithMessage("The window has not been associated with a dispatcher.");
        Dispatcher.FromHandle(window).Should().BeNull();
        Dispatcher.FromHandle(window.Handle).Should().BeNull();
        Dispatcher.FromHandle(HWND.Null).Should().BeNull();
    }

    [STATestMethod]
    public void FromHandle_DestroyedWindow_ReturnsNull()
    {
        HWND handle;
        using (Window window = new(Window.DefaultBounds))
        {
            handle = window.Handle;
        }

        Dispatcher.FromHandle(handle).Should().BeNull();
    }

    [STATestMethod]
    public void FromHandle_ForeignUiThreadWithoutDispatcher_ReturnsNull()
    {
        TaskCompletionSource<HWND> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread thread = new(() =>
        {
            try
            {
                using Window window = new(Window.DefaultBounds);
                created.SetResult(window.Handle);
                release.Task.GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                created.TrySetException(exception);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        try
        {
            HWND handle = created.Task.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            Dispatcher.FromHandle(handle).Should().BeNull();
        }
        finally
        {
            release.TrySetResult();
            thread.Join();
        }
    }

    [STATestMethod]
    public void FromHandle_ApplicationRun_ResolvesOwningDispatcher()
    {
        using Window window = new(Window.DefaultBounds);
        using Window child = new(Window.DefaultBounds, parentWindow: window);
        HWND windowHandle = window.Handle;
        HWND childHandle = child.Handle;
        uint dispatcherThreadId = PInvoke.GetCurrentThreadId();
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher? resolvedDispatcher = null;

        Action getDispatcher = () => _ = window.Dispatcher;
        getDispatcher.Should().Throw<InvalidOperationException>();

        Thread worker = new(() =>
        {
            try
            {
                if (!SpinWait.SpinUntil(
                    () => (resolvedDispatcher = Dispatcher.FromHandle(windowHandle)) is not null,
                    TimeSpan.FromSeconds(5)))
                {
                    throw new TimeoutException("The window dispatcher did not become available.");
                }

                Dispatcher dispatcher = resolvedDispatcher
                    ?? throw new InvalidOperationException("Dispatcher lookup succeeded without a result.");
                dispatcher.CheckAccess().Should().BeFalse();
                window.Dispatcher.Should().BeSameAs(dispatcher);
                Dispatcher.FromHandle(childHandle).Should().BeSameAs(dispatcher);

                dispatcher.InvokeAsync(() =>
                {
                    dispatcher.CheckAccess().Should().BeTrue();
                    Dispatcher.Current.Should().BeSameAs(dispatcher);
                    window.Dispatcher.Should().BeSameAs(dispatcher);
                    child.Dispatcher.Should().BeSameAs(dispatcher);
                    Dispatcher.FromHandle(window).Should().BeSameAs(dispatcher);
                    Dispatcher.FromHandle(windowHandle).Should().BeSameAs(dispatcher);
                    Dispatcher.FromHandle(childHandle).Should().BeSameAs(dispatcher);
                    PInvoke.PostQuitMessage(0);
                }).GetAwaiter().GetResult();

                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
                PInvoke.PostThreadMessage(dispatcherThreadId, Interop.WM_QUIT, default, default);
            }
        });

        worker.Start();
        Application.Run(window, disposeWindow: false);
        completion.Task.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        worker.Join();

        window.Handle.Should().Be(windowHandle);
        window.Dispatcher.Should().BeSameAs(resolvedDispatcher);
        child.Dispatcher.Should().BeSameAs(resolvedDispatcher);
        resolvedDispatcher!.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        resolvedDispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
        resolvedDispatcher.TryPost(static () => { }).Should().BeFalse();
        Dispatcher.FromHandle(windowHandle).Should().BeNull();
        Dispatcher.FromHandle(childHandle).Should().BeNull();
    }

    [STATestMethod]
    public void ApplicationRun_WindowFactory_HasActiveDispatcher()
    {
        Dispatcher? factoryDispatcher = null;
        Dispatcher? windowDispatcher = null;
        Window? createdWindow = null;

        Application.Run(() =>
        {
            factoryDispatcher = Dispatcher.Current;
            createdWindow = new Window(Window.DefaultBounds);
            windowDispatcher = createdWindow.Dispatcher;
            createdWindow.PostMessage(MessageType.Close);
            return createdWindow;
        });

        factoryDispatcher.Should().NotBeNull();
        windowDispatcher.Should().BeSameAs(factoryDispatcher);
        createdWindow.Should().NotBeNull();
        createdWindow!.Handle.Should().Be(HWND.Null);
        createdWindow.Dispatcher.Should().BeSameAs(factoryDispatcher);
        factoryDispatcher!.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        factoryDispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
    }

    [STATestMethod]
    public void ApplicationRun_SurvivingWindow_ReassociatesAfterCompletion()
    {
        using Window window = new(Window.DefaultBounds);
        PInvoke.PostQuitMessage(0);

        Application.Run(window, disposeWindow: false);
        Dispatcher firstDispatcher = window.Dispatcher;

        firstDispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
        firstDispatcher.TryPost(static () => { }).Should().BeFalse();

        PInvoke.PostQuitMessage(0);
        Application.Run(window, disposeWindow: false);
        Dispatcher secondDispatcher = window.Dispatcher;

        secondDispatcher.Should().NotBeSameAs(firstDispatcher);
        secondDispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
        secondDispatcher.TryPost(static () => { }).Should().BeFalse();
    }

    [STATestMethod]
    public void ApplicationRun_WindowFactoryThrows_CompletesDispatcherAndClearsCurrent()
    {
        Dispatcher? dispatcher = null;

        Action run = () => Application.Run(() =>
        {
            dispatcher = Dispatcher.Current;
            throw new InvalidOperationException("Expected");
        });

        run.Should().Throw<InvalidOperationException>().WithMessage("Expected");
        dispatcher.Should().NotBeNull();
        dispatcher!.ShutdownToken.IsCancellationRequested.Should().BeTrue();
        dispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
        Dispatcher.Current.Should().BeNull();

        Application.Run(() =>
        {
            Window window = new(Window.DefaultBounds);
            window.PostMessage(MessageType.Close);
            return window;
        });
    }

    [STATestMethod]
    public void ApplicationRun_WindowFactoryReturnsNull_ThrowsAndClearsCurrent()
    {
        Action run = () => Application.Run(static () => null!);

        run.Should().Throw<InvalidOperationException>().WithMessage("The window factory returned null.");
        Dispatcher.Current.Should().BeNull();

        Application.Run(() =>
        {
            Window window = new(Window.DefaultBounds);
            window.PostMessage(MessageType.Close);
            return window;
        });
    }
}
