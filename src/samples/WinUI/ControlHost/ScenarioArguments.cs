// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ControlHost;

internal static class ScenarioArguments
{
    internal static ControlHostScenario Parse(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (arguments.Length == 0)
        {
            return ControlHostScenario.Interactive;
        }

        if (arguments.Length != 2 || arguments[0] != "--scenario")
        {
            throw new ArgumentException("Expected '--scenario <startup|uia-tree|normal-close|shutdown-timeout|airspace>'.");
        }

        return arguments[1] switch
        {
            "startup" => ControlHostScenario.Startup,
            "uia-tree" => ControlHostScenario.UiaTree,
            "normal-close" => ControlHostScenario.NormalClose,
            "shutdown-timeout" => ControlHostScenario.ShutdownTimeout,
            "airspace" => ControlHostScenario.Airspace,
            _ => throw new ArgumentException($"Unknown ControlHost scenario '{arguments[1]}'.")
        };
    }
}
