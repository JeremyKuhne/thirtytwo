// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public partial class RichEditControl
{
    [Flags]
    public enum Styles : uint
    {
        /// <inheritdoc cref="EditControl.Styles.Left"/>
        Left = EditControl.Styles.Left,

        /// <inheritdoc cref="EditControl.Styles.Center"/>
        Center = EditControl.Styles.Center,

        /// <inheritdoc cref="EditControl.Styles.Right"/>
        Right = EditControl.Styles.Right,

        /// <inheritdoc cref="EditControl.Styles.Multiline"/>
        Multiline = EditControl.Styles.Multiline,

        /// <inheritdoc cref="EditControl.Styles.Password"/>
        Password = EditControl.Styles.Password,

        /// <inheritdoc cref="EditControl.Styles.AutoVerticalScroll"/>
        AutoVerticalScroll = EditControl.Styles.AutoVerticalScroll,

        /// <inheritdoc cref="EditControl.Styles.AutoHorizontalScroll"/>
        AutoHorizontalScroll = EditControl.Styles.AutoHorizontalScroll,

        /// <inheritdoc cref="EditControl.Styles.NoHideSelection"/>
        NoHideSelection = EditControl.Styles.NoHideSelection,

        /// <inheritdoc cref="EditControl.Styles.ReadOnly"/>
        ReadOnly = EditControl.Styles.ReadOnly,

        /// <inheritdoc cref="EditControl.Styles.WantReturn"/>
        WantReturn = EditControl.Styles.WantReturn,

        /// <inheritdoc cref="EditControl.Styles.Number"/>
        Number = EditControl.Styles.Number
    }
}