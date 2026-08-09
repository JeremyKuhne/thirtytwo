// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IntegrationHost;

internal enum EnvironmentScenario
{
    Owned,
    Borrowed,
    Composition,
    MultipleLeases,
    CompatibleApplication,
    IncompatibleApplication,
    MtaRejected,
    WrongThreadRejected,
    SecondThreadRejected,
    FinalRelease,
    HostBasic,
    HostColorPicker,
    HostStress,
    HostMultiple,
    HostLayout,
    HostAirspace,
    HostReparent,
    HostReplacement,
    HostPopupClose,
    HostShutdownCleanup,
    FocusTraversal,
    InputSemantics
}