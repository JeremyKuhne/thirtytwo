// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref struct Size(WPARAM wParam, LPARAM lParam)
    {
        public System.Drawing.Size NewSize { get; } = new System.Drawing.Size(lParam.LOWORD, lParam.HIWORD);
        public SizeType Type { get; } = (SizeType)(int)wParam;

        public enum SizeType
        {
            /// <summary>
            ///  [SIZE_RESTORED]
            /// </summary>
            Restored = 0,

            /// <summary>
            ///  [SIZE_MINIMIZED]
            /// </summary>
            Minimized = 1,

            /// <summary>
            ///  [SIZE_MAXIMIZED]
            /// </summary>
            Maximized = 2,

            /// <summary>
            ///  [SIZE_MAXSHOW]
            /// </summary>
            MaxShow = 3,

            /// <summary>
            ///  [SIZE_MAXHIDE]
            /// </summary>
            MaxHide = 4
        }
    }
}