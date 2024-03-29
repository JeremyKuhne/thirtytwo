﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/object-roles">Object Roles</see>
///   documentation.
///  </para>
/// </remarks>
public enum ObjectRoles : int
{
    TitleBar = (int)Interop.ROLE_SYSTEM_TITLEBAR,
    MenuBar = (int)Interop.ROLE_SYSTEM_MENUBAR,
    ScrollBar = (int)Interop.ROLE_SYSTEM_SCROLLBAR,
    Grip = (int)Interop.ROLE_SYSTEM_GRIP,
    Sound = (int)Interop.ROLE_SYSTEM_SOUND,
    Cursor = (int)Interop.ROLE_SYSTEM_CURSOR,
    Caret = (int)Interop.ROLE_SYSTEM_CARET,
    Alert = (int)Interop.ROLE_SYSTEM_ALERT,
    Window = (int)Interop.ROLE_SYSTEM_WINDOW,
    Client = (int)Interop.ROLE_SYSTEM_CLIENT,
    MenuPopup = (int)Interop.ROLE_SYSTEM_MENUPOPUP,
    MenuItem = (int)Interop.ROLE_SYSTEM_MENUITEM,
    Tooltip = (int)Interop.ROLE_SYSTEM_TOOLTIP,
    Application = (int)Interop.ROLE_SYSTEM_APPLICATION,
    Document = (int)Interop.ROLE_SYSTEM_DOCUMENT,
    Pane = (int)Interop.ROLE_SYSTEM_PANE,
    Chart = (int)Interop.ROLE_SYSTEM_CHART,
    Dialog = (int)Interop.ROLE_SYSTEM_DIALOG,
    Border = (int)Interop.ROLE_SYSTEM_BORDER,
    Grouping = (int)Interop.ROLE_SYSTEM_GROUPING,
    Separator = (int)Interop.ROLE_SYSTEM_SEPARATOR,
    Toolbar = (int)Interop.ROLE_SYSTEM_TOOLBAR,
    Statusbar = (int)Interop.ROLE_SYSTEM_STATUSBAR,
    Table = (int)Interop.ROLE_SYSTEM_TABLE,
    ColumnHeader = (int)Interop.ROLE_SYSTEM_COLUMNHEADER,
    RowHeader = (int)Interop.ROLE_SYSTEM_ROWHEADER,
    Column = (int)Interop.ROLE_SYSTEM_COLUMN,
    Row = (int)Interop.ROLE_SYSTEM_ROW,
    Cell = (int)Interop.ROLE_SYSTEM_CELL,
    Link = (int)Interop.ROLE_SYSTEM_LINK,
    HelpBalloon = (int)Interop.ROLE_SYSTEM_HELPBALLOON,
    Character = (int)Interop.ROLE_SYSTEM_CHARACTER,
    List = (int)Interop.ROLE_SYSTEM_LIST,
    ListItem = (int)Interop.ROLE_SYSTEM_LISTITEM,
    Outline = (int)Interop.ROLE_SYSTEM_OUTLINE,
    OutlineItem = (int)Interop.ROLE_SYSTEM_OUTLINEITEM,
    PageTab = (int)Interop.ROLE_SYSTEM_PAGETAB,
    PropertyPage = (int)Interop.ROLE_SYSTEM_PROPERTYPAGE,
    Indicator = (int)Interop.ROLE_SYSTEM_INDICATOR,
    Graphic = (int)Interop.ROLE_SYSTEM_GRAPHIC,
    StaticText = (int)Interop.ROLE_SYSTEM_STATICTEXT,
    SystemText = (int)Interop.ROLE_SYSTEM_TEXT,
    PushButton = (int)Interop.ROLE_SYSTEM_PUSHBUTTON,
    CheckButton = (int)Interop.ROLE_SYSTEM_CHECKBUTTON,
    RadioButton = (int)Interop.ROLE_SYSTEM_RADIOBUTTON,
    ComboBox = (int)Interop.ROLE_SYSTEM_COMBOBOX,
    DropList = (int)Interop.ROLE_SYSTEM_DROPLIST,
    ProgressBar = (int)Interop.ROLE_SYSTEM_PROGRESSBAR,
    Dial = (int)Interop.ROLE_SYSTEM_DIAL,
    HotkeyField = (int)Interop.ROLE_SYSTEM_HOTKEYFIELD,
    Slider = (int)Interop.ROLE_SYSTEM_SLIDER,
    SpinButton = (int)Interop.ROLE_SYSTEM_SPINBUTTON,
    Diagram = (int)Interop.ROLE_SYSTEM_DIAGRAM,
    Animation = (int)Interop.ROLE_SYSTEM_ANIMATION,
    Equation = (int)Interop.ROLE_SYSTEM_EQUATION,
    ButtonDropDown = (int)Interop.ROLE_SYSTEM_BUTTONDROPDOWN,
    ButtonMenu = (int)Interop.ROLE_SYSTEM_BUTTONMENU,
    ButtonDropDownGrid = (int)Interop.ROLE_SYSTEM_BUTTONDROPDOWNGRID,
    WhiteSpace = (int)Interop.ROLE_SYSTEM_WHITESPACE,
    PageTabList = (int)Interop.ROLE_SYSTEM_PAGETABLIST,
    Clock = (int)Interop.ROLE_SYSTEM_CLOCK,
    SplitButton = (int)Interop.ROLE_SYSTEM_SPLITBUTTON,
    IPAddress = (int)Interop.ROLE_SYSTEM_IPADDRESS,
    OutlineButton = (int)Interop.ROLE_SYSTEM_OUTLINEBUTTON,
}