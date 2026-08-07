// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Windows.WinUI.IntegrationHarness;

internal sealed class ScenarioOutputReader(string expectedScenario, int expectedProcessId)
{
    private const int MaximumEventCount = 256;
    private const int MaximumLineLength = 64 * 1024;
    private const int MaximumOutputLength = 1024 * 1024;
    private const int MaximumProtocolErrorCount = 16;
    private const int ReadBufferLength = 4096;

    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly List<WinUIIntegrationEvent> _events = [];
    private readonly List<string> _lines = [];
    private readonly List<string> _protocolErrors = [];
    private readonly TaskCompletionSource<WinUIIntegrationEvent> _readySource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _retainedOutputLength;
    private bool _outputLimitReported;

    internal IReadOnlyList<WinUIIntegrationEvent> Events => _events;

    internal IReadOnlyList<string> ProtocolErrors => _protocolErrors;

    internal Task<WinUIIntegrationEvent> Ready => _readySource.Task;

    internal string StandardOutput => string.Join(Environment.NewLine, _lines);

    internal async Task ReadAsync(StreamReader reader)
    {
        char[] readBuffer = new char[ReadBufferLength];
        char[] lineBuffer = new char[MaximumLineLength];
        int lineLength = 0;
        bool lineTooLong = false;

        while (true)
        {
            int read = await reader.ReadAsync(readBuffer).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            for (int index = 0; index < read; index++)
            {
                char character = readBuffer[index];
                if (character == '\n')
                {
                    ProcessBufferedLine(lineBuffer, lineLength, lineTooLong);
                    lineLength = 0;
                    lineTooLong = false;
                    continue;
                }

                if (lineLength < lineBuffer.Length)
                {
                    lineBuffer[lineLength++] = character;
                }
                else
                {
                    lineTooLong = true;
                }
            }
        }

        if (lineLength > 0 || lineTooLong)
        {
            ProcessBufferedLine(lineBuffer, lineLength, lineTooLong);
        }
    }

    private void ProcessBufferedLine(char[] lineBuffer, int lineLength, bool lineTooLong)
    {
        if (lineTooLong)
        {
            AddProtocolError($"A protocol line exceeded {MaximumLineLength} characters.");
            return;
        }

        if (lineLength > 0 && lineBuffer[lineLength - 1] == '\r')
        {
            lineLength--;
        }

        if (lineLength == 0)
        {
            return;
        }

        int separatorLength = _lines.Count == 0 ? 0 : Environment.NewLine.Length;
        if (_retainedOutputLength > MaximumOutputLength - lineLength - separatorLength)
        {
            if (!_outputLimitReported)
            {
                _outputLimitReported = true;
                AddProtocolError($"Protocol output exceeded {MaximumOutputLength} retained characters.");
            }

            return;
        }

        string line = new(lineBuffer, 0, lineLength);
        _lines.Add(line);
        _retainedOutputLength += lineLength + separatorLength;

        try
        {
            WinUIIntegrationEvent? scenarioEvent =
                JsonSerializer.Deserialize<WinUIIntegrationEvent>(line, s_jsonOptions);
            if (scenarioEvent is null)
            {
                AddProtocolError("A JSON event deserialized to null.");
                return;
            }

            if (scenarioEvent.Scenario != expectedScenario)
            {
                AddProtocolError(
                    $"Event scenario '{scenarioEvent.Scenario}' does not match '{expectedScenario}'.");
                return;
            }

            if (scenarioEvent.ProcessId != expectedProcessId)
            {
                AddProtocolError(
                    $"Event process {scenarioEvent.ProcessId} does not match {expectedProcessId}.");
                return;
            }

            if (scenarioEvent.ThreadId == 0)
            {
                AddProtocolError("An event reported native thread ID 0.");
                return;
            }

            if (scenarioEvent.Event == "ready" && scenarioEvent.WindowHandle <= 0)
            {
                AddProtocolError("The ready event did not report a valid window handle.");
                return;
            }

            if (_events.Count >= MaximumEventCount)
            {
                AddProtocolError($"Protocol event count exceeded {MaximumEventCount}.");
                return;
            }

            _events.Add(scenarioEvent);
            if (scenarioEvent.Event == "ready")
            {
                _readySource.TrySetResult(scenarioEvent);
            }
        }
        catch (JsonException exception)
        {
            AddProtocolError($"Invalid JSON at byte {exception.BytePositionInLine}: {exception.Message}");
        }
    }

    private void AddProtocolError(string message)
    {
        if (_protocolErrors.Count < MaximumProtocolErrorCount)
        {
            _protocolErrors.Add(message);
        }
    }
}
