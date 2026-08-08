// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Dispatching;
using Windows.Win32;

namespace Windows.WinUI;

/// <summary>
///  Acquires a reference-counted WinUI environment for the current thirtytwo UI thread.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="XamlHostControl"/> and typed wrappers acquire their own environment leases. Applications using those
///   controls do not need to acquire another lease. Acquire directly only when WinUI services are needed independently
///   of a host control.
///  </para>
///  <para>
///   Acquire the environment from the window factory passed to <see cref="Application.Run(Window, bool)"/>, typically
///   at the start of the primary window's construction. At that point the thirtytwo dispatcher is active on the STA
///   thread. Acquire it before creating or loading WinUI content and before registering metadata providers or resource
///   dictionaries. Do not acquire it before <see cref="Application.Run(Window, bool)"/> or from a background thread.
///  </para>
///  <para>
///   Public leases may fall to zero and be acquired again; native XAML and queue state is released when the owning core
///   dispatcher shuts down.
///  </para>
///  <para>Only one designated XAML UI thread is supported per process.</para>
/// </remarks>
public sealed class XamlHostEnvironment : IDisposable
{
    private static readonly Lock s_lock = new();

    // Application construction permanently designates its XAML thread. Native environment state may stop, but moving
    // the retained process application to another thread or replacing it is not a supported rollback.
    private static Thread? s_designatedThread;
    private static XamlHostEnvironmentState? s_state;
    private static Microsoft.UI.Xaml.Application? s_processApplication;

    private XamlHostEnvironmentState? _state;

    private XamlHostEnvironment(XamlHostEnvironmentState state)
    {
        _state = state;
    }

    /// <summary>Gets the process WinUI application.</summary>
    /// <exception cref="ObjectDisposedException">This lease has been disposed.</exception>
    public Microsoft.UI.Xaml.Application Application => GetState().Application;

    /// <summary>Gets the current thread's Windows App SDK dispatcher queue.</summary>
    /// <exception cref="ObjectDisposedException">This lease has been disposed.</exception>
    public DispatcherQueue DispatcherQueue => GetState().Queue;

    /// <summary>Gets the application-wide metadata provider registry.</summary>
    /// <exception cref="ObjectDisposedException">This lease has been disposed.</exception>
    public XamlMetadataProviderRegistry MetadataProviders => GetState().HostApplication.MetadataProviders;

    /// <summary>Gets the application-wide resource dictionary registry.</summary>
    /// <exception cref="ObjectDisposedException">This lease has been disposed.</exception>
    public XamlResourceDictionaryRegistry ResourceDictionaries => GetState().HostApplication.ResourceDictionaries;

    /// <summary>
    ///  Gets whether this environment created the process WinUI application instead of adopting an existing one.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This is initialization information only. The process application is retained whether this environment created
    ///   or adopted it, so the value does not change application cleanup responsibilities.
    ///  </para>
    /// </remarks>
    public bool OwnsApplication => GetState().OwnsApplication;

    /// <summary>Gets whether this environment created the current thread's dispatcher queue.</summary>
    /// <remarks>
    ///  <para>
    ///   The environment shuts down a queue it created during core dispatcher shutdown. An existing queue is borrowed
    ///   and remains under its creator's lifetime management.
    ///  </para>
    /// </remarks>
    public bool OwnsDispatcherQueue => GetState().OwnsDispatcherQueue;

    /// <summary>
    ///  Gets a point-in-time snapshot of the active thread environment, or <see langword="null"/> when none exists.
    /// </summary>
    /// <remarks>
    ///  <para>The returned lease count and ownership state may become stale immediately after this property returns.</para>
    /// </remarks>
    public static XamlHostEnvironmentInfo? Current
    {
        get
        {
            lock (s_lock)
            {
                return s_state is { Disposed: false } state
                    ? new(
                        state.OwnerManagedThreadId,
                        state.OwnerNativeThreadId,
                        state.LeaseCount,
                        state.OwnsApplication,
                        state.OwnsDispatcherQueue)
                    : null;
            }
        }
    }

