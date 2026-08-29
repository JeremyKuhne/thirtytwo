// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Identifies predefined stock font objects.
/// </summary>
public enum StockFont : uint
{
    /// <summary>
    ///  Original equipment manufacturer (OEM)-dependent fixed-pitch system font.
    /// </summary>
    OemFixed = GET_STOCK_OBJECT_FLAGS.OEM_FIXED_FONT,

    /// <summary>
    ///  Windows fixed-pitch system font.
    /// </summary>
    AnsiFixed = GET_STOCK_OBJECT_FLAGS.ANSI_FIXED_FONT,

    /// <summary>
    ///  Windows variable-pitch system font.
    /// </summary>
    AnsiVariable = GET_STOCK_OBJECT_FLAGS.ANSI_VAR_FONT,

    /// <summary>
    ///  System font used by default to draw menus, dialog box controls, and text.
    /// </summary>
    System = GET_STOCK_OBJECT_FLAGS.SYSTEM_FONT,

    /// <summary>
    ///  Device-dependent font.
    /// </summary>
    DeviceDefault = GET_STOCK_OBJECT_FLAGS.DEVICE_DEFAULT_FONT,

    /// <summary>
    ///  Fixed-pitch system font retained for compatibility with 16-bit Windows versions earlier than 3.0.
    /// </summary>
    SystemFixed = GET_STOCK_OBJECT_FLAGS.SYSTEM_FIXED_FONT,

    /// <summary>
    ///  Default font for user-interface objects such as menus and dialog boxes.
    /// </summary>
    GuiFont = GET_STOCK_OBJECT_FLAGS.DEFAULT_GUI_FONT
}