// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal sealed record UiaElementSnapshot(
    int Depth,
    string Name,
    string AutomationId,
    string ControlType,
    int NativeWindowHandle,
    bool IsKeyboardFocusable,
    bool HasKeyboardFocus,
    int ParentIndex,
    string RuntimeId,
    string[] SupportedPatterns);
