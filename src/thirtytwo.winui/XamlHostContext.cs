// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Dispatching;

namespace Windows.WinUI;

/// <summary>
///  Provides WinUI services through the environment lease owned by a XAML host control.
/// </summary>
/// <remarks>
///  <para>
///   This context is not independently disposable. Its properties remain available while the host that supplied it is
///   active and throw <see cref="ObjectDisposedException"/> after that host is disposed.
///  </para>
///  <para>
///   Services are thread-affine and must be accessed on the owning XAML host thread.
///  </para>
/// </remarks>
public sealed class XamlHostContext
{
    private readonly XamlHostEnvironment _environment;

    /// <summary>
    ///  Initializes a context backed by the specified host environment.
    /// </summary>
    /// <param name="environment">The environment lease that supplies the context's thread-affine services.</param>
    internal XamlHostContext(XamlHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    ///  Gets the process WinUI application.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public Microsoft.UI.Xaml.Application Application => _environment.Application;

    /// <summary>
    ///  Gets the current thread's Windows App SDK dispatcher queue.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public DispatcherQueue DispatcherQueue => _environment.DispatcherQueue;

    /// <summary>
    ///  Gets the application-wide metadata provider registry.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public XamlMetadataProviderRegistry MetadataProviders => _environment.MetadataProviders;

    /// <summary>
    ///  Gets the application-wide resource dictionary registry.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public XamlResourceDictionaryRegistry ResourceDictionaries => _environment.ResourceDictionaries;

    /// <summary>
    ///  Gets whether the host environment created the process WinUI application.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public bool OwnsApplication => _environment.OwnsApplication;

    /// <summary>
    ///  Gets whether the host environment created the current thread's dispatcher queue.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The owning host has been disposed.</exception>
    public bool OwnsDispatcherQueue => _environment.OwnsDispatcherQueue;
}