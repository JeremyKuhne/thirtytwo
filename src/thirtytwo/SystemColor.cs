// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

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

    ActiveCaption = SYS_COLOR_INDEX.COLOR_ACTIVECAPTION,

    InactiveCaption = SYS_COLOR_INDEX.COLOR_INACTIVECAPTION,

    Menu = SYS_COLOR_INDEX.COLOR_MENU,

    Window = SYS_COLOR_INDEX.COLOR_WINDOW,

    WindowFrame = SYS_COLOR_INDEX.COLOR_WINDOWFRAME,

    MenuText = SYS_COLOR_INDEX.COLOR_MENUTEXT,

    WindowText = SYS_COLOR_INDEX.COLOR_WINDOWTEXT,

    CaptionText = SYS_COLOR_INDEX.COLOR_CAPTIONTEXT,

    ActiveBorder = SYS_COLOR_INDEX.COLOR_ACTIVEBORDER,

    InactiveBorder = SYS_COLOR_INDEX.COLOR_INACTIVEBORDER,

    AppWorkspace = SYS_COLOR_INDEX.COLOR_APPWORKSPACE,

    Highlight = SYS_COLOR_INDEX.COLOR_HIGHLIGHT,

    HightlightText = SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT,

    ButtonFace = SYS_COLOR_INDEX.COLOR_BTNFACE,

    ButtonShadow = SYS_COLOR_INDEX.COLOR_BTNSHADOW,

    GrayText = SYS_COLOR_INDEX.COLOR_GRAYTEXT,

    ButtonText = SYS_COLOR_INDEX.COLOR_BTNTEXT,

    InactiveCaptionText = SYS_COLOR_INDEX.COLOR_INACTIVECAPTIONTEXT,

    ButtonHighlight = SYS_COLOR_INDEX.COLOR_BTNHIGHLIGHT,

    DarkShadow3d = SYS_COLOR_INDEX.COLOR_3DDKSHADOW,

    Light3d = SYS_COLOR_INDEX.COLOR_3DLIGHT,

    InfoText = SYS_COLOR_INDEX.COLOR_INFOTEXT,

    InfoBackground = SYS_COLOR_INDEX.COLOR_INFOBK,

    HotLight = SYS_COLOR_INDEX.COLOR_HOTLIGHT,

    GradientActiveCaption = SYS_COLOR_INDEX.COLOR_GRADIENTACTIVECAPTION,

    GradientInactiveCaption = SYS_COLOR_INDEX.COLOR_GRADIENTINACTIVECAPTION,

    MenuHighlight = SYS_COLOR_INDEX.COLOR_MENUHILIGHT,

    MenuBar = SYS_COLOR_INDEX.COLOR_MENUBAR,

#pragma warning disable CA1069 // Enums values should not be duplicated
    Desktop = SYS_COLOR_INDEX.COLOR_DESKTOP,

    Face3d = SYS_COLOR_INDEX.COLOR_3DFACE,

    Shadow3d = SYS_COLOR_INDEX.COLOR_3DSHADOW,

    Highlight3d = SYS_COLOR_INDEX.COLOR_3DHIGHLIGHT
#pragma warning restore CA1069 // Enums values should not be duplicated
}