    /// <summary>
    ///  Gets an independently disposable handle to the current UI thread's WinUI environment, creating the environment
    ///  on the first call.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Call this from the window factory passed to <see cref="Application.Run(Window, bool)"/> after it establishes
    ///   the thirtytwo dispatcher on an STA thread. If the process already has a WinUI application, that application
    ///   must implement <see cref="IXamlHostApplication"/> and expose registries owned by the same thread. Do not create
    ///   or load WinUI content that depends on those registries before calling this method.
    ///  </para>
    ///  <para>
    ///   On the first call, this method borrows or creates the current thread's dispatcher queue, adopts or creates the
    ///   process WinUI application, and initializes the XAML manager. When it creates the application, it also registers
    ///   WinUI's built-in metadata provider and resources. After this method returns, register any library metadata
    ///   providers and resource dictionaries before creating or loading content that depends on them.
    ///  </para>
    ///  <para>
    ///   Each call returns a separate lease over the same thread-bound environment. Keep the lease while the caller
    ///   needs its properties, then dispose it on the owner thread. Disposal invalidates only that lease and decrements
    ///   the active lease count; native XAML state remains active until the owning thirtytwo dispatcher shuts down.
    ///  </para>
    /// </remarks>
    /// <returns>A thread-bound lease on the initialized WinUI environment.</returns>
    /// <exception cref="XamlHostInitializationException">
    ///  The thread, dispatcher, runtime, application, or XAML manager is incompatible with hosting.
    /// </exception>
    public static XamlHostEnvironment Acquire()
    {
        lock (s_lock)
        {
            Thread currentThread = Thread.CurrentThread;
            if (s_designatedThread is not null && !ReferenceEquals(s_designatedThread, currentThread))
            {
                int designatedThreadId = s_designatedThread.ManagedThreadId;
                XamlHostInitializationException exception = new(
                    XamlHostInitializationStage.ThreadValidation,
                    $"WinUI hosting is designated to managed thread {designatedThreadId}; the calling thread is {Environment.CurrentManagedThreadId}.",
                    GetCurrentNativeThreadId());
                XamlHostEventSource.Log.InitializationFailed(
                    exception.NativeThreadId,
                    (int)exception.Stage,
                    exception.HResult,
                    exception.GetType().FullName!);
                throw exception;
            }

            if (s_state is { Disposed: false } state)
            {
                state.AddLease();
                return new(state);
            }

            state = XamlHostEnvironmentState.Create();
            s_designatedThread ??= currentThread;
            s_state = state;
            return new(state);
        }
    }

    /// <summary>Releases this public lease. Repeated disposal has no effect.</summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own this lease.</exception>
    public void Dispose()
    {
        XamlHostEnvironmentState? state = _state;
        if (state is null)
        {
            return;
        }

        state.VerifyAccess();
        _state = null;

        lock (s_lock)
        {
            state.ReleaseLease();
        }
    }

    internal static uint GetCurrentNativeThreadId() => PInvoke.GetCurrentThreadId();

    internal static XamlApplication CreateProcessApplication()
    {
        if (s_processApplication is not null)
        {
            throw new XamlHostInitializationException(
                XamlHostInitializationStage.Application,
                "The retained process WinUI application cannot be rebound after its XAML thread has stopped.",
                GetCurrentNativeThreadId());
        }

        XamlApplication application = new();

        // Retain immediately. If later XAML initialization fails, Application construction has already affected
        // process WinUI state and a replacement Application would hide that partial initialization.
        s_processApplication = application;
        s_designatedThread ??= Thread.CurrentThread;
        return application;
    }

    internal static void RetainProcessApplication(Microsoft.UI.Xaml.Application application)
    {
        if (s_processApplication is not null && !ReferenceEquals(s_processApplication, application))
        {
            throw new XamlHostInitializationException(
                XamlHostInitializationStage.Application,
                "A different WinUI application is already retained for this process.",
                GetCurrentNativeThreadId());
        }

        s_processApplication = application;
        s_designatedThread ??= Thread.CurrentThread;
    }

    internal static void Shutdown(XamlHostEnvironmentState state)
    {
        state.VerifyAccess();
        lock (s_lock)
        {
            if (ReferenceEquals(s_state, state))
            {
                s_state = null;
            }
        }

        state.Dispose();
    }

    private XamlHostEnvironmentState GetState()
    {
        XamlHostEnvironmentState state = _state
            ?? throw new ObjectDisposedException(nameof(XamlHostEnvironment));
        state.VerifyAccess();
        ObjectDisposedException.ThrowIf(state.Disposed, this);
        return state;
    }
}