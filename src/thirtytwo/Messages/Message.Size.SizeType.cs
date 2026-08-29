// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <inheritdoc cref="Size"/>
    public readonly ref partial struct Size
    {
        /// <summary>
        ///  Resize reason values carried in <c>WM_SIZE</c> <c>wParam</c>.
        /// </summary>
        public enum SizeType
        {
            /// <summary>
            ///  The window has been restored from minimized or maximized state.
            /// </summary>
            Restored = 0,

            /// <summary>
            ///  The window has been minimized.
            /// </summary>
            Minimized = 1,

            /// <summary>
            ///  The window has been maximized.
            /// </summary>
            Maximized = 2,

            /// <summary>
            ///  A maximized pop-up window is being shown.
            /// </summary>
            MaxShow = 3,

            /// <summary>
            ///  A maximized pop-up window is being hidden.
            /// </summary>
            MaxHide = 4
        }
    }
}