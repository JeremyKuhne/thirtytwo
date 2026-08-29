// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>WM_SIZE</c> payload values.
    /// </summary>
    /// <param name="wParam">The message <c>wParam</c> that identifies the resize reason.</param>
    /// <param name="lParam">The message <c>lParam</c> that carries new client width/height in low/high words.</param>
    public readonly ref partial struct Size(WPARAM wParam, LPARAM lParam)
    {
        /// <summary>
        ///  Gets the new client size decoded from <c>lParam</c> low/high words.
        /// </summary>
        public System.Drawing.Size NewSize { get; } = new System.Drawing.Size(lParam.LOWORD, lParam.HIWORD);

        /// <summary>
        ///  Gets the resize cause decoded from <c>wParam</c>.
        /// </summary>
        public SizeType Type { get; } = (SizeType)(int)wParam;
    }
}