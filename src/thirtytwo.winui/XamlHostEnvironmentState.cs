// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Hosting;
using Windows.Threading;

namespace Windows.WinUI;

/// <summary>
///  Owns the WinUI state for the designated UI thread and tracks active host-environment leases.
/// </summary>
internal sealed class XamlHostEnvironmentState
{
    private readonly XamlThreadAffinity _affinity = new();
    private readonly DispatcherQueueController? _queueController;
    private readonly WindowsXamlManager _xamlManager;
    private ShutdownRegistration _shutdownRegistration;
    private bool _disposed;

    private XamlHostEnvironmentState(
        Dispatcher dispatcher,
        DispatcherQueue queue,
        DispatcherQueueController? queueController,
        WindowsXamlManager xamlManager,
        Microsoft.UI.Xaml.Application application,
        IXamlHostApplication hostApplication,
        bool ownsApplication)
    {
        Dispatcher = dispatcher;
        Queue = queue;
        _queueController = queueController;
        _xamlManager = xamlManager;
        Application = application;
        HostApplication = hostApplication;
        OwnsApplication = ownsApplication;
        LeaseCount = 1;
    }

    /// <summary>Gets the owning thirtytwo dispatcher whose shutdown tears down this environment.</summary>
    internal Dispatcher Dispatcher { get; }

    /// <summary>Gets the Windows App SDK dispatcher queue associated with the owner thread.</summary>
    internal DispatcherQueue Queue { get; }

    /// <summary>Gets the retained process WinUI application.</summary>
    internal Microsoft.UI.Xaml.Application Application { get; }

    /// <summary>Gets the application's metadata and resource composition contract.</summary>
    internal IXamlHostApplication HostApplication { get; }

    /// <summary>Gets whether this environment created the process WinUI application.</summary>
    internal bool OwnsApplication { get; }

    /// <summary>Gets whether this environment created and must shut down the dispatcher queue.</summary>
    internal bool OwnsDispatcherQueue => _queueController is not null;

    /// <summary>Gets the number of active public environment leases.</summary>
    internal int LeaseCount { get; private set; }

    /// <summary>Gets the managed identifier of the owner thread.</summary>
    internal int OwnerManagedThreadId => _affinity.ManagedThreadId;

    /// <summary>Gets the native identifier of the owner thread.</summary>
    internal uint OwnerNativeThreadId => _affinity.NativeThreadId;

    /// <summary>Gets whether this environment has entered dispatcher shutdown.</summary>
    internal bool Disposed => _disposed;

    internal static XamlHostEnvironmentState Create()
    {
        uint nativeThreadId = XamlHostEnvironment.GetCurrentNativeThreadId();
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            XamlHostInitializationException exception = new(
                XamlHostInitializationStage.ThreadValidation,
                $"WinUI hosting requires an STA thread. Actual apartment: {Thread.CurrentThread.GetApartmentState()}.",
                nativeThreadId);
            XamlHostEventSource.Log.InitializationFailed(
                nativeThreadId,
                (int)exception.Stage,
                exception.HResult,
                exception.GetType().FullName!);
            throw exception;
        }

        Dispatcher dispatcher = Dispatcher.Current
            ?? throw CreateThreadValidationException(nativeThreadId);

        DispatcherQueueController? queueController = null;
        WindowsXamlManager? xamlManager = null;
        Microsoft.UI.Xaml.Application? preexistingApplication = Microsoft.UI.Xaml.Application.Current;
        XamlApplication? createdApplication = null;

