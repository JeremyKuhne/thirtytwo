// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Windows.WinUI.IntegrationHarness;

[TestClass]
public class ScenarioOutputReaderTests
{
    [TestMethod]
    public void ReadAsync_LineExceedsLimit_RejectsWithoutRetainingLine()
    {
        ScenarioOutputReader reader = Read(new string('x', 70_000));

        reader.StandardOutput.Should().BeEmpty();
        reader.Events.Should().BeEmpty();
        reader.ProtocolErrors.Should().ContainSingle().Which.Should().Contain("exceeded 65536");
        reader.Ready.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public void ReadAsync_ReadyProcessDoesNotMatch_RejectsEvent()
    {
        const int ExpectedProcessId = 42;
        WinUIIntegrationEvent scenarioEvent = new(
            "startup",
            "ready",
            DateTimeOffset.UtcNow,
            ExpectedProcessId + 1,
            10,
            100,
            null);
        string json = JsonSerializer.Serialize(scenarioEvent, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        ScenarioOutputReader reader = Read(json, ExpectedProcessId);

        reader.Events.Should().BeEmpty();
        reader.ProtocolErrors.Should().ContainSingle().Which.Should().Contain("does not match 42");
        reader.Ready.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public void ReadAsync_ManyMalformedLines_BoundsErrorsAndRetainedOutput()
    {
        string malformedLine = new('x', 60_000);
        string input = string.Join('\n', Enumerable.Repeat(malformedLine, 30));

        ScenarioOutputReader reader = Read(input);

        reader.ProtocolErrors.Should().HaveCount(16);
        reader.StandardOutput.Length.Should().BeLessThanOrEqualTo(1024 * 1024);
        reader.Events.Should().BeEmpty();
        reader.Ready.IsCompleted.Should().BeFalse();
    }

    [TestMethod]
    public void ReadAsync_CanceledToken_DoesNotThrow()
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes("ignored"));
        using StreamReader reader = new(stream, Encoding.UTF8);
        using CancellationTokenSource cancellationSource = new();
        cancellationSource.Cancel();
        ScenarioOutputReader outputReader = new("startup", 42);

        Action read = () => outputReader.ReadAsync(reader, cancellationSource.Token).GetAwaiter().GetResult();

        read.Should().NotThrow();
    }

    private static ScenarioOutputReader Read(string input, int expectedProcessId = 42)
    {
        ScenarioOutputReader outputReader = new("startup", expectedProcessId);
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(input));
        using StreamReader streamReader = new(stream, Encoding.UTF8);
        outputReader.ReadAsync(streamReader).GetAwaiter().GetResult();
        return outputReader;
    }
}
