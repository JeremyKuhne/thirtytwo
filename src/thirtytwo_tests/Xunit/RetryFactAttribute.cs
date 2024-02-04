// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

using Xunit.Sdk;

namespace Xunit;

[XunitTestCaseDiscoverer("Xunit.RetryFactDiscoverer", "thirtytwo_tests")]
public class RetryFactAttribute : FactAttribute
{
    public int MaxRetries { get; set; } = 3;
}