// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Identifies system color indexes used by Win32 APIs.
/// </summary>
public enum SystemColor : int
{
    /// <summary>
    ///  Scroll bar background area.
    /// </summary>
    ScrollBar = SYS_COLOR_INDEX.COLOR_SCROLLBAR,

    /// <summary>
    ///  The desktop color.
    /// </summary>
    Background = SYS_COLOR_INDEX.COLOR_BACKGROUND,

    /// <summary>
    ///  Active window title bar. When title bar gradients are enabled, this is the left-side color.
    /// </summary>
    ActiveCaption = SYS_COLOR_INDEX.COLOR_ACTIVECAPTION,

    /// <summary>
    ///  Inactive window title bar. When title bar gradients are enabled, this is the left-side color.
    /// </summary>
    InactiveCaption = SYS_COLOR_INDEX.COLOR_INACTIVECAPTION,

    /// <summary>
    ///  Menu background color.
    /// </summary>
    Menu = SYS_COLOR_INDEX.COLOR_MENU,

    /// <summary>
    ///  Window background color.
    /// </summary>
    Window = SYS_COLOR_INDEX.COLOR_WINDOW,

    /// <summary>
    ///  Window frame color.
    /// </summary>
    WindowFrame = SYS_COLOR_INDEX.COLOR_WINDOWFRAME,

    /// <summary>
    ///  Text color used in menus.
    /// </summary>
    MenuText = SYS_COLOR_INDEX.COLOR_MENUTEXT,

    /// <summary>
    ///  Text color used in windows.
    /// </summary>
    WindowText = SYS_COLOR_INDEX.COLOR_WINDOWTEXT,

    /// <summary>
    ///  Text color used in captions, size boxes, and scroll bar arrow boxes.
    /// </summary>
    CaptionText = SYS_COLOR_INDEX.COLOR_CAPTIONTEXT,

    /// <summary>
    ///  Active window border color.
    /// </summary>
    ActiveBorder = SYS_COLOR_INDEX.COLOR_ACTIVEBORDER,

    /// <summary>
    ///  Inactive window border color.
    /// </summary>
    InactiveBorder = SYS_COLOR_INDEX.COLOR_INACTIVEBORDER,

    /// <summary>
    ///  Background color of multiple-document interface (MDI) applications.
    /// </summary>
    AppWorkspace = SYS_COLOR_INDEX.COLOR_APPWORKSPACE,

    /// <summary>
    ///  Background color of selected items in a control.
    /// </summary>
    Highlight = SYS_COLOR_INDEX.COLOR_HIGHLIGHT,

    /// <summary>
    ///  Text color of selected items in a control.
    /// </summary>
    HightlightText = SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT,

    /// <summary>
    ///  Face color for three-dimensional display elements and dialog box backgrounds.
    /// </summary>
    ButtonFace = SYS_COLOR_INDEX.COLOR_BTNFACE,

    /// <summary>
    ///  Shadow color for edges of three-dimensional display elements that face away from the light source.
    /// </summary>
    ButtonShadow = SYS_COLOR_INDEX.COLOR_BTNSHADOW,

    /// <summary>
    ///  Grayed or disabled text color.
    /// </summary>
    GrayText = SYS_COLOR_INDEX.COLOR_GRAYTEXT,

    /// <summary>
    ///  Text color used on push buttons.
    /// </summary>
    ButtonText = SYS_COLOR_INDEX.COLOR_BTNTEXT,

    /// <summary>
    ///  Text color used in an inactive window title bar.
    /// </summary>
    InactiveCaptionText = SYS_COLOR_INDEX.COLOR_INACTIVECAPTIONTEXT,

    /// <summary>
    ///  Highlight color for edges of three-dimensional display elements that face the light source.
    /// </summary>
    ButtonHighlight = SYS_COLOR_INDEX.COLOR_BTNHIGHLIGHT,

    /// <summary>
    ///  Dark shadow color for three-dimensional display elements.
    /// </summary>
    DarkShadow3d = SYS_COLOR_INDEX.COLOR_3DDKSHADOW,

    /// <summary>
    ///  Light color for edges of three-dimensional display elements that face the light source.
    /// </summary>
    Light3d = SYS_COLOR_INDEX.COLOR_3DLIGHT,

    /// <summary>
    ///  Text color used by tooltip controls.
    /// </summary>
    InfoText = SYS_COLOR_INDEX.COLOR_INFOTEXT,

    /// <summary>
    ///  Background color used by tooltip controls.
    /// </summary>
    InfoBackground = SYS_COLOR_INDEX.COLOR_INFOBK,

    /// <summary>
    ///  Color used for hyperlinks and hot-tracked items.
    /// </summary>
    HotLight = SYS_COLOR_INDEX.COLOR_HOTLIGHT,

    /// <summary>
    ///  Right-side color in the gradient of an active window title bar.
    /// </summary>
    GradientActiveCaption = SYS_COLOR_INDEX.COLOR_GRADIENTACTIVECAPTION,

    /// <summary>
    ///  Right-side color in the gradient of an inactive window title bar.
    /// </summary>
    GradientInactiveCaption = SYS_COLOR_INDEX.COLOR_GRADIENTINACTIVECAPTION,

    /// <summary>
    ///  Color used to highlight menu items when menus use the flat-menu appearance.
    /// </summary>
    MenuHighlight = SYS_COLOR_INDEX.COLOR_MENUHILIGHT,

    /// <summary>
    ///  Menu bar background color when menus use the flat-menu appearance.
    /// </summary>
    MenuBar = SYS_COLOR_INDEX.COLOR_MENUBAR,

    /// <summary>
    ///  Desktop color.
    /// </summary>
    Desktop = SYS_COLOR_INDEX.COLOR_DESKTOP,

    /// <summary>
    ///  Face color for three-dimensional display elements and dialog box backgrounds.
    /// </summary>
    Face3d = SYS_COLOR_INDEX.COLOR_3DFACE,

    /// <summary>
    ///  Shadow color for edges of three-dimensional display elements that face away from the light source.
    /// </summary>
    Shadow3d = SYS_COLOR_INDEX.COLOR_3DSHADOW,

    /// <summary>
    ///  Highlight color for edges of three-dimensional display elements that face the light source.
    /// </summary>
    Highlight3d = SYS_COLOR_INDEX.COLOR_3DHIGHLIGHT
}