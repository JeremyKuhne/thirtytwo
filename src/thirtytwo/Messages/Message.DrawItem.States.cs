// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref partial struct DrawItem
    {
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
    }
}