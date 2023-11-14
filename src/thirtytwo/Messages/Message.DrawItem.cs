// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public readonly unsafe ref struct DrawItem(LPARAM lParam)
    {
        private readonly DRAWITEMSTRUCT* _drawItemStruct = (DRAWITEMSTRUCT*)lParam;

        public ControlType Type => (ControlType)_drawItemStruct->CtlType;
        public uint ControlId => _drawItemStruct->CtlID;
        public uint ItemId => _drawItemStruct->itemID;
        public Actions ItemAction => (Actions)_drawItemStruct->itemAction;
        public States ItemState => (States)_drawItemStruct->itemState;
        public HWND ItemWindow => _drawItemStruct->hwndItem;
        public DeviceContext DeviceContext => DeviceContext.Create(_drawItemStruct->hDC);
        public Rectangle ItemRectangle => _drawItemStruct->rcItem;
        public nuint ItemData => _drawItemStruct->itemData;

        [Flags]
        public enum Actions : uint
        {
            DrawEntire = ODA_FLAGS.ODA_DRAWENTIRE,
            Select = ODA_FLAGS.ODA_SELECT,
            Focus = ODA_FLAGS.ODA_FOCUS,
        }

        [Flags]
        public enum States : uint
        {
            Selected = ODS_FLAGS.ODS_SELECTED,
            Grayed = ODS_FLAGS.ODS_GRAYED,
            Disabled = ODS_FLAGS.ODS_DISABLED,
            Checked = ODS_FLAGS.ODS_CHECKED,
            Focus = ODS_FLAGS.ODS_FOCUS,
            Default = ODS_FLAGS.ODS_DEFAULT,
            ComboBoxEdit = ODS_FLAGS.ODS_COMBOBOXEDIT,
            HotLight = ODS_FLAGS.ODS_HOTLIGHT,
            Inactive = ODS_FLAGS.ODS_INACTIVE,
            NoAccelerator = ODS_FLAGS.ODS_NOACCEL,
            NoFocusRect = ODS_FLAGS.ODS_NOFOCUSRECT,
        }

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