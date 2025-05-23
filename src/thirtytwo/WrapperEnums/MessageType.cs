// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public enum MessageType : uint
{
    Null = Interop.WM_NULL,

    /// <inheritdoc cref="Interop.WM_CREATE"/>
    Create = Interop.WM_CREATE,

    /// <inheritdoc cref="Interop.WM_DESTROY"/>
    Destroy = Interop.WM_DESTROY,

    /// <inheritdoc cref="Interop.WM_MOVE"/>
    Move = Interop.WM_MOVE,

    /// <inheritdoc cref="Interop.WM_SIZE"/>
    Size = Interop.WM_SIZE,

    /// <inheritdoc cref="Interop.WM_ACTIVATE"/>
    Activate = Interop.WM_ACTIVATE,

    /// <inheritdoc cref="Interop.WM_SETFOCUS"/>
    SetFocus = Interop.WM_SETFOCUS,

    /// <inheritdoc cref="Interop.WM_KILLFOCUS"/>
    KillFocus = Interop.WM_KILLFOCUS,

    /// <inheritdoc cref="Interop.WM_ENABLE"/>
    Enable = Interop.WM_ENABLE,

    /// <inheritdoc cref="Interop.WM_SETREDRAW"/>
    SetRedraw = Interop.WM_SETREDRAW,

    /// <inheritdoc cref="Interop.WM_SETTEXT"/>
    SetText = Interop.WM_SETTEXT,

    /// <inheritdoc cref="Interop.WM_GETTEXT"/>
    GetText = Interop.WM_GETTEXT,

    /// <inheritdoc cref="Interop.WM_GETTEXTLENGTH"/>
    GetTextLength = Interop.WM_GETTEXTLENGTH,

    /// <inheritdoc cref="Interop.WM_PAINT"/>
    Paint = Interop.WM_PAINT,

    /// <inheritdoc cref="Interop.WM_CLOSE"/>
    Close = Interop.WM_CLOSE,

    /// <inheritdoc cref="Interop.WM_QUIT"/>
    Quit = Interop.WM_QUIT,

    /// <inheritdoc cref="Interop.WM_ERASEBKGND"/>
    EraseBackground = Interop.WM_ERASEBKGND,

    /// <inheritdoc cref="Interop.WM_SYSCOLORCHANGE"/>
    SystemColorChange = Interop.WM_SYSCOLORCHANGE,

    /// <inheritdoc cref="Interop.WM_SHOWWINDOW"/>
    ShowWindow = Interop.WM_SHOWWINDOW,

    /// <inheritdoc cref="Interop.WM_WININICHANGE"/>
    SettingChange = Interop.WM_SETTINGCHANGE,

    /// <inheritdoc cref="Interop.WM_DEVMODECHANGE"/>
    DevModeChange = Interop.WM_DEVMODECHANGE,

    /// <inheritdoc cref="Interop.WM_ACTIVATEAPP"/>
    ActivateApp = Interop.WM_ACTIVATEAPP,

    /// <inheritdoc cref="Interop.WM_FONTCHANGE"/>
    FontChange = Interop.WM_FONTCHANGE,

    /// <inheritdoc cref="Interop.WM_TIMECHANGE"/>
    TimeChange = Interop.WM_TIMECHANGE,

    /// <inheritdoc cref="Interop.WM_CANCELMODE"/>
    CancelMode = Interop.WM_CANCELMODE,

    /// <inheritdoc cref="Interop.WM_SETCURSOR"/>
    SetCursor = Interop.WM_SETCURSOR,

    /// <inheritdoc cref="Interop.WM_MOUSEACTIVATE"/>
    MouseActivate = Interop.WM_MOUSEACTIVATE,

    /// <inheritdoc cref="Interop.WM_CHILDACTIVATE"/>
    ChildActivate = Interop.WM_CHILDACTIVATE,

    /// <inheritdoc cref="Interop.WM_QUEUESYNC"/>
    QueueSync = Interop.WM_QUEUESYNC,

    /// <inheritdoc cref="Interop.WM_GETMINMAXINFO"/>
    GetMinMaxInfo = Interop.WM_GETMINMAXINFO,

    /// <inheritdoc cref="Interop.WM_PAINTICON"/>
    PaintIcon = Interop.WM_PAINTICON,

    /// <inheritdoc cref="Interop.WM_ICONERASEBKGND"/>
    IconEraseBackground = Interop.WM_ICONERASEBKGND,

    /// <inheritdoc cref="Interop.WM_NEXTDLGCTL"/>
    NextDialogControl = Interop.WM_NEXTDLGCTL,

    /// <inheritdoc cref="Interop.WM_SPOOLERSTATUS"/>
    SpoolerStatus = Interop.WM_SPOOLERSTATUS,

    /// <inheritdoc cref="Interop.WM_DRAWITEM"/>
    DrawItem = Interop.WM_DRAWITEM,

    /// <inheritdoc cref="Interop.WM_MEASUREITEM"/>
    MeasureItem = Interop.WM_MEASUREITEM,

    /// <inheritdoc cref="Interop.WM_DELETEITEM"/>
    DeleteItem = Interop.WM_DELETEITEM,

    /// <inheritdoc cref="Interop.WM_VKEYTOITEM"/>
    VirtualKeyToItem = Interop.WM_VKEYTOITEM,

    /// <inheritdoc cref="Interop.WM_CHARTOITEM"/>
    CharToItem = Interop.WM_CHARTOITEM,

    /// <inheritdoc cref="Interop.WM_SETFONT"/>
    SetFont = Interop.WM_SETFONT,

    /// <inheritdoc cref="Interop.WM_GETFONT"/>
    GetFont = Interop.WM_GETFONT,

    /// <inheritdoc cref="Interop.WM_SETHOTKEY"/>
    SetHotKey = Interop.WM_SETHOTKEY,

    /// <inheritdoc cref="Interop.WM_GETHOTKEY"/>
    GetHotKey = Interop.WM_GETHOTKEY,

    /// <inheritdoc cref="Interop.WM_QUERYDRAGICON"/>
    QueryDragIcon = Interop.WM_QUERYDRAGICON,

    /// <inheritdoc cref="Interop.WM_COMPAREITEM"/>
    CompareItem = Interop.WM_COMPAREITEM,

    /// <inheritdoc cref="Interop.WM_GETOBJECT"/>
    GetObject = Interop.WM_GETOBJECT,

    /// <inheritdoc cref="Interop.WM_COMPACTING"/>
    Compacting = Interop.WM_COMPACTING,

    /// <inheritdoc cref="Interop.WM_COMMNOTIFY"/>
    CommNotify = Interop.WM_COMMNOTIFY,

    /// <inheritdoc cref="Interop.WM_WINDOWPOSCHANGING"/>
    WindowPositionChanging = Interop.WM_WINDOWPOSCHANGING,

    /// <inheritdoc cref="Interop.WM_WINDOWPOSCHANGED"/>
    WindowPositionChanged = Interop.WM_WINDOWPOSCHANGED,

    /// <inheritdoc cref="Interop.WM_POWER"/>
    Power = Interop.WM_POWER,

    /// <inheritdoc cref="Interop.WM_COPYDATA"/>
    CopyData = Interop.WM_COPYDATA,

    /// <inheritdoc cref="Interop.WM_CANCELJOURNAL"/>
    CancelJournal = Interop.WM_CANCELJOURNAL,

    /// <inheritdoc cref="Interop.WM_NOTIFY"/>
    Notify = Interop.WM_NOTIFY,

    /// <inheritdoc cref="Interop.WM_INPUTLANGCHANGEREQUEST"/>
    InputLanguageChangeRequest = Interop.WM_INPUTLANGCHANGEREQUEST,

    /// <inheritdoc cref="Interop.WM_INPUTLANGCHANGE"/>
    InputLanguageChange = Interop.WM_INPUTLANGCHANGE,

    /// <inheritdoc cref="Interop.WM_TCARD"/>
    TrainingCard = Interop.WM_TCARD,

    /// <inheritdoc cref="Interop.WM_HELP"/>
    Help = Interop.WM_HELP,

    /// <inheritdoc cref="Interop.WM_USERCHANGED"/>
    UserChanged = Interop.WM_USERCHANGED,

    /// <inheritdoc cref="Interop.WM_NOTIFYFORMAT"/>
    NotifyFormat = Interop.WM_NOTIFYFORMAT,

    /// <inheritdoc cref="Interop.WM_CONTEXTMENU"/>
    ContextMenu = Interop.WM_CONTEXTMENU,

    /// <inheritdoc cref="Interop.WM_STYLECHANGING"/>
    StyleChanging = Interop.WM_STYLECHANGING,

    /// <inheritdoc cref="Interop.WM_STYLECHANGED"/>
    StyleChanged = Interop.WM_STYLECHANGED,

    /// <inheritdoc cref="Interop.WM_DISPLAYCHANGE"/>
    DisplayChange = Interop.WM_DISPLAYCHANGE,

    /// <inheritdoc cref="Interop.WM_GETICON"/>
    GetIcon = Interop.WM_GETICON,

    /// <inheritdoc cref="Interop.WM_SETICON"/>
    SetIcon = Interop.WM_SETICON,

    /// <inheritdoc cref="Interop.WM_NCCREATE"/>
    NonClientCreate = Interop.WM_NCCREATE,

    /// <inheritdoc cref="Interop.WM_NCDESTROY"/>
    NonClientDestroy = Interop.WM_NCDESTROY,

    /// <inheritdoc cref="Interop.WM_NCCALCSIZE"/>
    NonClientCalculateSize = Interop.WM_NCCALCSIZE,

    /// <inheritdoc cref="Interop.WM_NCHITTEST"/>
    NonClientHitTest = Interop.WM_NCHITTEST,

    /// <inheritdoc cref="Interop.WM_NCPAINT"/>
    NonClientPaint = Interop.WM_NCPAINT,

    /// <inheritdoc cref="Interop.WM_NCACTIVATE"/>
    NonClientActivate = Interop.WM_NCACTIVATE,

    /// <inheritdoc cref="Interop.WM_GETDLGCODE"/>
    GetDialogCode = Interop.WM_GETDLGCODE,

    /// <inheritdoc cref="Interop.WM_SYNCPAINT"/>
    SyncPaint = Interop.WM_SYNCPAINT,

    /// <inheritdoc cref="Interop.WM_NCMOUSEMOVE"/>
    NonClientMouseMove = Interop.WM_NCMOUSEMOVE,

    /// <inheritdoc cref="Interop.WM_NCLBUTTONDOWN"/>
    NonClientLeftButtonDown = Interop.WM_NCLBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_NCLBUTTONUP"/>
    NonClientLeftButtonUp = Interop.WM_NCLBUTTONUP,

    /// <inheritdoc cref="Interop.WM_NCLBUTTONDBLCLK"/>
    NonClientLeftButtonDoubleClick = Interop.WM_NCLBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_NCRBUTTONDOWN"/>
    NonClientRightButtonDown = Interop.WM_NCRBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_NCRBUTTONUP"/>
    NonClientRightButtonUp = Interop.WM_NCRBUTTONUP,

    /// <inheritdoc cref="Interop.WM_NCRBUTTONDBLCLK"/>
    NonClientRightButtonDoubleClick = Interop.WM_NCRBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_NCMBUTTONDOWN"/>
    NonClientMiddleButtonDown = Interop.WM_NCMBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_NCMBUTTONUP"/>
    NonClientMiddleButtonUp = Interop.WM_NCMBUTTONUP,

    /// <inheritdoc cref="Interop.WM_NCMBUTTONDBLCLK"/>
    NonClientMiddleButtonDoubleClick = Interop.WM_NCMBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_NCXBUTTONDOWN"/>
    NonClientExtraButtonDown = Interop.WM_NCXBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_NCXBUTTONUP"/>
    NonClientExtraButtonUp = Interop.WM_NCXBUTTONUP,

    /// <inheritdoc cref="Interop.WM_NCXBUTTONDBLCLK"/>
    NonClientExtraButtonDoubleClick = Interop.WM_NCXBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_INPUT_DEVICE_CHANGE"/>
    InputDeviceChange = Interop.WM_INPUT_DEVICE_CHANGE,

    /// <inheritdoc cref="Interop.WM_INPUT"/>
    Input = Interop.WM_INPUT,

    /// <inheritdoc cref="Interop.WM_KEYDOWN"/>
    KeyDown = Interop.WM_KEYDOWN,

    /// <inheritdoc cref="Interop.WM_KEYUP"/>
    KeyUp = Interop.WM_KEYUP,

    /// <inheritdoc cref="Interop.WM_CHAR"/>
    Char = Interop.WM_CHAR,

    /// <inheritdoc cref="Interop.WM_DEADCHAR"/>
    DeadChar = Interop.WM_DEADCHAR,

    /// <inheritdoc cref="Interop.WM_SYSKEYDOWN"/>
    SystemKeyDown = Interop.WM_SYSKEYDOWN,

    /// <inheritdoc cref="Interop.WM_SYSKEYUP"/>
    SystemKeyUp = Interop.WM_SYSKEYUP,

    /// <inheritdoc cref="Interop.WM_SYSCHAR"/>
    SystemChar = Interop.WM_SYSCHAR,

    /// <inheritdoc cref="Interop.WM_SYSDEADCHAR"/>
    SystemDeadChar = Interop.WM_SYSDEADCHAR,

    /// <inheritdoc cref="Interop.WM_UNICHAR"/>
    UnicodeChar = Interop.WM_UNICHAR,

    /// <inheritdoc cref="Interop.WM_IME_STARTCOMPOSITION"/>
    ImeStartComposition = Interop.WM_IME_STARTCOMPOSITION,

    /// <inheritdoc cref="Interop.WM_IME_ENDCOMPOSITION"/>
    ImeEndComposition = Interop.WM_IME_ENDCOMPOSITION,

    /// <inheritdoc cref="Interop.WM_IME_COMPOSITION"/>
    ImeComposition = Interop.WM_IME_COMPOSITION,

    /// <inheritdoc cref="Interop.WM_INITDIALOG"/>
    InitDialog = Interop.WM_INITDIALOG,

    /// <inheritdoc cref="Interop.WM_COMMAND"/>
    Command = Interop.WM_COMMAND,

    /// <inheritdoc cref="Interop.WM_SYSCOMMAND"/>
    SystemCommand = Interop.WM_SYSCOMMAND,

    /// <inheritdoc cref="Interop.WM_TIMER"/>
    Timer = Interop.WM_TIMER,

    /// <inheritdoc cref="Interop.WM_HSCROLL"/>
    HorizontalScroll = Interop.WM_HSCROLL,

    /// <inheritdoc cref="Interop.WM_VSCROLL"/>
    VerticalScroll = Interop.WM_VSCROLL,

    /// <inheritdoc cref="Interop.WM_INITMENU"/>
    InitMenu = Interop.WM_INITMENU,

    /// <inheritdoc cref="Interop.WM_INITMENUPOPUP"/>
    InitMenuPopUp = Interop.WM_INITMENUPOPUP,

    /// <inheritdoc cref="Interop.WM_MENUSELECT"/>
    Gesture = Interop.WM_GESTURE,

    /// <inheritdoc cref="Interop.WM_GESTURENOTIFY"/>
    GestureNotify = Interop.WM_GESTURENOTIFY,

    /// <inheritdoc cref="Interop.WM_MENUSELECT"/>
    MenuSelect = Interop.WM_MENUSELECT,

    /// <inheritdoc cref="Interop.WM_MENUCHAR"/>
    MenuChar = Interop.WM_MENUCHAR,

    /// <inheritdoc cref="Interop.WM_ENTERIDLE"/>
    EnterIdle = Interop.WM_ENTERIDLE,

    /// <inheritdoc cref="Interop.WM_MENURBUTTONUP"/>
    MenuRightButtonUp = Interop.WM_MENURBUTTONUP,

    /// <inheritdoc cref="Interop.WM_MENUDRAG"/>
    MenuDrag = Interop.WM_MENUDRAG,

    /// <inheritdoc cref="Interop.WM_MENUGETOBJECT"/>
    MenuGetObject = Interop.WM_MENUGETOBJECT,

    /// <inheritdoc cref="Interop.WM_UNINITMENUPOPUP"/>
    UninitializeMenupPopUp = Interop.WM_UNINITMENUPOPUP,

    /// <inheritdoc cref="Interop.WM_MENUCOMMAND"/>
    MenuCommand = Interop.WM_MENUCOMMAND,

    /// <inheritdoc cref="Interop.WM_CHANGEUISTATE"/>
    ChangeUIState = Interop.WM_CHANGEUISTATE,

    /// <inheritdoc cref="Interop.WM_UPDATEUISTATE"/>
    UpdateUIState = Interop.WM_UPDATEUISTATE,

    /// <inheritdoc cref="Interop.WM_QUERYUISTATE"/>
    QueryUIState = Interop.WM_QUERYUISTATE,

    /// <inheritdoc cref="Interop.WM_CTLCOLORMSGBOX"/>
    ControlColorMessageBox = Interop.WM_CTLCOLORMSGBOX,

    /// <inheritdoc cref="Interop.WM_CTLCOLOREDIT"/>
    ControlColorEdit = Interop.WM_CTLCOLOREDIT,

    /// <inheritdoc cref="Interop.WM_CTLCOLORLISTBOX"/>
    ControlColorListBox = Interop.WM_CTLCOLORLISTBOX,

    /// <inheritdoc cref="Interop.WM_CTLCOLORBTN"/>
    ControlColorButton = Interop.WM_CTLCOLORBTN,

    /// <inheritdoc cref="Interop.WM_CTLCOLORDLG"/>
    ControlColorDialog = Interop.WM_CTLCOLORDLG,

    /// <inheritdoc cref="Interop.WM_CTLCOLORSCROLLBAR"/>
    ControlColorScrollBar = Interop.WM_CTLCOLORSCROLLBAR,

    /// <inheritdoc cref="Interop.WM_CTLCOLORSTATIC"/>
    ControlColorStatic = Interop.WM_CTLCOLORSTATIC,

    /// <inheritdoc cref="Interop.WM_MOUSEMOVE"/>
    MouseMove = Interop.WM_MOUSEMOVE,

    /// <inheritdoc cref="Interop.WM_LBUTTONDOWN"/>
    LeftButtonDown = Interop.WM_LBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_LBUTTONUP"/>
    LeftButtonUp = Interop.WM_LBUTTONUP,

    /// <inheritdoc cref="Interop.WM_LBUTTONDBLCLK"/>
    LeftButtonDoubleClick = Interop.WM_LBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_RBUTTONDOWN"/>
    RightButtonDown = Interop.WM_RBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_RBUTTONUP"/>
    RightButtonUp = Interop.WM_RBUTTONUP,

    /// <inheritdoc cref="Interop.WM_RBUTTONDBLCLK"/>
    RightButtonDoubleClick = Interop.WM_RBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_MBUTTONDOWN"/>
    MiddleButtonDown = Interop.WM_MBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_MBUTTONUP"/>
    MiddleButtonUp = Interop.WM_MBUTTONUP,

    /// <inheritdoc cref="Interop.WM_MBUTTONDBLCLK"/>
    MiddleButtonDoubleClick = Interop.WM_MBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_MOUSEWHEEL"/>
    MouseWheel = Interop.WM_MOUSEWHEEL,

    /// <inheritdoc cref="Interop.WM_XBUTTONDOWN"/>
    ExtraButtonDown = Interop.WM_XBUTTONDOWN,

    /// <inheritdoc cref="Interop.WM_XBUTTONUP"/>
    ExtraButtonUp = Interop.WM_XBUTTONUP,

    /// <inheritdoc cref="Interop.WM_XBUTTONDBLCLK"/>
    ExtraButtonDoubleClick = Interop.WM_XBUTTONDBLCLK,

    /// <inheritdoc cref="Interop.WM_MOUSEHWHEEL"/>
    MouseHorizontalWheel = Interop.WM_MOUSEHWHEEL,

    /// <inheritdoc cref="Interop.WM_PARENTNOTIFY"/>
    ParentNotify = Interop.WM_PARENTNOTIFY,

    /// <inheritdoc cref="Interop.WM_ENTERMENULOOP"/>
    EnterMenuLoop = Interop.WM_ENTERMENULOOP,

    /// <inheritdoc cref="Interop.WM_EXITMENULOOP"/>
    ExitMenuLoop = Interop.WM_EXITMENULOOP,

    /// <inheritdoc cref="Interop.WM_NEXTMENU"/>
    NextMenu = Interop.WM_NEXTMENU,

    /// <inheritdoc cref="Interop.WM_SIZING"/>
    Sizing = Interop.WM_SIZING,

    /// <inheritdoc cref="Interop.WM_CAPTURECHANGED"/>
    CaptureChanged = Interop.WM_CAPTURECHANGED,

    /// <inheritdoc cref="Interop.WM_MOVING"/>
    Moving = Interop.WM_MOVING,

    /// <inheritdoc cref="Interop.WM_POWERBROADCAST"/>
    PowerBroadcast = Interop.WM_POWERBROADCAST,

    /// <inheritdoc cref="Interop.WM_DEVICECHANGE"/>
    DeviceChange = Interop.WM_DEVICECHANGE,

    /// <inheritdoc cref="Interop.WM_MDICREATE"/>
    MdiCreate = Interop.WM_MDICREATE,

    /// <inheritdoc cref="Interop.WM_MDIDESTROY"/>
    MdiDestroy = Interop.WM_MDIDESTROY,

    /// <inheritdoc cref="Interop.WM_MDIACTIVATE"/>
    MdiActivate = Interop.WM_MDIACTIVATE,

    /// <inheritdoc cref="Interop.WM_MDIRESTORE"/>
    MdiRestore = Interop.WM_MDIRESTORE,

    /// <inheritdoc cref="Interop.WM_MDINEXT"/>
    MdiNext = Interop.WM_MDINEXT,

    /// <inheritdoc cref="Interop.WM_MDIMAXIMIZE"/>
    MdiMaximize = Interop.WM_MDIMAXIMIZE,

    /// <inheritdoc cref="Interop.WM_MDITILE"/>
    MdiTile = Interop.WM_MDITILE,

    /// <inheritdoc cref="Interop.WM_MDICASCADE"/>
    MdiCascade = Interop.WM_MDICASCADE,

    /// <inheritdoc cref="Interop.WM_MDIICONARRANGE"/>
    MdiIconArrange = Interop.WM_MDIICONARRANGE,

    /// <inheritdoc cref="Interop.WM_MDIGETACTIVE"/>
    MdiGetActive = Interop.WM_MDIGETACTIVE,

    /// <inheritdoc cref="Interop.WM_MDISETMENU"/>
    MdiSetMenu = Interop.WM_MDISETMENU,

    /// <inheritdoc cref="Interop.WM_ENTERSIZEMOVE"/>
    EnterSizeMove = Interop.WM_ENTERSIZEMOVE,

    /// <inheritdoc cref="Interop.WM_EXITSIZEMOVE"/>
    ExitSizeMove = Interop.WM_EXITSIZEMOVE,

    /// <inheritdoc cref="Interop.WM_DROPFILES"/>
    DropFiles = Interop.WM_DROPFILES,

    /// <inheritdoc cref="Interop.WM_MDIREFRESHMENU"/>
    MdiRefreshMenu = Interop.WM_MDIREFRESHMENU,

    /// <inheritdoc cref="Interop.WM_POINTERDEVICECHANGE"/>
    PointerDeviceChange = Interop.WM_POINTERDEVICECHANGE,

    /// <inheritdoc cref="Interop.WM_POINTERDEVICEINRANGE"/>
    PointDeviceInRange = Interop.WM_POINTERDEVICEINRANGE,

    /// <inheritdoc cref="Interop.WM_POINTERDEVICEOUTOFRANGE"/>
    PointerDeviceOutOfRange = Interop.WM_POINTERDEVICEOUTOFRANGE,

    /// <inheritdoc cref="Interop.WM_TOUCH"/>
    Touch = Interop.WM_TOUCH,

    /// <inheritdoc cref="Interop.WM_NCPOINTERUPDATE"/>
    NonClientPointerUpdate = Interop.WM_NCPOINTERUPDATE,

    /// <inheritdoc cref="Interop.WM_NCPOINTERDOWN"/>
    NonClientPointerDown = Interop.WM_NCPOINTERDOWN,

    /// <inheritdoc cref="Interop.WM_NCPOINTERUP"/>
    NonClientPointerUp = Interop.WM_NCPOINTERUP,

    /// <inheritdoc cref="Interop.WM_POINTERUPDATE"/>
    PointerUpdate = Interop.WM_POINTERUPDATE,

    /// <inheritdoc cref="Interop.WM_POINTERDOWN"/>
    PointerDown = Interop.WM_POINTERDOWN,

    /// <inheritdoc cref="Interop.WM_POINTERUP"/>
    PointerUp = Interop.WM_POINTERUP,

    /// <inheritdoc cref="Interop.WM_POINTERENTER"/>
    PointerEnter = Interop.WM_POINTERENTER,

    /// <inheritdoc cref="Interop.WM_POINTERLEAVE"/>
    PointerLeave = Interop.WM_POINTERLEAVE,

    /// <inheritdoc cref="Interop.WM_POINTERACTIVATE"/>
    PointerActivate = Interop.WM_POINTERACTIVATE,

    /// <inheritdoc cref="Interop.WM_POINTERCAPTURECHANGED"/>
    PointerCaptureChanged = Interop.WM_POINTERCAPTURECHANGED,

    /// <inheritdoc cref="Interop.WM_TOUCHHITTESTING"/>
    TouchHitTesting = Interop.WM_TOUCHHITTESTING,

    /// <inheritdoc cref="Interop.WM_POINTERWHEEL"/>
    PointerWheel = Interop.WM_POINTERWHEEL,

    /// <inheritdoc cref="Interop.WM_POINTERHWHEEL"/>
    PointerHorizontalWheel = Interop.WM_POINTERHWHEEL,

    /// <inheritdoc cref="Interop.WM_POINTERROUTEDTO"/>
    PointerRoutedTo = Interop.WM_POINTERROUTEDTO,

    /// <inheritdoc cref="Interop.WM_POINTERROUTEDAWAY"/>
    PointerRoutedAway = Interop.WM_POINTERROUTEDAWAY,

    /// <inheritdoc cref="Interop.WM_POINTERROUTEDRELEASED"/>
    PointerRoutedReleased = Interop.WM_POINTERROUTEDRELEASED,

    /// <inheritdoc cref="Interop.WM_IME_SETCONTEXT"/>
    ImeSetContext = Interop.WM_IME_SETCONTEXT,

    /// <inheritdoc cref="Interop.WM_IME_NOTIFY"/>
    ImeNotify = Interop.WM_IME_NOTIFY,

    /// <inheritdoc cref="Interop.WM_IME_CONTROL"/>
    ImeControl = Interop.WM_IME_CONTROL,

    /// <inheritdoc cref="Interop.WM_IME_COMPOSITIONFULL"/>
    ImeCompositionFull = Interop.WM_IME_COMPOSITIONFULL,

    /// <inheritdoc cref="Interop.WM_IME_SELECT"/>
    ImeSelect = Interop.WM_IME_SELECT,

    /// <inheritdoc cref="Interop.WM_IME_CHAR"/>
    ImeChar = Interop.WM_IME_CHAR,

    /// <inheritdoc cref="Interop.WM_IME_REQUEST"/>
    ImeRequest = Interop.WM_IME_REQUEST,

    /// <inheritdoc cref="Interop.WM_IME_KEYDOWN"/>
    ImeKeyDown = Interop.WM_IME_KEYDOWN,

    /// <inheritdoc cref="Interop.WM_IME_KEYUP"/>
    ImeKeyUp = Interop.WM_IME_KEYUP,

    /// <inheritdoc cref="Interop.WM_MOUSEHOVER"/>
    MouseHover = Interop.WM_MOUSEHOVER,

    /// <inheritdoc cref="Interop.WM_MOUSELEAVE"/>
    MouseLeave = Interop.WM_MOUSELEAVE,

    /// <inheritdoc cref="Interop.WM_NCMOUSEHOVER"/>
    NonClientMouseHover = Interop.WM_NCMOUSEHOVER,

    /// <inheritdoc cref="Interop.WM_NCMOUSELEAVE"/>
    NonClientMouseLeave = Interop.WM_NCMOUSELEAVE,

    /// <inheritdoc cref="Interop.WM_WTSSESSION_CHANGE"/>
    WtsSessionChange = Interop.WM_WTSSESSION_CHANGE,

    /// <inheritdoc cref="Interop.WM_DPICHANGED"/>
    DpiChanged = Interop.WM_DPICHANGED,

    /// <inheritdoc cref="Interop.WM_DPICHANGED_BEFOREPARENT"/>
    DpiChangedBeforeParent = Interop.WM_DPICHANGED_BEFOREPARENT,

    /// <inheritdoc cref="Interop.WM_DPICHANGED_AFTERPARENT"/>
    DpiChangedAfterParent = Interop.WM_DPICHANGED_AFTERPARENT,

    /// <inheritdoc cref="Interop.WM_GETDPISCALEDSIZE"/>
    GetDpiScaledSize = Interop.WM_GETDPISCALEDSIZE,

    /// <inheritdoc cref="Interop.WM_CUT"/>
    Cut = Interop.WM_CUT,

    /// <inheritdoc cref="Interop.WM_COPY"/>
    Copy = Interop.WM_COPY,

    /// <inheritdoc cref="Interop.WM_PASTE"/>
    Paste = Interop.WM_PASTE,

    /// <inheritdoc cref="Interop.WM_CLEAR"/>
    Clear = Interop.WM_CLEAR,

    /// <inheritdoc cref="Interop.WM_UNDO"/>
    Undo = Interop.WM_UNDO,

    /// <inheritdoc cref="Interop.WM_RENDERFORMAT"/>
    RenderFormat = Interop.WM_RENDERFORMAT,

    /// <inheritdoc cref="Interop.WM_RENDERALLFORMATS"/>
    RenderAllFormats = Interop.WM_RENDERALLFORMATS,

    /// <inheritdoc cref="Interop.WM_DESTROYCLIPBOARD"/>
    DestroyClipboard = Interop.WM_DESTROYCLIPBOARD,

    /// <inheritdoc cref="Interop.WM_DRAWCLIPBOARD"/>
    DrawClipboard = Interop.WM_DRAWCLIPBOARD,

    /// <inheritdoc cref="Interop.WM_PAINTCLIPBOARD"/>
    PaintClipboard = Interop.WM_PAINTCLIPBOARD,

    /// <inheritdoc cref="Interop.WM_VSCROLLCLIPBOARD"/>
    VerticalScrollClipboard = Interop.WM_VSCROLLCLIPBOARD,

    /// <inheritdoc cref="Interop.WM_SIZECLIPBOARD"/>
    SizeClipboard = Interop.WM_SIZECLIPBOARD,

    /// <inheritdoc cref="Interop.WM_ASKCBFORMATNAME"/>
    AskClipboardFormatName = Interop.WM_ASKCBFORMATNAME,

    /// <inheritdoc cref="Interop.WM_CHANGECBCHAIN"/>
    ChangeClipboardChain = Interop.WM_CHANGECBCHAIN,

    /// <inheritdoc cref="Interop.WM_HSCROLLCLIPBOARD"/>
    HorizontalScrollClipboard = Interop.WM_HSCROLLCLIPBOARD,

    /// <inheritdoc cref="Interop.WM_QUERYNEWPALETTE"/>
    QueryNewPalette = Interop.WM_QUERYNEWPALETTE,

    /// <inheritdoc cref="Interop.WM_PALETTEISCHANGING"/>
    PaletteIsChanging = Interop.WM_PALETTEISCHANGING,

    /// <inheritdoc cref="Interop.WM_PALETTECHANGED"/>
    PaletteChanged = Interop.WM_PALETTECHANGED,

    /// <inheritdoc cref="Interop.WM_HOTKEY"/>
    HotKey = Interop.WM_HOTKEY,

    /// <inheritdoc cref="Interop.WM_PRINT"/>
    Print = Interop.WM_PRINT,

    /// <inheritdoc cref="Interop.WM_PRINTCLIENT"/>
    PrintClient = Interop.WM_PRINTCLIENT,

    /// <inheritdoc cref="Interop.WM_APPCOMMAND"/>
    AppCommand = Interop.WM_APPCOMMAND,

    /// <inheritdoc cref="Interop.WM_THEMECHANGED"/>
    ThemeChanged = Interop.WM_THEMECHANGED,

    /// <inheritdoc cref="Interop.WM_CLIPBOARDUPDATE"/>
    ClipboardUpdate = Interop.WM_CLIPBOARDUPDATE,

    /// <inheritdoc cref="Interop.WM_DWMCOMPOSITIONCHANGED"/>
    DwmCompositionChanged = Interop.WM_DWMCOMPOSITIONCHANGED,

    /// <inheritdoc cref="Interop.WM_DWMNCRENDERINGCHANGED"/>
    DwmNonClientRenderingChanged = Interop.WM_DWMNCRENDERINGCHANGED,

    /// <inheritdoc cref="Interop.WM_DWMCOLORIZATIONCOLORCHANGED"/>
    DwmColorizationColorChanged = Interop.WM_DWMCOLORIZATIONCOLORCHANGED,

    /// <inheritdoc cref="Interop.WM_DWMWINDOWMAXIMIZEDCHANGE"/>
    DwmWindowMaximizedChange = Interop.WM_DWMWINDOWMAXIMIZEDCHANGE,

    /// <inheritdoc cref="Interop.WM_DWMSENDICONICTHUMBNAIL"/>
    DwmSendIconicThumbnail = Interop.WM_DWMSENDICONICTHUMBNAIL,

    /// <inheritdoc cref="Interop.WM_DWMSENDICONICLIVEPREVIEWBITMAP"/>
    DwmSendIconicLivePreviewBitmap = Interop.WM_DWMSENDICONICLIVEPREVIEWBITMAP,

    /// <inheritdoc cref="Interop.WM_GETTITLEBARINFOEX"/>
    GetTitleBarInfo = Interop.WM_GETTITLEBARINFOEX,

    /// <summary>
    ///  Used as a base value for reflecting messages sent from Controls to parent Windows back to the Control.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://web.archive.org/web/20200426010008/http://www.tech-archive.net/Archive/VC/microsoft.public.vc.language/2005-08/msg00589.html">
    ///    What is the point of using WM_REFLECT?
    ///   </see>
    ///  </para>
    /// </remarks>
    Reflect = Interop.WM_USER + 0x1C00,

    /// <inheritdoc cref="Interop.WM_COMMAND"/>
    ReflectCommand = Reflect | Interop.WM_COMMAND
}