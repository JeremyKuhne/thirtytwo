// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Taken from Xuint samples. https://github.com/xunit/samples.xunit/blob/main/v3/RetryFactExample

using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit;

[XunitTestCaseDiscoverer(typeof(RetryFactDiscoverer))]
public class RetryFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = -1)
    : FactAttribute(sourceFilePath, sourceLineNumber)
{
    public int MaxRetries { get; set; } = 3;
}