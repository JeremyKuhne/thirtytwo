// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets <c>lParam</c> for window-position notifications as a native <c>WINDOWPOS</c>.
    /// </summary>
    public readonly ref struct WindowPositionChanged
    {
        /// <summary>
        ///  Gets the z-order insertion reference window from <c>WINDOWPOS.hwndInsertAfter</c>.
        /// </summary>
        public HWND InsertAfter { get; }

        /// <summary>
        ///  Gets the target window handle from <c>WINDOWPOS.hwnd</c>.
        /// </summary>
        public HWND Handle { get; }

        /// <summary>
        ///  New bounds of the window in screen coordinates.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        ///  Gets position/update flags from <c>WINDOWPOS.flags</c>.
        /// </summary>
        public SET_WINDOW_POS_FLAGS Flags { get; }

        /// <summary>
        ///  Initializes a <see cref="WindowPositionChanged"/> wrapper from an <c>lParam</c> <c>WINDOWPOS*</c> payload.
        /// </summary>
        /// <param name="lParam">The message payload pointer to a native <c>WINDOWPOS</c> structure.</param>
        public unsafe WindowPositionChanged(LPARAM lParam)
        {
            WINDOWPOS* position = (WINDOWPOS*)lParam;
            InsertAfter = position->hwndInsertAfter;
            Handle = position->hwnd;
            Bounds = new(position->x, position->y, position->cx, position->cy);
            Flags = position->flags;
        }
    }
}