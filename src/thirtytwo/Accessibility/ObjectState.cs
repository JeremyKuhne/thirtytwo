// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/object-state-constants">Object State Constants</see>
///   documentation.
///  </para>
/// </remarks>
[Flags]
public enum ObjectState : int
{
    Unavailable = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_UNAVAILABLE,
    Selected = (int)Interop.STATE_SYSTEM_SELECTED,
    Focused = (int)Interop.STATE_SYSTEM_FOCUSED,
    Pressed = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_PRESSED,
    Checked = (int)Interop.STATE_SYSTEM_CHECKED,
    Mixed = (int)Interop.STATE_SYSTEM_MIXED,
    ReadOnly = (int)Interop.STATE_SYSTEM_READONLY,
    HotTracked = (int)Interop.STATE_SYSTEM_HOTTRACKED,
    Default = (int)Interop.STATE_SYSTEM_DEFAULT,
    Expanded = (int)Interop.STATE_SYSTEM_EXPANDED,
    Collapsed = (int)Interop.STATE_SYSTEM_COLLAPSED,
    Busy = (int)Interop.STATE_SYSTEM_BUSY,
    Floating = (int)Interop.STATE_SYSTEM_FLOATING,
    Marqueed = (int)Interop.STATE_SYSTEM_MARQUEED,
    Animated = (int)Interop.STATE_SYSTEM_ANIMATED,
    Invisible = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_INVISIBLE,
    OffScreen = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_OFFSCREEN,
    Sizeable = (int)Interop.STATE_SYSTEM_SIZEABLE,
    Moveable = (int)Interop.STATE_SYSTEM_MOVEABLE,
    SelfVoicing = (int)Interop.STATE_SYSTEM_SELFVOICING,
    Focusable = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_FOCUSABLE,
    Selectable = (int)Interop.STATE_SYSTEM_SELECTABLE,
    Linked = (int)Interop.STATE_SYSTEM_LINKED,
    Traversed = (int)Interop.STATE_SYSTEM_TRAVERSED,
    Multiselectable = (int)Interop.STATE_SYSTEM_MULTISELECTABLE,
    ExtendedSelectable = (int)Interop.STATE_SYSTEM_EXTSELECTABLE,
    AlertLow = (int)Interop.STATE_SYSTEM_ALERT_LOW,
    AlertMedium = (int)Interop.STATE_SYSTEM_ALERT_MEDIUM,
    AlertHigh = (int)Interop.STATE_SYSTEM_ALERT_HIGH,
    Protected = (int)Interop.STATE_SYSTEM_PROTECTED,
    HasPopup = (int)Interop.STATE_SYSTEM_HASPOPUP
}