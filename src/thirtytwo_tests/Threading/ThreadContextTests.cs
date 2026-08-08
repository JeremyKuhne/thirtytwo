// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Threading;

[TestClass]
public class ThreadContextTests
{
    [TestMethod]
    public void Create_ExistingContext_Throws()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();

        Action action = () => ThreadingTestAccessors.CreateThreadContext();

        action.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void Create_AfterDispose_Succeeds()
    {
        ThreadingTestAccessors.CreateThreadContext().Dispose();

        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
    }

    [STATestMethod]
    public void RunMessageLoop_QuitPosted_Returns()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        PInvoke.PostQuitMessage(0);

        context.RunMessageLoop();
    }

    [STATestMethod]
    public void RunMessageLoop_PreviousSynchronizationContext_Restores()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        SynchronizationContext previous = new();
        SynchronizationContext.SetSynchronizationContext(previous);

        try
        {
            PInvoke.PostQuitMessage(0);
            context.RunMessageLoop();

            SynchronizationContext.Current.Should().BeSameAs(previous);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    [STATestMethod]
    public void RegisterShutdownCallback_MultipleCallbacks_RunInReverseOrder()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        List<int> order = [];
        context.RegisterShutdownCallback(() =>
        {
            Dispatcher.Current.Should().BeNull();
            Action getResult = () => context.Dispatcher.InvokeAsync(static () => { }).GetAwaiter().GetResult();
            getResult.Should().Throw<ObjectDisposedException>().WithMessage("*Stopping*");
            order.Add(1);
        });
        context.RegisterShutdownCallback(() => order.Add(2));

        PInvoke.PostQuitMessage(0);
        context.RunMessageLoop();

        order.Should().Equal(2, 1);
    }

    [STATestMethod]
    public void DispatcherRegisterShutdownCallback_RunsOnOwnerThreadBeforeCompletion()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        uint ownerThreadId = PInvoke.GetCurrentThreadId();
        uint callbackThreadId = 0;
        bool completionObserved = true;
        using ShutdownRegistration registration = context.Dispatcher.RegisterShutdownCallback(() =>
        {
            callbackThreadId = PInvoke.GetCurrentThreadId();
            completionObserved = context.Dispatcher.Completion.IsCompleted;
        });

        PInvoke.PostQuitMessage(0);
        context.RunMessageLoop();

        callbackThreadId.Should().Be(ownerThreadId);
        completionObserved.Should().BeFalse();
        context.Dispatcher.Completion.IsCompleted.Should().BeFalse();
        context.Dispose();
        context.Dispatcher.Completion.IsCompletedSuccessfully.Should().BeTrue();
    }

    [TestMethod]
    public void DispatcherRegisterShutdownCallback_ForeignThread_Throws()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Exception? failure = null;
        Thread thread = new(() =>
        {
            try
            {
                _ = context.Dispatcher.RegisterShutdownCallback(static () => { });
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });

        thread.Start();
        thread.Join();

        failure.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("The calling thread does not own this dispatcher.");
    }

    [TestMethod]
    public void DispatcherRegisterShutdownCallback_NullCallback_Throws()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();

        Action action = () => context.Dispatcher.RegisterShutdownCallback(null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("callback");
    }

    [TestMethod]
    public void DispatcherRegisterShutdownCallback_NoActiveContext_Throws()
    {
        ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher dispatcher = context.Dispatcher;
        context.Dispose();

        Action action = () => dispatcher.RegisterShutdownCallback(static () => { });

        action.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Be("A message loop is not running on this thread.");
    }

    [TestMethod]
    public void DispatcherRegisterShutdownCallback_DifferentActiveContext_Throws()
    {
        ThreadContext previousContext = ThreadingTestAccessors.CreateThreadContext();
        Dispatcher previousDispatcher = previousContext.Dispatcher;
        previousContext.Dispose();
        using ThreadContext currentContext = ThreadingTestAccessors.CreateThreadContext();

        Action action = () => previousDispatcher.RegisterShutdownCallback(static () => { });

        action.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Be("The current message loop does not own this dispatcher.");
    }

    [STATestMethod]
    public void RegisterShutdownCallback_ThrowingCallback_RunsRemainingCallbacks()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        List<int> order = [];
        context.RegisterShutdownCallback(() => order.Add(1));
        context.RegisterShutdownCallback(() =>
        {
            order.Add(2);
            throw new InvalidOperationException("Expected");
        });

        PInvoke.PostQuitMessage(0);
        Action run = context.RunMessageLoop;

        run.Should().Throw<AggregateException>()
            .Which.InnerExceptions.Should().ContainSingle()
            .Which.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("Expected");
        order.Should().Equal(2, 1);
    }

    [STATestMethod]
    public void RunMessageLoop_ThrowingShutdownTokenCallback_RunsShutdownCallbacks()
    {
        using ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        bool shutdownCallbackRan = false;
        context.RegisterShutdownCallback(() => shutdownCallbackRan = true);
        using CancellationTokenRegistration registration = context.Dispatcher.ShutdownToken.Register(
            static () => throw new InvalidOperationException("Expected"));
        PInvoke.PostQuitMessage(0);

        Action run = context.RunMessageLoop;

        run.Should().Throw<InvalidOperationException>().WithMessage("Expected");
        shutdownCallbackRan.Should().BeTrue();
    }

    [TestMethod]
    public void MessageFilterRegistration_DisposeAfterContextDispose_DoesNotThrow()
    {
        ThreadContext context = ThreadingTestAccessors.CreateThreadContext();
        MessageFilterRegistration registration =
            context.AddMessageFilter(new PassiveMessageFilter());
        context.Dispose();

        Action dispose = registration.Dispose;

        dispose.Should().NotThrow();
    }

    private sealed class PassiveMessageFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref MSG message) => false;
    }
}
