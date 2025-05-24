// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit;

public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public RetryFactDiscoverer(IMessageSink diagnosticMessageSink) => _diagnosticMessageSink = diagnosticMessageSink;

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        var maxRetries = factAttribute.GetNamedArgument<int>("MaxRetries");
        if (maxRetries < 1)
        {
            maxRetries = 3;
        }

        yield return new RetryTestCase(
            _diagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod,
            maxRetries);
    }
}
