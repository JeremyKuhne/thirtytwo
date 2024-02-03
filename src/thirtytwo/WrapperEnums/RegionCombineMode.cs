// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum RegionCombineMode : int
{
    /// <summary>
    ///  Creates the intersection of the two combined regions.
    /// </summary>
    And = RGN_COMBINE_MODE.RGN_AND,

    /// <summary>
    ///  Creates the union of two combined regions.
    /// </summary>
    Or = RGN_COMBINE_MODE.RGN_OR,

    /// <summary>
    ///  Creates the union of two combined regions except for any overlapping areas.
    /// </summary>
    Xor = RGN_COMBINE_MODE.RGN_XOR,

    /// <summary>
    ///  Creates a new region consisting of the areas that are not common to both regions.
    /// </summary>
    Diff = RGN_COMBINE_MODE.RGN_DIFF,

    /// <summary>
    ///  Creates a copy of the region identified by the first region handle.
    /// </summary>
    Copy = RGN_COMBINE_MODE.RGN_COPY
}