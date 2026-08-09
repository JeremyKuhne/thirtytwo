// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IntegrationHost;

internal static class ScenarioArguments
{
    internal static EnvironmentScenario Parse(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Length != 2 || arguments[0] != "--scenario")
        {
            throw new ArgumentException("Expected '--scenario <name>'.");
        }

        return arguments[1] switch
        {
            "environment-owned" => EnvironmentScenario.Owned,
            "environment-borrowed" => EnvironmentScenario.Borrowed,
            "environment-composition" => EnvironmentScenario.Composition,
            "environment-multiple-leases" => EnvironmentScenario.MultipleLeases,
            "environment-compatible-application" => EnvironmentScenario.CompatibleApplication,
            "environment-incompatible-application" => EnvironmentScenario.IncompatibleApplication,
            "environment-mta-rejected" => EnvironmentScenario.MtaRejected,
            "environment-wrong-thread-rejected" => EnvironmentScenario.WrongThreadRejected,
            "environment-second-thread-rejected" => EnvironmentScenario.SecondThreadRejected,
            "environment-final-release" => EnvironmentScenario.FinalRelease,
            "host-basic" => EnvironmentScenario.HostBasic,
            "host-color-picker" => EnvironmentScenario.HostColorPicker,
            "host-stress" => EnvironmentScenario.HostStress,
            "host-multiple" => EnvironmentScenario.HostMultiple,
            "host-layout" => EnvironmentScenario.HostLayout,
            "host-airspace" => EnvironmentScenario.HostAirspace,
            "host-scrolling" => EnvironmentScenario.HostScrolling,
            "host-reparent" => EnvironmentScenario.HostReparent,
            "host-replacement" => EnvironmentScenario.HostReplacement,
            "host-popup-close" => EnvironmentScenario.HostPopupClose,
            "host-shutdown-cleanup" => EnvironmentScenario.HostShutdownCleanup,
            "focus-traversal" => EnvironmentScenario.FocusTraversal,
            "input-semantics" => EnvironmentScenario.InputSemantics,
            _ => throw new ArgumentException($"Unknown environment scenario '{arguments[1]}'.")
        };
    }

    internal static string GetName(EnvironmentScenario scenario) => scenario switch
    {
        EnvironmentScenario.Owned => "environment-owned",
        EnvironmentScenario.Borrowed => "environment-borrowed",
        EnvironmentScenario.Composition => "environment-composition",
        EnvironmentScenario.MultipleLeases => "environment-multiple-leases",
        EnvironmentScenario.CompatibleApplication => "environment-compatible-application",
        EnvironmentScenario.IncompatibleApplication => "environment-incompatible-application",
        EnvironmentScenario.MtaRejected => "environment-mta-rejected",
        EnvironmentScenario.WrongThreadRejected => "environment-wrong-thread-rejected",
        EnvironmentScenario.SecondThreadRejected => "environment-second-thread-rejected",
        EnvironmentScenario.FinalRelease => "environment-final-release",
        EnvironmentScenario.HostBasic => "host-basic",
        EnvironmentScenario.HostColorPicker => "host-color-picker",
        EnvironmentScenario.HostStress => "host-stress",
        EnvironmentScenario.HostMultiple => "host-multiple",
        EnvironmentScenario.HostLayout => "host-layout",
        EnvironmentScenario.HostAirspace => "host-airspace",
        EnvironmentScenario.HostScrolling => "host-scrolling",
        EnvironmentScenario.HostReparent => "host-reparent",
        EnvironmentScenario.HostReplacement => "host-replacement",
        EnvironmentScenario.HostPopupClose => "host-popup-close",
        EnvironmentScenario.HostShutdownCleanup => "host-shutdown-cleanup",
        EnvironmentScenario.FocusTraversal => "focus-traversal",
        EnvironmentScenario.InputSemantics => "input-semantics",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario))
    };
}