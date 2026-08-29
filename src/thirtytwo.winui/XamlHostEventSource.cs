// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace Windows.WinUI;

/// <summary>
///  Emits WinUI host lifecycle, DPI, lease, initialization failure, and composition collision events.
/// </summary>
[EventSource(Name = "ThirtyTwo-WinUI")]
internal sealed class XamlHostEventSource : EventSource
{
    /// <summary>
    ///  Gets the singleton event source instance used by WinUI host components.
    /// </summary>
    internal static XamlHostEventSource Log { get; } = new();

    /// <summary>
    ///  Writes an event when the thread-bound WinUI environment is created.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the owning UI thread.</param>
    /// <param name="ownsQueue">
    ///  <see langword="true"/> when the environment created the Windows App SDK dispatcher queue.
    /// </param>
    /// <param name="ownsApplication">
    ///  <see langword="true"/> when the environment created the process WinUI application.
    /// </param>
    [Event(1, Level = EventLevel.Informational)]
    public void EnvironmentCreated(uint nativeThreadId, bool ownsQueue, bool ownsApplication)
        => WriteEvent(1, nativeThreadId, ownsQueue, ownsApplication);

    /// <summary>
    ///  Writes an event when the active public lease count for the WinUI environment changes.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the owning UI thread.</param>
    /// <param name="leaseCount">Updated number of active public leases after the increment or decrement.</param>
    [Event(2, Level = EventLevel.Informational)]
    public void LeaseCountChanged(uint nativeThreadId, int leaseCount)
        => WriteEvent(2, nativeThreadId, leaseCount);

    /// <summary>
    ///  Writes an event when the thread-bound WinUI environment is stopped during dispatcher shutdown.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the owning UI thread.</param>
    [Event(3, Level = EventLevel.Informational)]
    public void EnvironmentStopped(uint nativeThreadId)
        => WriteEvent(3, nativeThreadId);

    /// <summary>
    ///  Writes an event when WinUI host environment initialization fails.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the thread where initialization failed.</param>
    /// <param name="stage">
    ///  Numeric <see cref="XamlHostInitializationStage"/> value identifying the failed initialization stage.
    /// </param>
    /// <param name="hresult">HRESULT associated with the failure.</param>
    /// <param name="exceptionType">Runtime type name of the exception that was observed.</param>
    [Event(4, Level = EventLevel.Error)]
    public void InitializationFailed(uint nativeThreadId, int stage, int hresult, string exceptionType)
        => WriteEvent(4, nativeThreadId, stage, hresult, exceptionType);

    /// <summary>
    ///  Writes an event when multiple metadata providers resolve the same requested XAML type.
    /// </summary>
    /// <param name="requestedType">Requested type name that more than one provider could resolve.</param>
    /// <param name="winningProvider">Type name of the provider that won by registration order.</param>
    /// <param name="conflictingProvider">
    ///  Type name of a later provider that also resolved the same requested type.
    /// </param>
    [Event(5, Level = EventLevel.Warning)]
    public void MetadataCollision(string requestedType, string winningProvider, string conflictingProvider)
        => WriteEvent(5, requestedType, winningProvider, conflictingProvider);

    /// <summary>
    ///  Writes an event when a later resource dictionary overrides an existing resource key.
    /// </summary>
    /// <param name="key">Resource key representation used in the collision report.</param>
    [Event(6, Level = EventLevel.Warning)]
    public void ResourceCollision(string key)
        => WriteEvent(6, key);

    /// <summary>
    ///  Writes an event when a host callback catches a failure at a native window-procedure boundary.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the UI thread handling the callback.</param>
    /// <param name="operation">Callback operation name where the failure was observed.</param>
    /// <param name="hresult">HRESULT associated with the failure.</param>
    /// <param name="exceptionType">Runtime type name of the exception that was observed.</param>
    [Event(7, Level = EventLevel.Error)]
    public void HostCallbackFailed(uint nativeThreadId, string operation, int hresult, string exceptionType)
        => WriteEvent(7, nativeThreadId, operation, hresult, exceptionType);

    /// <summary>
    ///  Writes an event when host DPI changes trigger a XAML site-bridge resize.
    /// </summary>
    /// <param name="nativeThreadId">Native thread identifier for the UI thread handling the DPI change.</param>
    /// <param name="oldDpi">Previous DPI value reported to the host.</param>
    /// <param name="newDpi">New DPI value reported to the host.</param>
    /// <param name="width">Host client width used for the resize operation.</param>
    /// <param name="height">Host client height used for the resize operation.</param>
    [Event(8, Level = EventLevel.Informational)]
    public void HostDpiChanged(uint nativeThreadId, uint oldDpi, uint newDpi, int width, int height)
        => WriteEvent(8, nativeThreadId, oldDpi, newDpi, width, height);
}