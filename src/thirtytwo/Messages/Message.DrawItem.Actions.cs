// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <inheritdoc cref="DrawItem"/>
    public readonly ref partial struct DrawItem
    {
        /// <summary>
        ///  Flags from <c>DRAWITEMSTRUCT.itemAction</c> that describe why owner-draw rendering is requested.
        /// </summary>
        [Flags]
        public enum Actions : uint
        {
            /// <summary>
            ///  Redraw the entire item.
            /// </summary>
            DrawEntire = ODA_FLAGS.ODA_DRAWENTIRE,

            /// <summary>
            ///  Update the item's selection appearance.
            /// </summary>
            Select = ODA_FLAGS.ODA_SELECT,

            /// <summary>
            ///  Update the item's focus rectangle appearance.
            /// </summary>
            Focus = ODA_FLAGS.ODA_FOCUS,
        }
    }
}