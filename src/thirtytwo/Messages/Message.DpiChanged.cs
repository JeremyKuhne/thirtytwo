// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>WM_DPICHANGED</c> payload values.
    /// </summary>
    public unsafe readonly ref struct DpiChanged
    {
        /// <summary>
        ///  Initializes a <see cref="DpiChanged"/> wrapper from message payload values.
        /// </summary>
        /// <param name="wParam">
        ///  The message <c>wParam</c>; the high word is interpreted as the new window DPI value.
        /// </param>
        /// <param name="lParam">
        ///  The message <c>lParam</c>; interpreted as a pointer to a suggested <c>RECT</c> in screen coordinates.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="lParam"/> is <c>0</c>.</exception>
        public DpiChanged(WPARAM wParam, LPARAM lParam)
        {
            if (lParam == 0)
            {
                throw new ArgumentNullException(nameof(lParam));
            }

            Dpi = wParam.HIWORD;
            SuggestedBounds = *(RECT*)lParam;
        }

        /// <summary>
        ///  Gets the DPI value decoded from the high word of <c>wParam</c>.
        /// </summary>
        public uint Dpi { get; }

        /// <summary>
        ///  Gets the suggested new window bounds decoded from the <c>RECT</c> pointed to by <c>lParam</c>.
        /// </summary>
        public Rectangle SuggestedBounds { get; }
    }
}