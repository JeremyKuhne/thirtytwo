// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public partial class EditControl
{
    [Flags]
    public enum Styles : uint
    {
        /// <summary>
        ///  Left align text.
        /// </summary>
        Left = Interop.ES_LEFT,

        /// <summary>
        ///  Center text.
        /// </summary>
        Center = Interop.ES_CENTER,

        /// <summary>
        ///  Right align text.
        /// </summary>
        Right = Interop.ES_RIGHT,

        /// <summary>
        ///  Multiline.
        /// </summary>
        Multiline = Interop.ES_MULTILINE,

        /// <summary>
        ///  Convert all characters to uppercase as typed.
        /// </summary>
        Uppercase = Interop.ES_UPPERCASE,

        /// <summary>
        ///  Convert all characters to lowercase as typed.
        /// </summary>
        Lowercase = Interop.ES_LOWERCASE,

        /// <summary>
        ///  Display asterisks for characters as typed.
        /// </summary>
        Password = Interop.ES_PASSWORD,

        AutoVerticalScroll = Interop.ES_AUTOVSCROLL,

        AutoHorizontalScroll = Interop.ES_AUTOHSCROLL,

        /// <summary>
        ///  Keep selection highlighted when focus is lost.
        /// </summary>
        NoHideSelection = Interop.ES_NOHIDESEL,

        /// <summary>
        ///  Converts to OEM character set.
        /// </summary>
        OemConvert = Interop.ES_OEMCONVERT,

        /// <summary>
        ///  Prevent typing in the control.
        /// </summary>
        ReadOnly = Interop.ES_READONLY,

        /// <summary>
        ///  Enter key insters carriage return rather than being sent to default push button.
        /// </summary>
        WantReturn = Interop.ES_WANTRETURN,

        /// <summary>
        ///  Allow only digits to be typed.
        /// </summary>
        Number = Interop.ES_NUMBER
    }
}