        try
        {
            DispatcherQueue? queue = DispatcherQueue.GetForCurrentThread();
            if (queue is null)
            {
                try
                {
                    queueController = DispatcherQueueController.CreateOnCurrentThread();
                    queue = queueController.DispatcherQueue;
                }
                catch (Exception exception)
                {
                    throw CreateInitializationException(
                        XamlHostInitializationStage.DispatcherQueue,
                        "Failed to create the Windows App SDK dispatcher queue. Verify runtime deployment and process architecture.",
                        nativeThreadId,
                        exception);
                }
            }

            Microsoft.UI.Xaml.Application? application = preexistingApplication;
            bool ownsApplication = preexistingApplication is null;
            if (application is null)
            {
                try
                {
                    createdApplication = XamlHostEnvironment.CreateProcessApplication();
                    application = createdApplication;
                }
                catch (Exception exception)
                {
                    throw CreateInitializationException(
                        XamlHostInitializationStage.Application,
                        "Failed to create the WinUI host application.",
                        nativeThreadId,
                        exception);
                }
            }

            try
            {
                xamlManager = WindowsXamlManager.InitializeForCurrentThread();
            }
            catch (Exception exception)
            {
                throw CreateInitializationException(
                    XamlHostInitializationStage.XamlManager,
                    "Failed to initialize the WinUI XAML manager. Verify runtime WinMD and resource deployment.",
                    nativeThreadId,
                    exception);
            }

            if (createdApplication is not null)
            {
                try
                {
                    createdApplication.InitializeComposition();
                }
                catch (Exception exception)
                {
                    throw CreateInitializationException(
                        XamlHostInitializationStage.Application,
                        "Failed to initialize the WinUI host application's metadata and resources.",
                        nativeThreadId,
                        exception);
                }
            }

            if (application is not IXamlHostApplication hostApplication)
            {
                string applicationType = application.GetType().FullName ?? application.GetType().Name;
                string contractType = typeof(IXamlHostApplication).FullName!;
                throw new XamlHostInitializationException(
                    XamlHostInitializationStage.Application,
                    $"Application.Current is '{applicationType}', which does not implement {contractType}.",
                    nativeThreadId);
            }

            if (hostApplication.MetadataProviders.OwnerNativeThreadId != nativeThreadId
                || hostApplication.ResourceDictionaries.OwnerNativeThreadId != nativeThreadId)
            {
                throw new XamlHostInitializationException(
                    XamlHostInitializationStage.Application,
                    "The existing WinUI host application's registries belong to a different XAML thread.",
                    nativeThreadId);
            }

                    XamlHostEnvironment.RetainProcessApplication(application);

            XamlHostEnvironmentState state = new(
                dispatcher,
                queue,
                queueController,
                xamlManager,
                application,
                hostApplication,
                ownsApplication);
            state._shutdownRegistration = dispatcher.RegisterShutdownCallback(
                () => XamlHostEnvironment.Shutdown(state));
            queueController = null;
            xamlManager = null;
            XamlHostEventSource.Log.EnvironmentCreated(
                nativeThreadId,
                state.OwnsDispatcherQueue,
                state.OwnsApplication);
            XamlHostEventSource.Log.LeaseCountChanged(nativeThreadId, state.LeaseCount);
            return state;
        }
        catch (Exception exception)
        {
            XamlHostInitializationStage stage = exception is XamlHostInitializationException initializationException
                ? initializationException.Stage
                : XamlHostInitializationStage.Application;
            XamlHostEventSource.Log.InitializationFailed(
                nativeThreadId,
                (int)stage,
                exception.HResult,
                exception.GetType().FullName ?? exception.GetType().Name);
            xamlManager?.Dispose();
            queueController?.ShutdownQueue();
            throw;
        }
    }

    internal void AddLease()
    {
        VerifyAccess();
        ObjectDisposedException.ThrowIf(_disposed, this);
        LeaseCount = checked(LeaseCount + 1);
        XamlHostEventSource.Log.LeaseCountChanged(OwnerNativeThreadId, LeaseCount);
    }

    internal void ReleaseLease()
    {
        VerifyAccess();
        if (_disposed)
        {
            return;
        }

        if (LeaseCount <= 0)
        {
            throw new InvalidOperationException("The XAML environment has no active public lease to release.");
        }

        LeaseCount--;
        XamlHostEventSource.Log.LeaseCountChanged(OwnerNativeThreadId, LeaseCount);
    }

    internal void VerifyAccess() => _affinity.VerifyAccess();

    internal void Dispose()
    {
        VerifyAccess();
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        LeaseCount = 0;
        try
        {
            _xamlManager.Dispose();
        }
        finally
        {
            try
            {
                _queueController?.ShutdownQueue();
            }
            finally
            {
                _shutdownRegistration.Dispose();
                XamlHostEventSource.Log.EnvironmentStopped(OwnerNativeThreadId);
            }
        }
    }

    private static XamlHostInitializationException CreateInitializationException(
        XamlHostInitializationStage stage,
        string message,
        uint nativeThreadId,
        Exception exception)
        => exception as XamlHostInitializationException
            ?? new(stage, message, nativeThreadId, exception);

    private static XamlHostInitializationException CreateThreadValidationException(uint nativeThreadId)
    {
        XamlHostInitializationException exception = new(
            XamlHostInitializationStage.ThreadValidation,
            "WinUI hosting requires an active thirtytwo Application.Run message loop.",
            nativeThreadId);
        XamlHostEventSource.Log.InitializationFailed(
            nativeThreadId,
            (int)exception.Stage,
            exception.HResult,
            exception.GetType().FullName!);
        return exception;
    }
}