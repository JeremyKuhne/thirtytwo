// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum StockPen : uint
{
    White = GET_STOCK_OBJECT_FLAGS.WHITE_PEN,
    Black = GET_STOCK_OBJECT_FLAGS.BLACK_PEN,
    Null = GET_STOCK_OBJECT_FLAGS.NULL_PEN,
    DeviceContext = GET_STOCK_OBJECT_FLAGS.DC_PEN
}