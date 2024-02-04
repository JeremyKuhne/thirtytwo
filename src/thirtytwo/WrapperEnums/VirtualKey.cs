// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Windows;

// https://learn.microsoft.com/windows/win32/inputdev/virtual-key-codes
public enum VirtualKey : ushort
{
    /// <summary>
    ///  (VK_LBUTTON)
    /// </summary>
    LeftButton = VK_LBUTTON,

    /// <summary>
    ///  (VK_RBUTTON)
    /// </summary>
    RightButton = VK_RBUTTON,

    /// <summary>
    ///  (VK_CANCEL)
    /// </summary>
    Cancel = VK_CANCEL,

    /// <summary>
    ///  (VK_MBUTTON)
    /// </summary>
    MiddleButton = VK_MBUTTON,

    /// <summary>
    ///  (VK_XBUTTON1)
    /// </summary>
    ExtraButton1 = VK_XBUTTON1,

    /// <summary>
    ///  (VK_XBUTTON2)
    /// </summary>
    ExtraButton2 = VK_XBUTTON2,

    /// <summary>
    ///  Reserved. Xbox button. (VK_NEXUS)
    /// </summary>
    Nexus = 0x07,

    /// <summary>
    ///  (VK_BACK)
    /// </summary>
    Back = VK_BACK,

    /// <summary>
    ///  (VK_TAB)
    /// </summary>
    Tab = VK_TAB,

    /// <summary>
    ///  (VK_CLEAR)
    /// </summary>
    Clear = VK_CLEAR,

    /// <summary>
    ///  (VK_RETURN)
    /// </summary>
    Return = VK_RETURN,

    /// <summary>
    ///  (VK_SHIFT)
    /// </summary>
    Shift = VK_SHIFT,

    /// <summary>
    ///  (VK_CONTROL)
    /// </summary>
    Control = VK_CONTROL,

    /// <summary>
    ///  (VK_MENU)
    /// </summary>
    Menu = VK_MENU,

    /// <summary>
    ///  (VK_PAUSE)
    /// </summary>
    Pause = VK_PAUSE,

    /// <summary>
    ///  (VK_CAPITAL)
    /// </summary>
    Capital = VK_CAPITAL,

    /// <summary>
    ///  (VK_KANA)
    /// </summary>
    Kana = VK_KANA,

    /// <summary>
    ///  (VK_HANGUL)
    /// </summary>
    Hangul = VK_HANGUL,

    /// <summary>
    ///  (VK_JUNJA)
    /// </summary>
    Junja = VK_JUNJA,

    /// <summary>
    ///  (VK_FINAL)
    /// </summary>
    Final = VK_FINAL,

    /// <summary>
    ///  (VK_HANJA)
    /// </summary>
    Hanja = VK_HANJA,

    /// <summary>
    ///  (VK_KANJI)
    /// </summary>
    Kanji = VK_KANJI,

    /// <summary>
    ///  (VK_ESCAPE)
    /// </summary>
    Escape = VK_ESCAPE,

    /// <summary>
    ///  (VK_CONVERT)
    /// </summary>
    Convert = VK_CONVERT,

    /// <summary>
    ///  (VK_NONCONVERT)
    /// </summary>
    NonConvert = VK_NONCONVERT,

    /// <summary>
    ///  (VK_ACCEPT)
    /// </summary>
    Accept = VK_ACCEPT,

    /// <summary>
    ///  (VK_MODECHANGE)
    /// </summary>
    ModeChange = VK_MODECHANGE,

    /// <summary>
    ///  (VK_SPACE)
    /// </summary>
    Space = VK_SPACE,

    /// <summary>
    ///  Page up. (VK_PRIOR)
    /// </summary>
    Prior = VK_PRIOR,

    /// <summary>
    ///  Page down. (VK_NEXT)
    /// </summary>
    Next = VK_NEXT,

    /// <summary>
    ///  (VK_END)
    /// </summary>
    End = VK_END,

    /// <summary>
    ///  (VK_HOME)
    /// </summary>
    Home = VK_HOME,

