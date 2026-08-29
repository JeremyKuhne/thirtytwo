// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Identifies predefined stock pen objects.
/// </summary>
public enum StockPen : uint
{
    /// <summary>
    ///  White pen.
    /// </summary>
    White = GET_STOCK_OBJECT_FLAGS.WHITE_PEN,

    /// <summary>
    ///  Black pen.
    /// </summary>
    Black = GET_STOCK_OBJECT_FLAGS.BLACK_PEN,

    /// <summary>
    ///  Null pen, which draws nothing.
    /// </summary>
    Null = GET_STOCK_OBJECT_FLAGS.NULL_PEN,

    /// <summary>
    ///  Solid-color pen whose color defaults to black and can be changed with <c>SetDCPenColor</c>.
    /// </summary>
    DeviceContext = GET_STOCK_OBJECT_FLAGS.DC_PEN
}