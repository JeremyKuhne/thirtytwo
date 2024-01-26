// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public partial class ButtonControl
{
    [Flags]
    public enum Styles : uint
    {
        /// <summary>
        ///  Push button.
        /// </summary>
        PushButton = Interop.BS_PUSHBUTTON,

        /// <summary>
        ///  Default push button.
        /// </summary>
        DefaultPushButton = Interop.BS_DEFPUSHBUTTON,

        /// <summary>
        ///  Check box.
        /// </summary>
        CheckBox = Interop.BS_CHECKBOX,

        /// <summary>
        ///  Check box that automatically toggles state when clicked.
        /// </summary>
        AutoCheckBox = Interop.BS_AUTOCHECKBOX,

        /// <summary>
        ///  Radio button.
        /// </summary>
        RadioButton = Interop.BS_RADIOBUTTON,

        /// <summary>
        ///  Check box that can be disabled.
        /// </summary>
        ThreeState = Interop.BS_3STATE,

        /// <summary>
        ///  Three state checkbox that automatically changes state when clicked.
        /// </summary>
        AutoThreeState = Interop.BS_AUTO3STATE,

        /// <summary>
        ///  Group box.
        /// </summary>
        GroupBox = Interop.BS_GROUPBOX,

        /// <summary>
        ///  Obsolete. Use OwnerDraw instead.
        /// </summary>
        UserButton = Interop.BS_USERBUTTON,

        /// <summary>
        ///  Radio button that automatically changes state when clicked.
        /// </summary>
        AutoRadioButton = Interop.BS_AUTORADIOBUTTON,

        /// <summary>
        ///  A push button with just text (no frame or face).
        /// </summary>
        PushBox = Interop.BS_PUSHBOX,

        /// <summary>
        ///  Sends DrawItem (WM_DRAWITEM) window messages to draw.
        /// </summary>
        OwnerDrawn = Interop.BS_OWNERDRAW,

        /// <summary>
        ///  Button with a drop down arrow.
        /// </summary>
        SplitButton = Interop.BS_SPLITBUTTON,

        /// <summary>
        ///  Default style split button.
        /// </summary>
        DefaultSplitButton = Interop.BS_DEFSPLITBUTTON,

        /// <summary>
        ///  Button with a default green arrow and additional note text.
        ///  https://learn.microsoft.com/windows/win32/uxguide/ctrl-command-links
        /// </summary>
        CommandLink = Interop.BS_COMMANDLINK,

        /// <summary>
        ///  Default style command link button.
        /// </summary>
        DefaultCommandLink = Interop.BS_DEFCOMMANDLINK,

        // Do not use- does not include all styles.
        // BS_TYPEMASK = 0x0000000F,

        /// <summary>
        ///  Draw text to the left of radio buttons and checkboxes.
        /// </summary>
        /// <remarks>
        ///  <para>Same as <see cref="RightButton"/>.</para>
        /// </remarks>
        LeftText = Interop.BS_LEFTTEXT,

        /// <summary>
        ///  Draw text to the left of radio buttons and checkboxes.
        /// </summary>
        /// <remarks>
        ///  <para>Same as <see cref="LeftText"/>.</para>
        /// </remarks>
        RightButton = Interop.BS_RIGHTBUTTON,

        // This is the default state- buttons have text
        // BS_TEXT = 0x00000000,

        /// <summary>
        ///  Button only displays an icon (no text) if SetImage (BM_SETIMAGE) is sent.
        /// </summary>
        Icon = Interop.BS_ICON,

        /// <summary>
        ///  Button only displays a bitmap (no text) if SetImage (BM_SETIMAGE) is sent.
        /// </summary>
        Bitmap = Interop.BS_BITMAP,

        /// <summary>
        ///  Align text on the left.
        /// </summary>
        Left = Interop.BS_LEFT,

        /// <summary>
        ///  Align text on the right.
        /// </summary>
        Right = Interop.BS_RIGHT,

        /// <summary>
        ///  Center text horizontally.
        /// </summary>
        Center = Interop.BS_CENTER,

        /// <summary>
        ///  Align text at the top.
        /// </summary>
        Top = Interop.BS_TOP,

        /// <summary>
        ///  Align text at the bottom.
        /// </summary>
        Bottom = Interop.BS_BOTTOM,

        /// <summary>
        ///  Center text vertically.
        /// </summary>
        VerticallyCenter = Interop.BS_VCENTER,

        /// <summary>
        ///  Makes a button look and act like a push button (raised when not pushed, sunken when pushed).
        /// </summary>
        PushLike = Interop.BS_PUSHLIKE,

        /// <summary>
        ///  Wraps text to multiple lines if needed.
        /// </summary>
        Multiline = Interop.BS_MULTILINE,

        /// <summary>
        ///  Enables a button to send KillFocus (BN_KILLFOCUS) and SetFocus (BN_SETFOCUS) notifications to the parent window.
        /// </summary>
        Notify = Interop.BS_NOTIFY,

        /// <summary>
        ///  Button is two-dimensional (doesn't shade).
        /// </summary>
        Flat = Interop.BS_FLAT
    }
}