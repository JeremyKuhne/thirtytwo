// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit;

[Serializable]
public class RetryTestCase : XunitTestCase
{
    private int _maxRetries;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public RetryTestCase() { }

    public RetryTestCase(
        IMessageSink diagnosticMessageSink,
        TestMethodDisplay testMethodDisplay,
        TestMethodDisplayOptions defaultMethodDisplayOptions,
        ITestMethod testMethod,
        int maxRetries)
        : base(diagnosticMessageSink, testMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments: null)
    {
        _maxRetries = maxRetries;
    }

    // This method is called by the xUnit test framework classes to run the test case. We will do the
    // loop here, forwarding on to the implementation in XunitTestCase to do the heavy lifting. We will
    // continue to re-run the test until the aggregator has an error (meaning that some internal error
    // condition happened), or the test runs without failure, or we've hit the maximum number of tries.
    public override async Task<RunSummary> RunAsync(
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var runCount = 0;

        while (true)
        {
            // This is really the only tricky bit: we need to capture and delay messages (since those will
            // contain run status) until we know we've decided to accept the final result;
            var delayedMessageBus = new DelayedMessageBus(messageBus);

            var summary = await base.RunAsync(diagnosticMessageSink, delayedMessageBus, constructorArguments, aggregator, cancellationTokenSource);
            if (aggregator.HasExceptions || summary.Failed == 0 || ++runCount >= _maxRetries)
            {
                delayedMessageBus.Dispose();  // Sends all the delayed messages
                return summary;
            }

            diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Execution of '{DisplayName}' failed (attempt #{runCount}), retrying..."));

            Thread.Sleep(50);  // Just to space things out a bit
        }
    }

    public override void Serialize(IXunitSerializationInfo data)
    {
        base.Serialize(data);
        data.AddValue("MaxRetries", _maxRetries);
    }

    public override void Deserialize(IXunitSerializationInfo data)
    {
        base.Deserialize(data);
        _maxRetries = data.GetValue<int>("MaxRetries");
    }
}