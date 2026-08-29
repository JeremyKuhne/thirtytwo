// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <summary>
///  Identifies the Microsoft Active Accessibility (MSAA) role for an accessible object.
/// </summary>
/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/object-roles">Object Roles</see>
///   documentation.
///  </para>
/// </remarks>
public enum ObjectRoles : int
{
    /// <summary>
    ///  Window title bar region.
    /// </summary>
    TitleBar = (int)Interop.ROLE_SYSTEM_TITLEBAR,

    /// <summary>
    ///  Menu bar that contains top-level menus.
    /// </summary>
    MenuBar = (int)Interop.ROLE_SYSTEM_MENUBAR,

    /// <summary>
    ///  Vertical or horizontal scroll bar control.
    /// </summary>
    ScrollBar = (int)Interop.ROLE_SYSTEM_SCROLLBAR,

    /// <summary>
    ///  Resize grip, typically at a window corner.
    /// </summary>
    Grip = (int)Interop.ROLE_SYSTEM_GRIP,

    /// <summary>
    ///  Non-visual sound source indicator.
    /// </summary>
    Sound = (int)Interop.ROLE_SYSTEM_SOUND,

    /// <summary>
    ///  Mouse cursor indicator.
    /// </summary>
    Cursor = (int)Interop.ROLE_SYSTEM_CURSOR,

    /// <summary>
    ///  Text caret insertion point.
    /// </summary>
    Caret = (int)Interop.ROLE_SYSTEM_CARET,

    /// <summary>
    ///  Alert element used for important notifications.
    /// </summary>
    Alert = (int)Interop.ROLE_SYSTEM_ALERT,

    /// <summary>
    ///  Top-level application window.
    /// </summary>
    Window = (int)Interop.ROLE_SYSTEM_WINDOW,

    /// <summary>
    ///  Client area of a window.
    /// </summary>
    Client = (int)Interop.ROLE_SYSTEM_CLIENT,

    /// <summary>
    ///  Pop-up menu surface.
    /// </summary>
    MenuPopup = (int)Interop.ROLE_SYSTEM_MENUPOPUP,

    /// <summary>
    ///  Individual command in a menu.
    /// </summary>
    MenuItem = (int)Interop.ROLE_SYSTEM_MENUITEM,

    /// <summary>
    ///  Tooltip that provides contextual help.
    /// </summary>
    Tooltip = (int)Interop.ROLE_SYSTEM_TOOLTIP,

    /// <summary>
    ///  Application container element.
    /// </summary>
    Application = (int)Interop.ROLE_SYSTEM_APPLICATION,

    /// <summary>
    ///  Document content region.
    /// </summary>
    Document = (int)Interop.ROLE_SYSTEM_DOCUMENT,

    /// <summary>
    ///  Generic pane container.
    /// </summary>
    Pane = (int)Interop.ROLE_SYSTEM_PANE,

    /// <summary>
    ///  Chart or graph visualization.
    /// </summary>
    Chart = (int)Interop.ROLE_SYSTEM_CHART,

    /// <summary>
    ///  Dialog window.
    /// </summary>
    Dialog = (int)Interop.ROLE_SYSTEM_DIALOG,

    /// <summary>
    ///  Border decoration element.
    /// </summary>
    Border = (int)Interop.ROLE_SYSTEM_BORDER,

    /// <summary>
    ///  Grouping container for related elements.
    /// </summary>
    Grouping = (int)Interop.ROLE_SYSTEM_GROUPING,

    /// <summary>
    ///  Visual separator between items.
    /// </summary>
    Separator = (int)Interop.ROLE_SYSTEM_SEPARATOR,

    /// <summary>
    ///  Toolbar hosting command buttons.
    /// </summary>
    Toolbar = (int)Interop.ROLE_SYSTEM_TOOLBAR,

    /// <summary>
    ///  Status bar with state information.
    /// </summary>
    Statusbar = (int)Interop.ROLE_SYSTEM_STATUSBAR,

    /// <summary>
    ///  Table container.
    /// </summary>
    Table = (int)Interop.ROLE_SYSTEM_TABLE,

    /// <summary>
    ///  Table column header.
    /// </summary>
    ColumnHeader = (int)Interop.ROLE_SYSTEM_COLUMNHEADER,

    /// <summary>
    ///  Table row header.
    /// </summary>
    RowHeader = (int)Interop.ROLE_SYSTEM_ROWHEADER,

    /// <summary>
    ///  Table column.
    /// </summary>
    Column = (int)Interop.ROLE_SYSTEM_COLUMN,

    /// <summary>
    ///  Table row.
    /// </summary>
    Row = (int)Interop.ROLE_SYSTEM_ROW,

    /// <summary>
    ///  Table cell.
    /// </summary>
    Cell = (int)Interop.ROLE_SYSTEM_CELL,

    /// <summary>
    ///  Hyperlink element.
    /// </summary>
    Link = (int)Interop.ROLE_SYSTEM_LINK,

    /// <summary>
    ///  Help balloon pop-up.
    /// </summary>
    HelpBalloon = (int)Interop.ROLE_SYSTEM_HELPBALLOON,

    /// <summary>
    ///  Individual character element.
    /// </summary>
    Character = (int)Interop.ROLE_SYSTEM_CHARACTER,

