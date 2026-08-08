// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace IntegrationHost;

internal sealed class XamlHostEventListener : EventListener
{
    private readonly List<int> _eventIds = [];

    internal IReadOnlyList<int> EventIds
    {
        get
        {
            lock (_eventIds)
            {
                return [.. _eventIds];
            }
        }
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "ThirtyTwo-WinUI")
        {
            EnableEvents(eventSource, EventLevel.Verbose);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        lock (_eventIds)
        {
            _eventIds.Add(eventData.EventId);
        }
    }
}