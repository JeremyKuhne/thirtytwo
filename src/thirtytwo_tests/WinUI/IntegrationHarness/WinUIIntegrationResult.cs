// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal sealed record WinUIIntegrationResult(
    string Scenario,
    int ProcessId,
    uint MainThreadId,
    long WindowHandle,
    IReadOnlyList<long> WindowHandles,
    int? ExitCode,
    bool TimedOut,
    TimeSpan Duration,
    IReadOnlyList<WinUIIntegrationEvent> Events,
    string StandardOutput,
    string StandardError,
    string? LastEvent,
    string? DiagnosticMessage,
    string ArtifactDirectory,
    string ResultPath,
    string? DumpPath,
    UiaSnapshot? Uia,
    ScreenshotSnapshot? Screenshot);