    /// <summary>
    ///  List container.
    /// </summary>
    List = (int)Interop.ROLE_SYSTEM_LIST,

    /// <summary>
    ///  Item within a list.
    /// </summary>
    ListItem = (int)Interop.ROLE_SYSTEM_LISTITEM,

    /// <summary>
    ///  Outline or tree container.
    /// </summary>
    Outline = (int)Interop.ROLE_SYSTEM_OUTLINE,

    /// <summary>
    ///  Item within an outline or tree.
    /// </summary>
    OutlineItem = (int)Interop.ROLE_SYSTEM_OUTLINEITEM,

    /// <summary>
    ///  Individual tab page selector.
    /// </summary>
    PageTab = (int)Interop.ROLE_SYSTEM_PAGETAB,

    /// <summary>
    ///  Property page content.
    /// </summary>
    PropertyPage = (int)Interop.ROLE_SYSTEM_PROPERTYPAGE,

    /// <summary>
    ///  Visual indicator such as a marker.
    /// </summary>
    Indicator = (int)Interop.ROLE_SYSTEM_INDICATOR,

    /// <summary>
    ///  Graphic or image element.
    /// </summary>
    Graphic = (int)Interop.ROLE_SYSTEM_GRAPHIC,

    /// <summary>
    ///  Static text label.
    /// </summary>
    StaticText = (int)Interop.ROLE_SYSTEM_STATICTEXT,

    /// <summary>
    ///  Editable or read-only text content element.
    /// </summary>
    SystemText = (int)Interop.ROLE_SYSTEM_TEXT,

    /// <summary>
    ///  Push button command control.
    /// </summary>
    PushButton = (int)Interop.ROLE_SYSTEM_PUSHBUTTON,

    /// <summary>
    ///  Check box style toggle control.
    /// </summary>
    CheckButton = (int)Interop.ROLE_SYSTEM_CHECKBUTTON,

    /// <summary>
    ///  Radio button option control.
    /// </summary>
    RadioButton = (int)Interop.ROLE_SYSTEM_RADIOBUTTON,

    /// <summary>
    ///  Combo box with editable and/or selectable content.
    /// </summary>
    ComboBox = (int)Interop.ROLE_SYSTEM_COMBOBOX,

    /// <summary>
    ///  Drop-down list control.
    /// </summary>
    DropList = (int)Interop.ROLE_SYSTEM_DROPLIST,

    /// <summary>
    ///  Progress indicator bar.
    /// </summary>
    ProgressBar = (int)Interop.ROLE_SYSTEM_PROGRESSBAR,

    /// <summary>
    ///  Rotary dial control.
    /// </summary>
    Dial = (int)Interop.ROLE_SYSTEM_DIAL,

    /// <summary>
    ///  Hotkey entry field.
    /// </summary>
    HotkeyField = (int)Interop.ROLE_SYSTEM_HOTKEYFIELD,

    /// <summary>
    ///  Slider (track bar) control.
    /// </summary>
    Slider = (int)Interop.ROLE_SYSTEM_SLIDER,

    /// <summary>
    ///  Spin button (up-down) control.
    /// </summary>
    SpinButton = (int)Interop.ROLE_SYSTEM_SPINBUTTON,

    /// <summary>
    ///  Diagram or schematic content.
    /// </summary>
    Diagram = (int)Interop.ROLE_SYSTEM_DIAGRAM,

    /// <summary>
    ///  Animated content.
    /// </summary>
    Animation = (int)Interop.ROLE_SYSTEM_ANIMATION,

    /// <summary>
    ///  Mathematical equation content.
    /// </summary>
    Equation = (int)Interop.ROLE_SYSTEM_EQUATION,

    /// <summary>
    ///  Button that opens a drop-down.
    /// </summary>
    ButtonDropDown = (int)Interop.ROLE_SYSTEM_BUTTONDROPDOWN,

    /// <summary>
    ///  Button that opens a menu.
    /// </summary>
    ButtonMenu = (int)Interop.ROLE_SYSTEM_BUTTONMENU,

    /// <summary>
    ///  Button that opens a grid-style drop-down.
    /// </summary>
    ButtonDropDownGrid = (int)Interop.ROLE_SYSTEM_BUTTONDROPDOWNGRID,

    /// <summary>
    ///  Whitespace content element.
    /// </summary>
    WhiteSpace = (int)Interop.ROLE_SYSTEM_WHITESPACE,

    /// <summary>
    ///  Container for tab selectors.
    /// </summary>
    PageTabList = (int)Interop.ROLE_SYSTEM_PAGETABLIST,

    /// <summary>
    ///  Clock display element.
    /// </summary>
    Clock = (int)Interop.ROLE_SYSTEM_CLOCK,

    /// <summary>
    ///  Split button with primary and secondary actions.
    /// </summary>
    SplitButton = (int)Interop.ROLE_SYSTEM_SPLITBUTTON,

    /// <summary>
    ///  IP address entry field.
    /// </summary>
    IPAddress = (int)Interop.ROLE_SYSTEM_IPADDRESS,

    /// <summary>
    ///  Expand/collapse button for an outline node.
    /// </summary>
    OutlineButton = (int)Interop.ROLE_SYSTEM_OUTLINEBUTTON,
}