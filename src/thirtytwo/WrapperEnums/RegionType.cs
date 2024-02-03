// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum RegionType : int
{
    /// <summary>
    ///  An error occurred.
    /// </summary>
    Error = GDI_REGION_TYPE.RGN_ERROR,

    /// <summary>
    ///  Region is empty.
    /// </summary>
    Null = GDI_REGION_TYPE.NULLREGION,

    /// <summary>
    ///  Region is a single rectangle.
    /// </summary>
    Simple = GDI_REGION_TYPE.SIMPLEREGION,

    /// <summary>
    ///  Region is more than one rectangle.
    /// </summary>
    Complex = GDI_REGION_TYPE.COMPLEXREGION
}