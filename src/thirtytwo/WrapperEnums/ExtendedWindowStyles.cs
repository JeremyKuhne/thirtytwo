// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

[Flags]
public enum ExtendedWindowStyles : uint
{
    /// <summary>
    ///  Default styles are standard left-to-right defaults. RTL styles are overrides and are defined
    ///  below. Includes <see cref="Left"/>, <see cref="LeftToRightReading"/>, and <see cref="RightScrollBar"/>.
    /// </summary>
    Default = 0,

    Left = WINDOW_EX_STYLE.WS_EX_LEFT,

    LeftToRightReading = WINDOW_EX_STYLE.WS_EX_LTRREADING,

    RightScrollBar = WINDOW_EX_STYLE.WS_EX_RIGHTSCROLLBAR,

    /// <summary>
    ///  Window has double border.
    /// </summary>
    DialogModalFrame = WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME,

    /// <summary>
    ///  The child window created with this style does not send the WM_PARENTNOTIFY message to its parent
    ///  window when it is created or destroyed.
    /// </summary>
    NoParentNotify = WINDOW_EX_STYLE.WS_EX_NOPARENTNOTIFY,

    /// <summary>
    ///  The window should be placed above all non-topmost windows and should stay above them, even when
    ///  the window is deactivated.
    /// </summary>
    TopMost = WINDOW_EX_STYLE.WS_EX_TOPMOST,

    /// <summary>
    ///  The window accepts drag-drop files.
    /// </summary>
    AcceptFiles = WINDOW_EX_STYLE.WS_EX_ACCEPTFILES,

    /// <summary>
    ///  The window should not be painted until siblings beneath the window (that were created by the
    ///  same thread) have been painted.
    /// </summary>
    Transparent = WINDOW_EX_STYLE.WS_EX_TRANSPARENT,

    /// <summary>
    ///  The window is a MDI child window.
    /// </summary>
    MdiChild = WINDOW_EX_STYLE.WS_EX_MDICHILD,

    /// <summary>
    ///  The window is intended to be used as a floating toolbar.
    /// </summary>
    ToolWindow = WINDOW_EX_STYLE.WS_EX_TOOLWINDOW,

    /// <summary>
    ///  The window has a border with a raised edge.
    /// </summary>
    WindowEdge = WINDOW_EX_STYLE.WS_EX_WINDOWEDGE,

    /// <summary>
    ///  The window has a border with a sunken edge.
    /// </summary>
    ClientEdge = WINDOW_EX_STYLE.WS_EX_CLIENTEDGE,

    /// <summary>
    ///  The title bar of the window includes a question mark.
    /// </summary>
    ContextHelp = WINDOW_EX_STYLE.WS_EX_CONTEXTHELP,

    /// <summary>
    ///  The window has generic "right-aligned" properties.
    /// </summary>
    Right = WINDOW_EX_STYLE.WS_EX_RIGHT,

    /// <summary>
    ///  If the shell language supports reading-order alignment, the window text is displayed using
    ///  right-to-left reading-order properties.
    /// </summary>
    RtlReading = WINDOW_EX_STYLE.WS_EX_RTLREADING,

    /// <summary>
    ///  If the shell language supports reading-order alignment, the vertical scroll bar (if
    ///  present) will be on the left instead of the right.
    /// </summary>
    LeftScrollBar = WINDOW_EX_STYLE.WS_EX_LEFTSCROLLBAR,

    /// <summary>
    ///  The window itself contains child windows that should take part in dialog box navigation.
    /// </summary>
    ControlParent = WINDOW_EX_STYLE.WS_EX_CONTROLPARENT,

    /// <summary>
    ///  The window has a three-dimensional border style intended to be used for items that do not
    ///  accept user input.
    /// </summary>
    StaticEdge = WINDOW_EX_STYLE.WS_EX_STATICEDGE,

    /// <summary>
    ///  Forces a top-level window onto the taskbar when the window is visible.
    /// </summary>
    AppWindow = WINDOW_EX_STYLE.WS_EX_APPWINDOW,

    OverlappedWindow = WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW,

    PaletteWindow = WINDOW_EX_STYLE.WS_EX_PALETTEWINDOW,

    /// <summary>
    ///  The window is a layered window.
    /// </summary>
    Layered = WINDOW_EX_STYLE.WS_EX_LAYERED,

    /// <summary>
    ///  The window does not pass its window layout to its child windows.
    /// </summary>
    NoInheritLayout = WINDOW_EX_STYLE.WS_EX_NOINHERITLAYOUT,

    /// <summary>
    ///  The window does not render to a redirection surface.
    /// </summary>
    NoRedirectionBitmap = WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP,

    LayoutRtl = WINDOW_EX_STYLE.WS_EX_LAYOUTRTL,

    /// <summary>
    ///  Paints all descendants of a window in bottom-to-top painting order using double-buffering.
    /// </summary>
    Composited = WINDOW_EX_STYLE.WS_EX_COMPOSITED,

    /// <summary>
    ///  A top-level window created with this style does not become the foreground window when the
    ///  user clicks it.
    /// </summary>
    NoActivate = WINDOW_EX_STYLE.WS_EX_NOACTIVATE
}