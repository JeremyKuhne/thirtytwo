// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal sealed record ScreenshotSnapshot(
    string Path,
    int Width,
    int Height,
    int SampledColorCount);
