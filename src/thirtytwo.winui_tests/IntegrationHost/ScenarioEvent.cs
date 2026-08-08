// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IntegrationHost;

internal sealed record ScenarioEvent(
    string Scenario,
    string Event,
    DateTimeOffset TimestampUtc,
    int ProcessId,
    uint ThreadId,
    long WindowHandle,
    string? Message);