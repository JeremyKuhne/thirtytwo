// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal enum WinUIIntegrationScenario
{
    Startup,
    UiaTree,
    NormalClose,
    ShutdownTimeout,
    RawAirspace,
    RawScrolling,
    EnvironmentOwned,
    EnvironmentBorrowed,
    EnvironmentComposition,
    EnvironmentMultipleLeases,
    EnvironmentCompatibleApplication,
    EnvironmentIncompatibleApplication,
    EnvironmentMtaRejected,
    EnvironmentWrongThreadRejected,
    EnvironmentSecondThreadRejected,
    EnvironmentFinalRelease,
    HostBasic,
    HostColorPicker,
    HostStress,
    HostMultiple,
    HostLayout,
    HostAirspace,
    HostScrolling,
    HostAccessibility,
    HostReparent,
    HostReplacement,
    HostPopupClose,
    HostShutdownCleanup,
    FocusTraversal,
    InputSemantics
}