    /// <summary>
    ///  (VK_LEFT)
    /// </summary>
    Left = VK_LEFT,

    /// <summary>
    ///  (VK_UP)
    /// </summary>
    Up = VK_UP,

    /// <summary>
    ///  (VK_RIGHT)
    /// </summary>
    Right = VK_RIGHT,

    /// <summary>
    ///  (VK_DOWN)
    /// </summary>
    Down = VK_DOWN,

    /// <summary>
    ///  (VK_SELECT)
    /// </summary>
    Select = VK_SELECT,

    /// <summary>
    ///  (VK_PRINT)
    /// </summary>
    Print = VK_PRINT,

    /// <summary>
    ///  (VK_EXECUTE)
    /// </summary>
    Execute = VK_EXECUTE,

    /// <summary>
    ///  Print screen. (VK_SNAPSHOT)
    /// </summary>
    Snapshot = VK_SNAPSHOT,

    /// <summary>
    ///  (VK_INSERT)
    /// </summary>
    Insert = VK_INSERT,

    /// <summary>
    ///  (VK_DELETE)
    /// </summary>
    Delete = VK_DELETE,

    /// <summary>
    ///  (VK_HELP)
    /// </summary>
    Help = VK_HELP,

    // 0-9 and A-Z match ASCII

    One = 0x30,
    Two = 0x31,
    Three = 0x32,
    Four = 0x33,
    Five = 0x34,
    Six = 0x35,
    Seven = 0x36,
    Eight = 0x37,
    Nine = 0x38,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,

    /// <summary>
    ///  (VK_LWIN)
    /// </summary>
    LeftWindows = VK_LWIN,

    /// <summary>
    ///  (VK_RWIN)
    /// </summary>
    RightWindows = VK_RWIN,

    /// <summary>
    ///  (VK_APPS)
    /// </summary>
    Apps = VK_APPS,

    /// <summary>
    ///  (VK_SLEEP)
    /// </summary>
    Sleep = VK_SLEEP,

    /// <summary>
    ///  (VK_NUMPAD0)
    /// </summary>
    NumPad0 = VK_NUMPAD0,

    /// <summary>
    ///  (VK_NUMPAD1)
    /// </summary>
    NumPad1 = VK_NUMPAD1,

    /// <summary>
    ///  (VK_NUMPAD2)
    /// </summary>
    NumPad2 = VK_NUMPAD2,

    /// <summary>
    ///  (VK_NUMPAD3)
    /// </summary>
    NumPad3 = VK_NUMPAD3,

    /// <summary>
    ///  (VK_NUMPAD4)
    /// </summary>
    NumPad4 = VK_NUMPAD4,

    /// <summary>
    ///  (VK_NUMPAD5)
    /// </summary>
    NumPad5 = VK_NUMPAD5,

    /// <summary>
    ///  (VK_NUMPAD6)
    /// </summary>
    NumPad6 = VK_NUMPAD6,

    /// <summary>
    ///  (VK_NUMPAD7)
    /// </summary>
    NumPad7 = VK_NUMPAD7,

    /// <summary>
    ///  (VK_NUMPAD8)
    /// </summary>
    NumPad8 = VK_NUMPAD8,

    /// <summary>
    ///  (VK_NUMPAD9)
    /// </summary>
    NumPad9 = VK_NUMPAD9,

    /// <summary>
    ///  (VK_MULTIPLY)
    /// </summary>
    Multiply = VK_MULTIPLY,

    /// <summary>
    ///  (VK_ADD)
    /// </summary>
    Add = VK_ADD,

    /// <summary>
    ///  (VK_SEPARATOR)
    /// </summary>
    Separator = VK_SEPARATOR,

    /// <summary>
    ///  (VK_SUBTRACT)
    /// </summary>
    Subtract = VK_SUBTRACT,

    /// <summary>
    ///  (VK_DECIMAL)
    /// </summary>
    Decimal = VK_DECIMAL,

