// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ControlHost;

internal sealed unsafe class ScenarioReporter(ControlHostScenario scenario)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    internal void Write(string eventName, HWND window = default, string? message = null)
    {
        ScenarioEvent scenarioEvent = new(
            Scenario: ScenarioName,
            Event: eventName,
            TimestampUtc: DateTimeOffset.UtcNow,
            ProcessId: Environment.ProcessId,
            ThreadId: PInvoke.GetCurrentThreadId(),
            WindowHandle: window.IsNull ? 0 : (long)window.Value,
            Message: message);

        Console.Out.WriteLine(JsonSerializer.Serialize(scenarioEvent, s_jsonOptions));
        Console.Out.Flush();
    }

    private string ScenarioName => scenario switch
    {
        ControlHostScenario.Startup => "startup",
        ControlHostScenario.UiaTree => "uia-tree",
        ControlHostScenario.NormalClose => "normal-close",
        ControlHostScenario.ShutdownTimeout => "shutdown-timeout",
        _ => "interactive"
    };

}
