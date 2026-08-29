// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <inheritdoc cref="DrawItem"/>
    public readonly ref partial struct DrawItem
    {
        /// <summary>
        ///  Values from <c>DRAWITEMSTRUCT.CtlType</c> identifying the owner-draw source control class.
        /// </summary>
        public enum ControlType : uint
        {
            /// <summary>
            ///  Owner-drawn button control.
            /// </summary>
            Button = DRAWITEMSTRUCT_CTL_TYPE.ODT_BUTTON,

            /// <summary>
            ///  Owner-drawn combo box control.
            /// </summary>
            ComboBox = DRAWITEMSTRUCT_CTL_TYPE.ODT_COMBOBOX,

            /// <summary>
            ///  Owner-drawn list box control.
            /// </summary>
            ListBox = DRAWITEMSTRUCT_CTL_TYPE.ODT_LISTBOX,

            /// <summary>
            ///  Owner-drawn list view control.
            /// </summary>
            ListView = DRAWITEMSTRUCT_CTL_TYPE.ODT_LISTVIEW,

            /// <summary>
            ///  Owner-drawn menu item.
            /// </summary>
            Menu = DRAWITEMSTRUCT_CTL_TYPE.ODT_MENU,

            /// <summary>
            ///  Owner-drawn static control.
            /// </summary>
            Static = DRAWITEMSTRUCT_CTL_TYPE.ODT_STATIC,

            /// <summary>
            ///  Owner-drawn tab control.
            /// </summary>
            Tab = DRAWITEMSTRUCT_CTL_TYPE.ODT_TAB,
        }
    }
}