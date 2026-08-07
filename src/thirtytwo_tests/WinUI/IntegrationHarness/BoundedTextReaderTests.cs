// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
public class BoundedTextReaderTests
{
    [TestMethod]
    public void ReadAsync_InputExceedsLimit_TruncatesAndDrains()
    {
        const int MaximumLength = 1024;
        string input = new('x', MaximumLength * 2);
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(input));
        using StreamReader reader = new(stream, Encoding.UTF8);

        BoundedTextReader boundedReader = new(MaximumLength);
        boundedReader.ReadAsync(reader).GetAwaiter().GetResult();

        boundedReader.Text.Should().HaveLength(MaximumLength);
        boundedReader.Truncated.Should().BeTrue();
        stream.Position.Should().Be(stream.Length);
    }

    [TestMethod]
    public void ReadAsync_CanceledToken_DoesNotThrow()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("ignored"));
        using StreamReader reader = new(stream, Encoding.UTF8);
        using CancellationTokenSource cancellationSource = new();
        cancellationSource.Cancel();
        BoundedTextReader boundedReader = new(1024);

        Action read = () => boundedReader.ReadAsync(reader, cancellationSource.Token).GetAwaiter().GetResult();

        read.Should().NotThrow();
    }
}