    /// <summary>
    ///  (VK_DIVIDE)
    /// </summary>
    Divide = VK_DIVIDE,

    /// <summary>
    ///  (VK_F1)
    /// </summary>
    F1 = VK_F1,

    /// <summary>
    ///  (VK_F2)
    /// </summary>
    F2 = VK_F2,

    /// <summary>
    ///  (VK_F3)
    /// </summary>
    F3 = VK_F3,

    /// <summary>
    ///  (VK_F4)
    /// </summary>
    F4 = VK_F4,

    /// <summary>
    ///  (VK_F5)
    /// </summary>
    F5 = VK_F5,

    /// <summary>
    ///  (VK_F6)
    /// </summary>
    F6 = VK_F6,

    /// <summary>
    ///  (VK_F7)
    /// </summary>
    F7 = VK_F7,

    /// <summary>
    ///  (VK_F8)
    /// </summary>
    F8 = VK_F8,

    /// <summary>
    ///  (VK_F9)
    /// </summary>
    F9 = VK_F9,

    /// <summary>
    ///  (VK_F10)
    /// </summary>
    F10 = VK_F10,

    /// <summary>
    ///  (VK_F11)
    /// </summary>
    F11 = VK_F11,

    /// <summary>
    ///  (VK_F12)
    /// </summary>
    F12 = VK_F12,

    /// <summary>
    ///  (VK_F13)
    /// </summary>
    F13 = VK_F13,

    /// <summary>
    ///  (VK_F14)
    /// </summary>
    F14 = VK_F14,

    /// <summary>
    ///  (VK_F15)
    /// </summary>
    F15 = VK_F15,

    /// <summary>
    ///  (VK_F16)
    /// </summary>
    F16 = VK_F16,

    /// <summary>
    ///  (VK_F17)
    /// </summary>
    F17 = VK_F17,

    /// <summary>
    ///  (VK_F18)
    /// </summary>
    F18 = VK_F18,

    /// <summary>
    ///  (VK_F19)
    /// </summary>
    F19 = VK_F19,

    /// <summary>
    ///  (VK_F20)
    /// </summary>
    F20 = VK_F20,

    /// <summary>
    ///  (VK_F21)
    /// </summary>
    F21 = VK_F21,

    /// <summary>
    ///  (VK_F22)
    /// </summary>
    F22 = VK_F22,

    /// <summary>
    ///  (VK_F23)
    /// </summary>
    F23 = VK_F23,

