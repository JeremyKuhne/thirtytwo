// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Identifies predefined stock brush objects.
/// </summary>
public enum StockBrush : uint
{
    /// <summary>
    ///  White brush.
    /// </summary>
    White = GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH,

    /// <summary>
    ///  Light gray brush.
    /// </summary>
    LightGray = GET_STOCK_OBJECT_FLAGS.LTGRAY_BRUSH,

    /// <summary>
    ///  Gray brush.
    /// </summary>
    Gray = GET_STOCK_OBJECT_FLAGS.GRAY_BRUSH,

    /// <summary>
    ///  Dark gray brush.
    /// </summary>
    DarkGray = GET_STOCK_OBJECT_FLAGS.DKGRAY_BRUSH,

    /// <summary>
    ///  Black brush.
    /// </summary>
    Black = GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH,

    /// <summary>
    ///  Hollow brush, equivalent to <see cref="Null"/>, which performs no fill.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://devblogs.microsoft.com/oldnewthing/20040126-00/?p=40903">The hollow brush</see>
    ///  </para>
    /// </remarks>
    Hollow = GET_STOCK_OBJECT_FLAGS.HOLLOW_BRUSH,

    /// <summary>
    ///  Null brush, equivalent to <see cref="Hollow"/>, which performs no fill.
    /// </summary>
    Null = GET_STOCK_OBJECT_FLAGS.NULL_BRUSH,

    /// <summary>
    ///  Solid-color brush whose color defaults to white and can be changed with <c>SetDCBrushColor</c>.
    /// </summary>
    DeviceContextBrush = GET_STOCK_OBJECT_FLAGS.DC_BRUSH
}