// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum MessageType : uint
{
    Null = Interop.WM_NULL,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-create">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    Create = Interop.WM_CREATE,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-destroy">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    Destroy = Interop.WM_DESTROY,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-move">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    Move = Interop.WM_MOVE,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-size">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    Size = Interop.WM_SIZE,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-activate">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    Activate = Interop.WM_ACTIVATE,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-setfocus">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    SetFocus = Interop.WM_SETFOCUS,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/inputdev/wm-killfocus">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    KillFocus = Interop.WM_KILLFOCUS,

    Enable = Interop.WM_ENABLE,
    SetRedraw = Interop.WM_SETREDRAW,
    SetText = Interop.WM_SETTEXT,
    GetText = Interop.WM_GETTEXT,
    GetTextLength = Interop.WM_GETTEXTLENGTH,
    Paint = Interop.WM_PAINT,
    Close = Interop.WM_CLOSE,
    Quit = Interop.WM_QUIT,
    EraseBackground = Interop.WM_ERASEBKGND,
    SystemColorChange = Interop.WM_SYSCOLORCHANGE,
    ShowWindow = Interop.WM_SHOWWINDOW,
    WinIniChange = Interop.WM_WININICHANGE,
    SettingChange = Interop.WM_SETTINGCHANGE,
    DevModeChange = Interop.WM_DEVMODECHANGE,
    ActivateApp = Interop.WM_ACTIVATEAPP,
    FontChange = Interop.WM_FONTCHANGE,
    TimeChange = Interop.WM_TIMECHANGE,
    CancelMode = Interop.WM_CANCELMODE,
    SetCursor = Interop.WM_SETCURSOR,
    MouseActivate = Interop.WM_MOUSEACTIVATE,
    ChildActivate = Interop.WM_CHILDACTIVATE,
    QueueSync = Interop.WM_QUEUESYNC,
    GetMinMaxInfo = Interop.WM_GETMINMAXINFO,
    PaintIcon = Interop.WM_PAINTICON,
    IconEraseBackground = Interop.WM_ICONERASEBKGND,
    NextDialogControl = Interop.WM_NEXTDLGCTL,
    SpoolerStatus = Interop.WM_SPOOLERSTATUS,
    DrawItem = Interop.WM_DRAWITEM,
    MeasureItem = Interop.WM_MEASUREITEM,
    DeleteItem = Interop.WM_DELETEITEM,
    VirtualKeyToItem = Interop.WM_VKEYTOITEM,
    CharToItem = Interop.WM_CHARTOITEM,
    SetFont = Interop.WM_SETFONT,
    GetFont = Interop.WM_GETFONT,
    SetHotKey = Interop.WM_SETHOTKEY,
    GetHotKey = Interop.WM_GETHOTKEY,
    QueryDragIcon = Interop.WM_QUERYDRAGICON,
    CompareItem = Interop.WM_COMPAREITEM,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winauto/wm-getobject">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    GetObject = Interop.WM_GETOBJECT,

    /// <summary>
    ///  [WM_COMPACTING]
    /// </summary>
    Compacting = Interop.WM_COMPACTING,

    /// <summary>
    ///  [WM_COMMNOTIFY]
    /// </summary>
    CommNotify = Interop.WM_COMMNOTIFY,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-windowposchanging">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    WindowPositionChanging = Interop.WM_WINDOWPOSCHANGING,

    /// <remarks>
    ///  <para><see href="https://learn.microsoft.com/windows/win32/winmsg/wm-windowposchanged">Learn more about this message from learn.microsoft.com</see>.</para>
    /// </remarks>
    WindowPositionChanged = Interop.WM_WINDOWPOSCHANGED,

    Power = Interop.WM_POWER,
    CopyData = Interop.WM_COPYDATA,
    CancelJournal = Interop.WM_CANCELJOURNAL,
    Notify = Interop.WM_NOTIFY,
    InputLanguageChangeRequest = Interop.WM_INPUTLANGCHANGEREQUEST,
    InputLanguageChange = Interop.WM_INPUTLANGCHANGE,
    TrainingCard = Interop.WM_TCARD,
    Help = Interop.WM_HELP,
    UserChanged = Interop.WM_USERCHANGED,
    NotifyFormat = Interop.WM_NOTIFYFORMAT,
    ContextMenu = Interop.WM_CONTEXTMENU,
    StyleChanging = Interop.WM_STYLECHANGING,
    StyleChanged = Interop.WM_STYLECHANGED,
    DisplayChange = Interop.WM_DISPLAYCHANGE,
    GetIcon = Interop.WM_GETICON,
    SetIcon = Interop.WM_SETICON,
    NonClientCreate = Interop.WM_NCCREATE,
    NonClientDestroy = Interop.WM_NCDESTROY,
    NonClientCalculateSize = Interop.WM_NCCALCSIZE,
    NonClientHitTest = Interop.WM_NCHITTEST,
    NonClientPaint = Interop.WM_NCPAINT,
    NonClientActivate = Interop.WM_NCACTIVATE,
    GetDialogCode = Interop.WM_GETDLGCODE,
    SyncPaint = Interop.WM_SYNCPAINT,
    NonClientMouseMove = Interop.WM_NCMOUSEMOVE,
    NonClientLeftButtonDown = Interop.WM_NCLBUTTONDOWN,
    NonClientLeftButtonUp = Interop.WM_NCLBUTTONUP,
    NonClientLeftButtonDoubleClick = Interop.WM_NCLBUTTONDBLCLK,
    NonClientRightButtonDown = Interop.WM_NCRBUTTONDOWN,
    NonClientRightButtonUp = Interop.WM_NCRBUTTONUP,
    NonClientRightButtonDoubleClick = Interop.WM_NCRBUTTONDBLCLK,
    NonClientMiddleButtonDown = Interop.WM_NCMBUTTONDOWN,
    NonClientMiddleButtonUp = Interop.WM_NCMBUTTONUP,
    NonClientMiddleButtonDoubleClick = Interop.WM_NCMBUTTONDBLCLK,
    NonClientExtraButtonDown = Interop.WM_NCXBUTTONDOWN,
    NonClientExtraButtonUp = Interop.WM_NCXBUTTONUP,
    NonClientExtraButtonDoubleClick = Interop.WM_NCXBUTTONDBLCLK,
    InputDeviceChange = Interop.WM_INPUT_DEVICE_CHANGE,
    Input = Interop.WM_INPUT,
    KeyDown = Interop.WM_KEYDOWN,
    KeyUp = Interop.WM_KEYUP,
    Char = Interop.WM_CHAR,
    DeadChar = Interop.WM_DEADCHAR,
    SystemKeyDown = Interop.WM_SYSKEYDOWN,
    SystemKeyUp = Interop.WM_SYSKEYUP,
    SystemChar = Interop.WM_SYSCHAR,
    SystemDeadChar = Interop.WM_SYSDEADCHAR,
    UnicodeChar = Interop.WM_UNICHAR,
    ImeStartComposition = Interop.WM_IME_STARTCOMPOSITION,
    ImeEndComposition = Interop.WM_IME_ENDCOMPOSITION,
    ImeComposition = Interop.WM_IME_COMPOSITION,
    InitDialog = Interop.WM_INITDIALOG,
    Command = Interop.WM_COMMAND,
    SystemCommand = Interop.WM_SYSCOMMAND,
    Timer = Interop.WM_TIMER,
    HorizontalScroll = Interop.WM_HSCROLL,
    VerticalScroll = Interop.WM_VSCROLL,
    InitMenu = Interop.WM_INITMENU,
    InitMenuPopUp = Interop.WM_INITMENUPOPUP,
    Gesture = Interop.WM_GESTURE,
    GestureNotify = Interop.WM_GESTURENOTIFY,
    MenuSelect = Interop.WM_MENUSELECT,
    MenuChar = Interop.WM_MENUCHAR,
    EnterIdle = Interop.WM_ENTERIDLE,
    MenuRightButtonUp = Interop.WM_MENURBUTTONUP,
    MenuDrag = Interop.WM_MENUDRAG,
    MenuGetObject = Interop.WM_MENUGETOBJECT,
    UninitializeMenupPopUp = Interop.WM_UNINITMENUPOPUP,
    MenuCommand = Interop.WM_MENUCOMMAND,
    ChangeUIState = Interop.WM_CHANGEUISTATE,
    UpdateUIState = Interop.WM_UPDATEUISTATE,
    QueryUIState = Interop.WM_QUERYUISTATE,
    ControlColorMessageBox = Interop.WM_CTLCOLORMSGBOX,
    ControlColorEdit = Interop.WM_CTLCOLOREDIT,
    ControlColorListBox = Interop.WM_CTLCOLORLISTBOX,
    ControlColorButton = Interop.WM_CTLCOLORBTN,
    ControlColorDialog = Interop.WM_CTLCOLORDLG,
    ControlColorScrollBar = Interop.WM_CTLCOLORSCROLLBAR,
    ControlColorStatic = Interop.WM_CTLCOLORSTATIC,
    MouseMove = Interop.WM_MOUSEMOVE,
    LeftButtonDown = Interop.WM_LBUTTONDOWN,
    LeftButtonUp = Interop.WM_LBUTTONUP,
    LeftButtonDoubleClick = Interop.WM_LBUTTONDBLCLK,
    RightButtonDown = Interop.WM_RBUTTONDOWN,
    RightButtonUp = Interop.WM_RBUTTONUP,
    RightButtonDoubleClick = Interop.WM_RBUTTONDBLCLK,
    MiddleButtonDown = Interop.WM_MBUTTONDOWN,
    MiddleButtonUp = Interop.WM_MBUTTONUP,
    MiddleButtonDoubleClick = Interop.WM_MBUTTONDBLCLK,
    MouseWheel = Interop.WM_MOUSEWHEEL,
    ExtraButtonDown = Interop.WM_XBUTTONDOWN,
    ExtraButtonUp = Interop.WM_XBUTTONUP,
    ExtraButtonDoubleClick = Interop.WM_XBUTTONDBLCLK,
    MouseHorizontalWheel = Interop.WM_MOUSEHWHEEL,
    ParentNotify = Interop.WM_PARENTNOTIFY,
    EnterMenuLoop = Interop.WM_ENTERMENULOOP,
    ExitMenuLoop = Interop.WM_EXITMENULOOP,
    NextMenu = Interop.WM_NEXTMENU,
    Sizing = Interop.WM_SIZING,
    CaptureChanged = Interop.WM_CAPTURECHANGED,
    Moving = Interop.WM_MOVING,
    PowerBroadcast = Interop.WM_POWERBROADCAST,
    DeviceChange = Interop.WM_DEVICECHANGE,
    MdiCreate = Interop.WM_MDICREATE,
    MdiDestroy = Interop.WM_MDIDESTROY,
    MdiActivate = Interop.WM_MDIACTIVATE,
    MdiRestore = Interop.WM_MDIRESTORE,
    MdiNext = Interop.WM_MDINEXT,
    MdiMaximize = Interop.WM_MDIMAXIMIZE,
    MdiTile = Interop.WM_MDITILE,
    MdiCascade = Interop.WM_MDICASCADE,
    MdiIconArrange = Interop.WM_MDIICONARRANGE,
    MdiGetActive = Interop.WM_MDIGETACTIVE,
    MdiSetMenu = Interop.WM_MDISETMENU,
    EnterSizeMove = Interop.WM_ENTERSIZEMOVE,
    ExitSizeMove = Interop.WM_EXITSIZEMOVE,
    DropFiles = Interop.WM_DROPFILES,
    MdiRefreshMenu = Interop.WM_MDIREFRESHMENU,
    PointerDeviceChange = Interop.WM_POINTERDEVICECHANGE,
    PointDeviceInRange = Interop.WM_POINTERDEVICEINRANGE,
    PointerDeviceOutOfRange = Interop.WM_POINTERDEVICEOUTOFRANGE,
    Touch = Interop.WM_TOUCH,
    NonClientPointerUpdate = Interop.WM_NCPOINTERUPDATE,
    NonClientPointerDown = Interop.WM_NCPOINTERDOWN,
    NonClientPointerUp = Interop.WM_NCPOINTERUP,
    PointerUpdate = Interop.WM_POINTERUPDATE,
    PointerDown = Interop.WM_POINTERDOWN,
    PointerUp = Interop.WM_POINTERUP,
    PointerEnter = Interop.WM_POINTERENTER,
    PointerLeave = Interop.WM_POINTERLEAVE,
    PointerActivate = Interop.WM_POINTERACTIVATE,
    PointerCaptureChanged = Interop.WM_POINTERCAPTURECHANGED,
    TouchHitTesting = Interop.WM_TOUCHHITTESTING,
    PointerWheel = Interop.WM_POINTERWHEEL,
    PointerHorizontalWheel = Interop.WM_POINTERHWHEEL,
    PointerRoutedTo = Interop.WM_POINTERROUTEDTO,
    PointerRoutedAway = Interop.WM_POINTERROUTEDAWAY,
    PointerRoutedReleased = Interop.WM_POINTERROUTEDRELEASED,
    ImeSetContext = Interop.WM_IME_SETCONTEXT,
    ImeNotify = Interop.WM_IME_NOTIFY,
    ImeControl = Interop.WM_IME_CONTROL,
    ImeCompositionFull = Interop.WM_IME_COMPOSITIONFULL,
    ImeSelect = Interop.WM_IME_SELECT,
    ImeChar = Interop.WM_IME_CHAR,
    ImeRequest = Interop.WM_IME_REQUEST,
    ImeKeyDown = Interop.WM_IME_KEYDOWN,
    ImeKeyUp = Interop.WM_IME_KEYUP,
    MouseHover = Interop.WM_MOUSEHOVER,
    MouseLeave = Interop.WM_MOUSELEAVE,
    NonClientMouseHover = Interop.WM_NCMOUSEHOVER,
    NonClientMouseLeave = Interop.WM_NCMOUSELEAVE,
    WtsSessionChange = Interop.WM_WTSSESSION_CHANGE,
    DpiChanged = Interop.WM_DPICHANGED,
    DpiChangedBeforeParent = Interop.WM_DPICHANGED_BEFOREPARENT,
    DpiChangedAfterParent = Interop.WM_DPICHANGED_AFTERPARENT,
    GetDpiScaledSize = Interop.WM_GETDPISCALEDSIZE,
    Cut = Interop.WM_CUT,
    Copy = Interop.WM_COPY,
    Paste = Interop.WM_PASTE,
    Clear = Interop.WM_CLEAR,
    Undo = Interop.WM_UNDO,
    RenderFormat = Interop.WM_RENDERFORMAT,
    RenderAllFormats = Interop.WM_RENDERALLFORMATS,
    DestroyClipboard = Interop.WM_DESTROYCLIPBOARD,
    DrawClipboard = Interop.WM_DRAWCLIPBOARD,
    PaintClipboard = Interop.WM_PAINTCLIPBOARD,
    VerticalScrollClipboard = Interop.WM_VSCROLLCLIPBOARD,
    SizeClipboard = Interop.WM_SIZECLIPBOARD,
    AskClipboardFormatName = Interop.WM_ASKCBFORMATNAME,
    ChangeClipboardChain = Interop.WM_CHANGECBCHAIN,
    HorizontalScrollClipboard = Interop.WM_HSCROLLCLIPBOARD,
    QueryNewPalette = Interop.WM_QUERYNEWPALETTE,
    PaletteIsChanging = Interop.WM_PALETTEISCHANGING,
    PaletteChanged = Interop.WM_PALETTECHANGED,
    HotKey = Interop.WM_HOTKEY,
    Print = Interop.WM_PRINT,
    PrintClient = Interop.WM_PRINTCLIENT,
    AppCommand = Interop.WM_APPCOMMAND,
    ThemeChanged = Interop.WM_THEMECHANGED,
    ClipboardUpdate = Interop.WM_CLIPBOARDUPDATE,
    DwmCompositionChanged = Interop.WM_DWMCOMPOSITIONCHANGED,
    DwmNonClientRenderingChanged = Interop.WM_DWMNCRENDERINGCHANGED,
    DwmColorizationColorChanged = Interop.WM_DWMCOLORIZATIONCOLORCHANGED,
    DwmWindowMaximizedChange = Interop.WM_DWMWINDOWMAXIMIZEDCHANGE,
    DwmSendIconicThumbnail = Interop.WM_DWMSENDICONICTHUMBNAIL,
    DwmSendIconicLivePreviewBitmap = Interop.WM_DWMSENDICONICLIVEPREVIEWBITMAP,
    GetTitleBarInfo = Interop.WM_GETTITLEBARINFOEX
}