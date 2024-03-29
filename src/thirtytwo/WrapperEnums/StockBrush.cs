﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum StockBrush : uint
{
    White = GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH,
    LightGray = GET_STOCK_OBJECT_FLAGS.LTGRAY_BRUSH,
    Gray = GET_STOCK_OBJECT_FLAGS.GRAY_BRUSH,
    DarkGray = GET_STOCK_OBJECT_FLAGS.DKGRAY_BRUSH,
    Black = GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH,

    /// <summary>
    ///  Used when a brush handle is required, but no painting is needed.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://devblogs.microsoft.com/oldnewthing/20040126-00/?p=40903">The hollow brush</see>
    ///  </para>
    /// </remarks>
    Hollow = GET_STOCK_OBJECT_FLAGS.HOLLOW_BRUSH,

    /// <inheritdoc cref="Hollow"/>
    Null = GET_STOCK_OBJECT_FLAGS.NULL_BRUSH,

    /// <summary>
    ///  Device context brush. Color is changed via Get/SetDeviceContextBrushColor.
    /// </summary>
    DeviceContextBrush = GET_STOCK_OBJECT_FLAGS.DC_BRUSH
}