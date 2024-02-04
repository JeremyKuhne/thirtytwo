// Taken from Xuint samples. https://github.com/xunit/samples.xunit/tree/main/RetryFactExample

namespace Xunit;

public class RetryFactAttribute : FactAttribute
{
    public int MaxRetries { get; set; } = 3;
}