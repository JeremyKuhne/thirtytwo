// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/blob/main/v3/RetryFactExample

using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class RetryTestCaseRunner
    : XunitTestCaseRunnerBase<RetryTestCaseRunnerContext, IXunitTestCase, IXunitTest>
{
    public static RetryTestCaseRunner Instance { get; } = new();

    public async ValueTask<RunSummary> Run(
        int maxRetries,
        IXunitTestCase testCase,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        string displayName,
        string? skipReason,
        ExplicitOption explicitOption,
        object?[] constructorArguments)
    {
        // This code comes from XunitRunnerHelper.RunXunitTestCase, and it's centralized
        // here just so we don't have to duplicate it in both RetryTestCase and
        // RetryDelayEnumeratedTestCase.
        var tests = await aggregator.RunAsync(testCase.CreateTests, []);

        if (aggregator.ToException() is Exception ex)
        {
            if (ex.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal))
                return XunitRunnerHelper.SkipTestCases(
                    messageBus,
                    cancellationTokenSource,
                    [testCase],
                    ex.Message[DynamicSkipToken.Value.Length..],
                    sendTestCaseMessages: false
                );
            else
                return XunitRunnerHelper.FailTestCases(
                    messageBus,
                    cancellationTokenSource,
                    [testCase],
                    ex,
                    sendTestCaseMessages: false
                );
        }

        await using var ctxt = new RetryTestCaseRunnerContext(
            maxRetries,
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            displayName,
            skipReason,
            explicitOption,
            constructorArguments);

        await ctxt.InitializeAsync();

        return await Run(ctxt);
    }

    protected override async ValueTask<RunSummary> RunTest(
        RetryTestCaseRunnerContext ctxt,
        IXunitTest test)
    {
        var runCount = 0;
        var maxRetries = ctxt.MaxRetries;

        if (maxRetries < 1)
            maxRetries = 3;

        while (true)
        {
            // This is really the only tricky bit: we need to capture and delay messages (since those will
            // contain run status) until we know we've decided to accept the final result.
            var delayedMessageBus = new DelayedMessageBus(ctxt.MessageBus);
            var aggregator = ctxt.Aggregator.Clone();
            var result = await XunitTestRunner.Instance.Run(
                test,
                delayedMessageBus,
                ctxt.ConstructorArguments,
                ctxt.ExplicitOption,
                aggregator,
                ctxt.CancellationTokenSource,
                ctxt.BeforeAfterTestAttributes
            );

            if (!(aggregator.HasExceptions || result.Failed != 0) || ++runCount >= maxRetries)
            {
                delayedMessageBus.Dispose();  // Sends all the delayed messages
                return result;
            }

            TestContext.Current.SendDiagnosticMessage(
                "Execution of '{0}' failed (attempt #{1}), retrying...",
                test.TestDisplayName,
                runCount);

            ctxt.Aggregator.Clear();
        }
    }
}
