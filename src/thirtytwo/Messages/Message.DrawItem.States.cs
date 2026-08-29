// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <inheritdoc cref="DrawItem"/>
    public readonly ref partial struct DrawItem
    {
        /// <summary>
        ///  Flags from <c>DRAWITEMSTRUCT.itemState</c> describing current owner-draw state.
        /// </summary>
        [Flags]
        public enum States : uint
        {
            /// <summary>
            ///  The item is selected.
            /// </summary>
            Selected = ODS_FLAGS.ODS_SELECTED,

            /// <summary>
            ///  The item is grayed.
            /// </summary>
            Grayed = ODS_FLAGS.ODS_GRAYED,

            /// <summary>
            ///  The item is disabled.
            /// </summary>
            Disabled = ODS_FLAGS.ODS_DISABLED,

            /// <summary>
            ///  The item is checked.
            /// </summary>
            Checked = ODS_FLAGS.ODS_CHECKED,

            /// <summary>
            ///  The item has keyboard focus.
            /// </summary>
            Focus = ODS_FLAGS.ODS_FOCUS,

            /// <summary>
            ///  The item is the default item for the control.
            /// </summary>
            Default = ODS_FLAGS.ODS_DEFAULT,

            /// <summary>
            ///  The drawing target is the editable field of a combo box.
            /// </summary>
            ComboBoxEdit = ODS_FLAGS.ODS_COMBOBOXEDIT,

            /// <summary>
            ///  The item is hot-tracked.
            /// </summary>
            HotLight = ODS_FLAGS.ODS_HOTLIGHT,

            /// <summary>
            ///  The item is inactive.
            /// </summary>
            Inactive = ODS_FLAGS.ODS_INACTIVE,

            /// <summary>
            ///  Keyboard accelerators should be hidden.
            /// </summary>
            NoAccelerator = ODS_FLAGS.ODS_NOACCEL,

            /// <summary>
            ///  Focus rectangle should be suppressed.
            /// </summary>
            NoFocusRect = ODS_FLAGS.ODS_NOFOCUSRECT,
        }
    }
}