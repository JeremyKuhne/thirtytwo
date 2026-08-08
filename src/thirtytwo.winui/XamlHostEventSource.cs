// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace Windows.WinUI;

/// <summary>
///  Emits WinUI host lifecycle, lease, initialization failure, and composition collision events.
/// </summary>
[EventSource(Name = "ThirtyTwo-WinUI")]
internal sealed class XamlHostEventSource : EventSource
{
    internal static XamlHostEventSource Log { get; } = new();

    [Event(1, Level = EventLevel.Informational)]
    public void EnvironmentCreated(uint nativeThreadId, bool ownsQueue, bool ownsApplication)
        => WriteEvent(1, nativeThreadId, ownsQueue, ownsApplication);

    [Event(2, Level = EventLevel.Informational)]
    public void LeaseCountChanged(uint nativeThreadId, int leaseCount)
        => WriteEvent(2, nativeThreadId, leaseCount);

    [Event(3, Level = EventLevel.Informational)]
    public void EnvironmentStopped(uint nativeThreadId)
        => WriteEvent(3, nativeThreadId);

    [Event(4, Level = EventLevel.Error)]
    public void InitializationFailed(uint nativeThreadId, int stage, int hresult, string exceptionType)
        => WriteEvent(4, nativeThreadId, stage, hresult, exceptionType);

    [Event(5, Level = EventLevel.Warning)]
    public void MetadataCollision(string requestedType, string winningProvider, string conflictingProvider)
        => WriteEvent(5, requestedType, winningProvider, conflictingProvider);

    [Event(6, Level = EventLevel.Warning)]
    public void ResourceCollision(string key)
        => WriteEvent(6, key);
}