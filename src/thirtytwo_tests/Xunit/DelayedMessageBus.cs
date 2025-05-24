// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
///  Used to capture messages to potentially be forwarded later. Messages are forwarded by
///  disposing of the message bus.
/// </summary>
public class DelayedMessageBus : IMessageBus
{
    private readonly IMessageBus _innerBus;
    private readonly List<IMessageSinkMessage> _messages = [];

    public DelayedMessageBus(IMessageBus innerBus) => _innerBus = innerBus;

    public bool QueueMessage(IMessageSinkMessage message)
    {
        // Technically speaking, this lock isn't necessary in our case, because we know we're using this
        // message bus for a single test (so there's no possibility of parallelism). However, it's good
        // practice when something might be used where parallel messages might arrive, so it's here in
        // this sample.
        lock (_messages)
        {
            _messages.Add(message);
        }

        // No way to ask the inner bus if they want to cancel without sending them the message, so
        // we just go ahead and continue always.
        return true;
    }

    public void Dispose()
    {
        foreach (var message in _messages)
        {
            _innerBus.QueueMessage(message);
        }

        GC.SuppressFinalize(this);
    }
}
