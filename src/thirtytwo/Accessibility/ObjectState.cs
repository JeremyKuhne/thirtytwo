// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Accessibility;

/// <summary>
///  Specifies Microsoft Active Accessibility (MSAA) state flags for an accessible object.
/// </summary>
/// <remarks>
///  <para>
///   See <see href="https://learn.microsoft.com/windows/win32/winauto/object-state-constants">Object State Constants</see>
///   documentation.
///  </para>
/// </remarks>
[Flags]
public enum ObjectState : int
{
    /// <summary>
    ///  The object is unavailable for interaction.
    /// </summary>
    Unavailable = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_UNAVAILABLE,

    /// <summary>
    ///  The object is selected.
    /// </summary>
    Selected = (int)Interop.STATE_SYSTEM_SELECTED,

    /// <summary>
    ///  The object currently has keyboard focus.
    /// </summary>
    Focused = (int)Interop.STATE_SYSTEM_FOCUSED,

    /// <summary>
    ///  The object is pressed.
    /// </summary>
    Pressed = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_PRESSED,

    /// <summary>
    ///  The object is checked.
    /// </summary>
    Checked = (int)Interop.STATE_SYSTEM_CHECKED,

    /// <summary>
    ///  The object is in an indeterminate or mixed state.
    /// </summary>
    Mixed = (int)Interop.STATE_SYSTEM_MIXED,

    /// <summary>
    ///  The object value cannot be edited.
    /// </summary>
    ReadOnly = (int)Interop.STATE_SYSTEM_READONLY,

    /// <summary>
    ///  The pointer is hovering over the object.
    /// </summary>
    HotTracked = (int)Interop.STATE_SYSTEM_HOTTRACKED,

    /// <summary>
    ///  The object is the default command target.
    /// </summary>
    Default = (int)Interop.STATE_SYSTEM_DEFAULT,

    /// <summary>
    ///  The object is expanded.
    /// </summary>
    Expanded = (int)Interop.STATE_SYSTEM_EXPANDED,

    /// <summary>
    ///  The object is collapsed.
    /// </summary>
    Collapsed = (int)Interop.STATE_SYSTEM_COLLAPSED,

    /// <summary>
    ///  The object is busy.
    /// </summary>
    Busy = (int)Interop.STATE_SYSTEM_BUSY,

    /// <summary>
    ///  The object is floating and not docked in normal flow.
    /// </summary>
    Floating = (int)Interop.STATE_SYSTEM_FLOATING,

    /// <summary>
    ///  The object is marqueed or continuously scrolling.
    /// </summary>
    Marqueed = (int)Interop.STATE_SYSTEM_MARQUEED,

    /// <summary>
    ///  The object is animated.
    /// </summary>
    Animated = (int)Interop.STATE_SYSTEM_ANIMATED,

    /// <summary>
    ///  The object is not visible.
    /// </summary>
    Invisible = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_INVISIBLE,

    /// <summary>
    ///  The object is outside the visible viewport.
    /// </summary>
    OffScreen = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_OFFSCREEN,

    /// <summary>
    ///  The object can be resized.
    /// </summary>
    Sizeable = (int)Interop.STATE_SYSTEM_SIZEABLE,

    /// <summary>
    ///  The object can be moved.
    /// </summary>
    Moveable = (int)Interop.STATE_SYSTEM_MOVEABLE,

    /// <summary>
    ///  The object provides its own spoken feedback.
    /// </summary>
    SelfVoicing = (int)Interop.STATE_SYSTEM_SELFVOICING,

    /// <summary>
    ///  The object can receive keyboard focus.
    /// </summary>
    Focusable = (int)COMBOBOXINFO_BUTTON_STATE.STATE_SYSTEM_FOCUSABLE,

    /// <summary>
    ///  The object can be selected.
    /// </summary>
    Selectable = (int)Interop.STATE_SYSTEM_SELECTABLE,

    /// <summary>
    ///  The object is linked to another item.
    /// </summary>
    Linked = (int)Interop.STATE_SYSTEM_LINKED,

    /// <summary>
    ///  A link object has been visited.
    /// </summary>
    Traversed = (int)Interop.STATE_SYSTEM_TRAVERSED,

    /// <summary>
    ///  The container allows multiple selection.
    /// </summary>
    Multiselectable = (int)Interop.STATE_SYSTEM_MULTISELECTABLE,

    /// <summary>
    ///  The container supports extended selection gestures.
    /// </summary>
    ExtendedSelectable = (int)Interop.STATE_SYSTEM_EXTSELECTABLE,

    /// <summary>
    ///  Low-priority alert state.
    /// </summary>
    AlertLow = (int)Interop.STATE_SYSTEM_ALERT_LOW,

    /// <summary>
    ///  Medium-priority alert state.
    /// </summary>
    AlertMedium = (int)Interop.STATE_SYSTEM_ALERT_MEDIUM,

    /// <summary>
    ///  High-priority alert state.
    /// </summary>
    AlertHigh = (int)Interop.STATE_SYSTEM_ALERT_HIGH,

    /// <summary>
    ///  Content is protected and should not be exposed directly.
    /// </summary>
    Protected = (int)Interop.STATE_SYSTEM_PROTECTED,

    /// <summary>
    ///  The object has an associated pop-up.
    /// </summary>
    HasPopup = (int)Interop.STATE_SYSTEM_HASPOPUP
}