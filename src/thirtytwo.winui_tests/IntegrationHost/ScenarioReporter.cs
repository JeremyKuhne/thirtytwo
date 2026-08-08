// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace IntegrationHost;

internal sealed unsafe class ScenarioReporter(EnvironmentScenario scenario)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    internal void Write(string eventName, HWND window = default, string? message = null)
    {
        ScenarioEvent scenarioEvent = new(
            ScenarioArguments.GetName(scenario),
            eventName,
            DateTimeOffset.UtcNow,
            Environment.ProcessId,
            PInvoke.GetCurrentThreadId(),
            window.IsNull ? 0 : (long)window.Value,
            message);
        Console.Out.WriteLine(JsonSerializer.Serialize(scenarioEvent, s_jsonOptions));
        Console.Out.Flush();
    }
}