// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Specifies how to fill polygons with self-intersections.
/// </summary>
public enum PolyFillMode : uint
{
    /// <summary>
    ///  Fills the area between odd-numbered and even-numbered polygon sides on each scan line.
    /// </summary>
    Alternate = CREATE_POLYGON_RGN_MODE.ALTERNATE,

    /// <summary>
    ///  Fills each region whose winding value is nonzero; the direction of each polygon edge affects that value.
    /// </summary>
    Winding = CREATE_POLYGON_RGN_MODE.WINDING
}