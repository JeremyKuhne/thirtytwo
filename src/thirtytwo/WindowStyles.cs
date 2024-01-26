// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

[Flags]
public enum WindowStyles : uint
{
    /// <summary>
    ///  Overlapped window with a title bar and border.
    /// </summary>
    Overlapped = WINDOW_STYLE.WS_OVERLAPPED,

    /// <summary>
    ///  Pop-up window. Cannot be used with <see cref="Child"/>.
    /// </summary>
    PopUp = WINDOW_STYLE.WS_POPUP,

    /// <summary>
    ///  Child window. Cannot have a menu bar or be used with PopUp.
    /// </summary>
    Child = WINDOW_STYLE.WS_CHILD,

    /// <summary>
    ///  Initially minimized.
    /// </summary>
    Minimize = WINDOW_STYLE.WS_MINIMIZE,

    /// <summary>
    ///  Initialy visible.
    /// </summary>
    Visible = WINDOW_STYLE.WS_VISIBLE,

    /// <summary>
    ///  Initially disabled.
    /// </summary>
    Diabled = WINDOW_STYLE.WS_DISABLED,

    /// <summary>
    ///  Clips child windows relative to each other, preventing drawing in each-other
    ///  when overlapping.
    /// </summary>
    ClipSiblings = WINDOW_STYLE.WS_CLIPSIBLINGS,

    /// <summary>
    ///  Clips out child windows when drawing in parent's client area.
    /// </summary>
    ClipChildren = WINDOW_STYLE.WS_CLIPCHILDREN,

    /// <summary>
    ///  Initially Maximized.
    /// </summary>
    Maximize = WINDOW_STYLE.WS_MAXIMIZE,

    /// <summary>
    ///  Has title bar and border.
    /// </summary>
    Caption = WINDOW_STYLE.WS_CAPTION,

    /// <summary>
    ///  Has a thin-line border.
    /// </summary>
    Border = WINDOW_STYLE.WS_BORDER,

    /// <summary>
    ///  Has a dialog style border. Cannot have a title bar.
    /// </summary>
    DialogFrame = WINDOW_STYLE.WS_DLGFRAME,

    /// <summary>
    ///  Has a vertical scroll bar.
    /// </summary>
    VerticalScroll = WINDOW_STYLE.WS_HSCROLL,

    /// <summary>
    ///  Has a horizontal scroll bar.
    /// </summary>
    HorizontalScroll = WINDOW_STYLE.WS_HSCROLL,

    /// <summary>
    ///  Has a window menu in the title bar. Must also specify <see cref="Caption"/>.
    /// </summary>
    SystemMenu = WINDOW_STYLE.WS_SYSMENU,

    /// <summary>
    ///  Has a sizing border.
    /// </summary>
    ThickFrame = WINDOW_STYLE.WS_THICKFRAME,

    /// <summary>
    ///  First control of a group of controls.
    /// </summary>
    Group = WINDOW_STYLE.WS_GROUP,

    /// <summary>
    ///  Is a control that can recieve focus via the TAB key.
    /// </summary>
    TabStop = WINDOW_STYLE.WS_TABSTOP,

    /// <summary>
    ///  Has a minimize button. Cannot be used with <see cref="ExtendedWindowStyles.ContextHelp"/>.
    ///  Must also have <see cref="SystemMenu"/> style.
    /// </summary>
    MinimizeBox = WINDOW_STYLE.WS_MINIMIZEBOX,

    /// <summary>
    ///  Has a maximize button. Cannot be used with <see cref="ExtendedWindowStyles.ContextHelp"/>.
    ///  Must also have <see cref="SystemMenu"/> style.
    /// </summary>
    MaximizeBox = WINDOW_STYLE.WS_MAXIMIZEBOX,

    /// <summary>
    ///  Same as <see cref="Overlapped"/>.
    /// </summary>
    Tiled = WINDOW_STYLE.WS_TILED,

    /// <summary>
    ///  Same as <see cref="Minimize"/>.
    /// </summary>
    Iconic = WINDOW_STYLE.WS_ICONIC,

    /// <summary>
    ///  Same as <see cref="ThickFrame"/>.
    /// </summary>
    SizeBox = WINDOW_STYLE.WS_SIZEBOX,

    /// <summary>
    ///  Same as <see cref="OverlappedWindow"/>.
    /// </summary>
    TiledWindow = WINDOW_STYLE.WS_TILEDWINDOW,

    /// <summary>
    ///  Standard overlapped window style.
    /// </summary>
    OverlappedWindow = WINDOW_STYLE.WS_OVERLAPPEDWINDOW,

    /// <summary>
    ///  Standard pop-up window style.
    /// </summary>
    PopUpWindow = WINDOW_STYLE.WS_POPUPWINDOW,

    /// <summary>
    ///  Same as <see cref="Child"/>.
    /// </summary>
    ChildWindow = WINDOW_STYLE.WS_CHILDWINDOW
}