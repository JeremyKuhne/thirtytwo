// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Threading;

using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

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
