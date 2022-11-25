// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum StockBrush : uint
{
    White = GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH,
    LightGray = GET_STOCK_OBJECT_FLAGS.LTGRAY_BRUSH,
    Gray = GET_STOCK_OBJECT_FLAGS.GRAY_BRUSH,
    DarkGray = GET_STOCK_OBJECT_FLAGS.DKGRAY_BRUSH,
    Black = GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH,
    Hollow = GET_STOCK_OBJECT_FLAGS.HOLLOW_BRUSH,
#pragma warning disable CA1069 // Enums values should not be duplicated
    Null = GET_STOCK_OBJECT_FLAGS.NULL_BRUSH,
#pragma warning restore CA1069

    /// <summary>
    ///  Device context brush. Color is changed via Get/SetDeviceContextBrushColor.
    /// </summary>
    DeviceContextBrush = GET_STOCK_OBJECT_FLAGS.DC_BRUSH
}