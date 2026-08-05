// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref partial struct DrawItem
    {
        public enum ControlType : uint
        {
            Button = DRAWITEMSTRUCT_CTL_TYPE.ODT_BUTTON,
            ComboBox = DRAWITEMSTRUCT_CTL_TYPE.ODT_COMBOBOX,
            ListBox = DRAWITEMSTRUCT_CTL_TYPE.ODT_LISTBOX,
            ListView = DRAWITEMSTRUCT_CTL_TYPE.ODT_LISTVIEW,
            Menu = DRAWITEMSTRUCT_CTL_TYPE.ODT_MENU,
            Static = DRAWITEMSTRUCT_CTL_TYPE.ODT_STATIC,
            Tab = DRAWITEMSTRUCT_CTL_TYPE.ODT_TAB,
        }
    }
}