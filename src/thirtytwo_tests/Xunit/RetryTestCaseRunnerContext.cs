// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/blob/main/v3/RetryFactExample

using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class RetryTestCaseRunnerContext(
    int maxRetries,
    IXunitTestCase testCase,
    IReadOnlyCollection<IXunitTest> tests,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    CancellationTokenSource cancellationTokenSource,
    string displayName,
    string? skipReason,
    ExplicitOption explicitOption,
    object?[] constructorArguments) :
        XunitTestCaseRunnerBaseContext<IXunitTestCase, IXunitTest>(
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            displayName,
            skipReason,
            explicitOption,
            constructorArguments)
{
    public int MaxRetries { get; } = maxRetries;
}