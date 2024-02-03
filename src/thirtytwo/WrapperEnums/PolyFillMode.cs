// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum PolyFillMode : uint
{
    Alternate = CREATE_POLYGON_RGN_MODE.ALTERNATE,
    Winding = CREATE_POLYGON_RGN_MODE.WINDING
}