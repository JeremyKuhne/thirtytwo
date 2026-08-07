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

        (string text, bool truncated) = BoundedTextReader.ReadAsync(reader, MaximumLength)
            .GetAwaiter()
            .GetResult();

        text.Should().HaveLength(MaximumLength);
        truncated.Should().BeTrue();
        stream.Position.Should().Be(stream.Length);
    }
}
