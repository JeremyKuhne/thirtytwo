// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public partial class ComboBoxControl
{
    [Flags]
    public enum Styles : uint
    {
        /// <summary>
        ///  Simple combo box style (CBS_SIMPLE).
        /// </summary>
        Simple = Interop.CBS_SIMPLE,

        /// <summary>
        ///  Dropdown combo box style (CBS_DROPDOWN).
        /// </summary>
        DropDown = Interop.CBS_DROPDOWN,

        /// <summary>
        ///  Dropdown list combo box style (CBS_DROPDOWNLIST).
        /// </summary>
        DropDownList = Interop.CBS_DROPDOWNLIST
    }
}