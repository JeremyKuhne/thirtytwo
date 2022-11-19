// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum StockFont : uint
{
    OemFixed = GET_STOCK_OBJECT_FLAGS.OEM_FIXED_FONT,
    AnsiFixed = GET_STOCK_OBJECT_FLAGS.ANSI_FIXED_FONT,
    AnsiVariable = GET_STOCK_OBJECT_FLAGS.ANSI_VAR_FONT,
    System = GET_STOCK_OBJECT_FLAGS.SYSTEM_FONT,
    DeviceDefault = GET_STOCK_OBJECT_FLAGS.DEVICE_DEFAULT_FONT,
    SystemFixed = GET_STOCK_OBJECT_FLAGS.SYSTEM_FIXED_FONT,

    /// <summary>
    ///  Default font used for menus, dialog boxes, etc.
    /// </summary>
    GuiFont = GET_STOCK_OBJECT_FLAGS.DEFAULT_GUI_FONT
}