    /// <summary>
    ///  (VK_F24)
    /// </summary>
    F24 = VK_F24,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_VIEW)
    /// </summary>
    NavigationView = 0x88,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_MENU)
    /// </summary>
    NavigationMenu = 0x89,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_UP)
    /// </summary>
    NavigationUp = 0x8A,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_DOWN)
    /// </summary>
    NavigationDown = 0x8B,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_LEFT)
    /// </summary>
    NavigationLeft = 0x8C,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_RIGHT)
    /// </summary>
    NavigationRight = 0x8D,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_ACCEPT)
    /// </summary>
    NavigationAccept = 0x8E,

    /// <summary>
    ///  Reserved. (VK_NAVIGATION_CANCEL)
    /// </summary>
    NavigationCancel = 0x8F,

    /// <summary>
    ///  (VK_NUMLOCK)
    /// </summary>
    NumLock = VK_NUMLOCK,

    /// <summary>
    ///  (VK_SCROLL)
    /// </summary>
    Scroll = VK_SCROLL,

    /// <summary>
    ///  '=' key on numpad, (VK_OEM_NEC_EQUAL)
    /// </summary>
    OemNecEqual = 0x92,

    /// <summary>
    ///  'Dictionary' key. (VK_OEM_FJ_JISHO)
    /// </summary>
    OemFujitsuJisho = 0x92,

    /// <summary>
    ///  'Unregister word' key. (VK_OEM_FJ_MASSHOU)
    /// </summary>
    OemFujitsuMasshou = 0x93,

    /// <summary>
    ///  'Register word' key. (VK_OEM_FJ_TOUROKU)
    /// </summary>
    OemFujitsuTouroku = 0x94,

    /// <summary>
    ///  'Left OYAYUBI' key. (VK_OEM_FJ_LOYA)
    /// </summary>
    OemFujitsuLoya = 0x95,

    /// <summary>
    ///  'Right OYAYUBI' key. (VK_OEM_FJ_ROYA)
    /// </summary>
    OemFujitsuRoya = 0x96,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_LSHIFT)
    /// </summary>
    LeftShift = VK_LSHIFT,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_RSHIFT)
    /// </summary>
    RightShift = VK_RSHIFT,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_LCONTROL)
    /// </summary>
    LeftControl = VK_LCONTROL,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_RCONTROL)
    /// </summary>
    RightControl = VK_RCONTROL,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_LMENU)
    /// </summary>
    LeftMenu = VK_LMENU,

    /// <summary>
    ///  Used only as parameter to GetAsyncKeyState() and GetKeyState(). (VK_RMENU)
    /// </summary>
    RightMenu = VK_RMENU,

    /// <summary>
    ///  (VK_BROWSER_BACK)
    /// </summary>
    BrowserBack = VK_BROWSER_BACK,

    /// <summary>
    ///  (VK_BROWSER_FORWARD)
    /// </summary>
    BrowserForward = VK_BROWSER_FORWARD,

    /// <summary>
    ///  (VK_BROWSER_REFRESH)
    /// </summary>
    BrowserRefresh = VK_BROWSER_REFRESH,

    /// <summary>
    ///  (VK_BROWSER_STOP)
    /// </summary>
    BrowserStop = VK_BROWSER_STOP,

    /// <summary>
    ///  (VK_BROWSER_SEARCH)
    /// </summary>
    BrowserSearch = VK_BROWSER_SEARCH,

    /// <summary>
    ///  (VK_BROWSER_FAVORITES)
    /// </summary>
    BrowserFavorites = VK_BROWSER_FAVORITES,

    /// <summary>
    ///  (VK_BROWSER_HOME)
    /// </summary>
    BrowserHome = VK_BROWSER_HOME,

    /// <summary>
    ///  (VK_VOLUME_MUTE)
    /// </summary>
    VolumeMute = VK_VOLUME_MUTE,

    /// <summary>
    ///  (VK_VOLUME_DOWN)
    /// </summary>
    VolumeDown = VK_VOLUME_DOWN,

    /// <summary>
    ///  (VK_VOLUME_UP)
    /// </summary>
    VolumeUp = VK_VOLUME_UP,

    /// <summary>
    ///  (VK_MEDIA_NEXT_TRACK)
    /// </summary>
    MediaNextTrack = VK_MEDIA_NEXT_TRACK,

    /// <summary>
    ///  (VK_MEDIA_PREV_TRACK)
    /// </summary>
    MediaPrevTrack = VK_MEDIA_PREV_TRACK,

    /// <summary>
    ///  (VK_MEDIA_STOP)
    /// </summary>
    MediaStop = VK_MEDIA_STOP,

    /// <summary>
    ///  (VK_MEDIA_PLAY_PAUSE)
    /// </summary>
    MediaPlayPause = VK_MEDIA_PLAY_PAUSE,

    /// <summary>
    ///  (VK_LAUNCH_MAIL)
    /// </summary>
    LaunchMail = VK_LAUNCH_MAIL,

    /// <summary>
    ///  (VK_LAUNCH_MEDIA_SELECT)
    /// </summary>
    LaunchMediaSelect = VK_LAUNCH_MEDIA_SELECT,

    /// <summary>
    ///  (VK_LAUNCH_APP1)
    /// </summary>
    LaunchApp1 = VK_LAUNCH_APP1,

    /// <summary>
    ///  (VK_LAUNCH_APP2)
    /// </summary>
    LaunchApp2 = VK_LAUNCH_APP2,

    /// <summary>
    ///  ';:' for US. (VK_OEM_1)
    /// </summary>
    Oem1 = VK_OEM_1,

    /// <summary>
    ///  '+' any country. (VK_OEM_PLUS)
    /// </summary>
    OemPlus = VK_OEM_PLUS,

    /// <summary>
    ///  ',' any country. (VK_OEM_COMMA)
    /// </summary>
    OemComma = VK_OEM_COMMA,

    /// <summary>
    ///  '-' any country. (VK_OEM_MINUS)
    /// </summary>
    OemMinus = VK_OEM_MINUS,

    /// <summary>
    ///  '.' any country. (VK_OEM_PERIOD)
    /// </summary>
    OemPeriod = VK_OEM_PERIOD,

    /// <summary>
    ///  '/?' for US. (VK_OEM_2)
    /// </summary>
    Oem2 = VK_OEM_2,

    /// <summary>
    ///  '`~' for US. (VK_OEM_3)
    /// </summary>
    Oem3 = VK_OEM_3,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_A)
    /// </summary>
    GamepadA = 0xC3,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_B)
    /// </summary>
    GamepadB = 0xC4,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_X)
    /// </summary>
    GamepadX = 0xC5,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_Y)
    /// </summary>
    GamepadY = 0xC6,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_SHOULDER)
    /// </summary>
    GamepadRightShoulder = 0xC7,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_SHOULDER)
    /// </summary>
    GamepadLeftShoulder = 0xC8,

    /// <summary>
    ///  Reserved. VK_GAMEPAD_LEFT_TRIGGER()
    /// </summary>
    GamepadLeftTrigger = 0xC9,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_TRIGGER)
    /// </summary>
    GamepadRightTrigger = 0xCA,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_DPAD_UP)
    /// </summary>
    GamepadDPadUp = 0xCB,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_DPAD_DOWN)
    /// </summary>
    GamepadDPadDown = 0xCC,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_DPAD_LEFT)
    /// </summary>
    GamepadDPadLeft = 0xCD,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_DPAD_RIGHT)
    /// </summary>
    GamepadDPadRight = 0xCE,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_MENU)
    /// </summary>
    GamepadMenu = 0xCF,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_VIEW)
    /// </summary>
    GamepadView = 0xD0,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON)
    /// </summary>
    GamepadLeftThumbstickButton = 0xD1,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON)
    /// </summary>
    GamepadRightThumbstickButton = 0xD2,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_THUMBSTICK_UP)
    /// </summary>
    GamepadLeftThumbstickUp = 0xD3,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_THUMBSTICK_DOWN)
    /// </summary>
    GamepadLeftThumbstickDown = 0xD4,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT)
    /// </summary>
    GamepadLeftThumbstickRight = 0xD5,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_LEFT_THUMBSTICK_LEFT)
    /// </summary>
    GamepadLeftThumbstickLeft = 0xD6,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_THUMBSTICK_UP)
    /// </summary>
    GamepadRightThumbstickUp = 0xD7,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN)
    /// </summary>
    GamepadRightThumbstickDown = 0xD8,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT)
    /// </summary>
    GamepadRightThumbstickRight = 0xD9,

    /// <summary>
    ///  Reserved. (VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT)
    /// </summary>
    GamepadRightThumbstickLeft = 0xDA,

    /// <summary>
    ///  '[{' for US. (VK_OEM_4)
    /// </summary>
    Oem4 = VK_OEM_4,

    /// <summary>
    ///  '\|' for US. (VK_OEM_5)
    /// </summary>
    Oem5 = VK_OEM_5,

    /// <summary>
    ///  ']}' for US. (VK_OEM_6)
    /// </summary>
    Oem6 = VK_OEM_6,

    /// <summary>
    ///  ''"' for US. (VK_OEM_7)
    /// </summary>
    Oem7 = VK_OEM_7,

    /// <summary>
    ///  (VK_OEM_8)
    /// </summary>
    Oem8 = VK_OEM_8,

    /// <summary>
    ///  'AX' key on Japanese AX keyboard. (VK_OEM_AX)
    /// </summary>
    OemAX = VK_OEM_AX,

    /// <summary>
    ///  "&lt;&gt;" or "\|" on RT 102-key keyboard. (VK_OEM_102)
    /// </summary>
    Oem102 = VK_OEM_102,

    /// <summary>
    ///  Help key on Olivetti M24 "ICO" (102-key) keyboard. (VK_ICO_HELP)
    /// </summary>
    IcoHelp = 0xE3,

    /// <summary>
    ///  00 key on Olivetti M24 "ICO" (102-key) keyboard. (VK_ICO_00)
    /// </summary>
    Ico00 = 0xE4,

    /// <summary>
    ///  (VK_PROCESSKEY)
    /// </summary>
    ProcessKey = VK_PROCESSKEY,

    /// <summary>
    ///  Clear key on Olivetti M24 "ICO" (102-key) keyboard. (VK_ICO_CLEAR)
    /// </summary>
    IcoClear = 0xE6,

    /// <summary>
    ///  Used to pass Unicode characters as if they were keystrokes. (VK_PACKET)
    /// </summary>
    Packet = VK_PACKET,

    // 0xE9 - 0xF5 are for Ericsson keyboards

    /// <summary>
    ///  Ericsson. (VK_OEM_RESET)
    /// </summary>
    OemReset = 0xE9,

    /// <summary>
    ///  Ericsson. (VK_OEM_JUMP)
    /// </summary>
    OemJump = 0xEA,

    /// <summary>
    ///  Ericsson. (VK_OEM_PA1)
    /// </summary>
    OemPA1 = 0xEB,

    /// <summary>
    ///  Ericsson. (VK_OEM_PA2)
    /// </summary>
    OemPA2 = 0xEC,

    /// <summary>
    ///  Ericsson. (VK_OEM_PA3)
    /// </summary>
    OemPA3 = 0xED,

    /// <summary>
    ///  Ericsson. (VK_OEM_WSCTRL)
    /// </summary>
    OemWSCtrl = 0xEE,

    /// <summary>
    ///  Ericsson. (VK_OEM_CUSEL)
    /// </summary>
    OemCuSel = 0xEF,

    /// <summary>
    ///  Ericsson. (VK_OEM_ATTN)
    /// </summary>
    OemAttn = 0xF0,

    /// <summary>
    ///  Ericsson. (VK_OEM_FINISH)
    /// </summary>
    OemFinish = 0xF1,

    /// <summary>
    ///  Ericsson. (VK_OEM_COPY)
    /// </summary>
    OemCopy = 0xF2,

    /// <summary>
    ///  Ericsson. (VK_OEM_AUTO)
    /// </summary>
    OemAuto = 0xF3,

    /// <summary>
    ///  Ericsson. (VK_OEM_ENLW)
    /// </summary>
    OemEnlw = 0xF4,

    /// <summary>
    ///  Ericsson. (VK_OEM_BACKTAB)
    /// </summary>
    OemBackTab = 0xF5,

    /// <summary>
    ///  (VK_ATTN)
    /// </summary>
    Attn = VK_ATTN,

    /// <summary>
    ///  (VK_CRSEL)
    /// </summary>
    CrSel = VK_CRSEL,

    /// <summary>
    ///  (VK_EXSEL)
    /// </summary>
    ExSel = VK_EXSEL,

    /// <summary>
    ///  (VK_EREOF)
    /// </summary>
    EraseEOF = VK_EREOF,

    /// <summary>
    ///  (VK_PLAY)
    /// </summary>
    Play = VK_PLAY,

    /// <summary>
    ///  (VK_ZOOM)
    /// </summary>
    Zoom = VK_ZOOM,

    /// <summary>
    ///  Reserved. (VK_NONAME)
    /// </summary>
    NoName = VK_NONAME,

    /// <summary>
    ///  (VK_PA1)
    /// </summary>
    PA1 = VK_PA1,

    /// <summary>
    ///  Clear key. (VK_OEM_CLEAR)
    /// </summary>
    OemClear = VK_OEM_CLEAR
}