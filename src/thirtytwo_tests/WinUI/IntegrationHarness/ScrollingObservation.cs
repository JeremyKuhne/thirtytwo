// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI.IntegrationHarness;

internal sealed record ScrollingObservation(
    int ViewportX,
    int ViewportY,
    int ContentX,
    int ContentY,
    int HostX,
    int HostY,
    int HostWidth,
    int HostHeight,
    int SiteX,
    int SiteY,
    int SiteWidth,
    int SiteHeight,
    bool SourcePreserved,
    bool FocusPreserved);