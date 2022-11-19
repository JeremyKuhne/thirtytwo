// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.SystemServices;

namespace Windows;

/// <summary>
///  Mouse key states for mouse messages.
/// </summary>
[Flags]
public enum MouseKey : uint
{
    /// <summary>
    ///  Left mouse button is down.
    /// </summary>
    LeftButton = MODIFIERKEYS_FLAGS.MK_LBUTTON,

    /// <summary>
    ///  Right mouse button is down.
    /// </summary>
    RightButton = MODIFIERKEYS_FLAGS.MK_RBUTTON,

    /// <summary>
    ///  Shift key is down.
    /// </summary>
    Shift = MODIFIERKEYS_FLAGS.MK_SHIFT,

    /// <summary>
    ///  Control key is down.
    /// </summary>
    Control = MODIFIERKEYS_FLAGS.MK_CONTROL,

    /// <summary>
    ///  Middle mouse button is down.
    /// </summary>
    MiddleButton = MODIFIERKEYS_FLAGS.MK_MBUTTON,

    /// <summary>
    ///  First extra mouse button is down.
    /// </summary>
    ExtraButton1 = MODIFIERKEYS_FLAGS.MK_XBUTTON1,

    /// <summary>
    ///  Second extra mouse button is down.
    /// </summary>
    ExtraButton2 = MODIFIERKEYS_FLAGS.MK_XBUTTON2
}