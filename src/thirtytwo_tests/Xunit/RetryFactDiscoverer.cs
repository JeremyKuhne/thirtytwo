// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

public class RetryFactDiscoverer : IXunitTestCaseDiscoverer
{
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        var maxRetries = (factAttribute as RetryFactAttribute)?.MaxRetries ?? 3;
        var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute, label: null);
        var testCase = new RetryTestCase(
            maxRetries,
            details.ResolvedTestMethod,
            details.TestCaseDisplayName,
            details.UniqueID,
            details.Explicit,
            details.SkipExceptions,
            details.SkipReason,
            details.SkipType,
            details.SkipUnless,
            details.SkipWhen,
            testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
            timeout: details.Timeout
        );

        return new([testCase]);
    }
}
