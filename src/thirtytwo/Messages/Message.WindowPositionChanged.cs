// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public readonly ref struct WindowPositionChanged
    {
        public HWND InsertAfter { get; }
        public HWND Handle { get; }

        /// <summary>
        ///  New bounds of the window in screen coordinates.
        /// </summary>
        public Rectangle Bounds { get; }

        public SET_WINDOW_POS_FLAGS Flags { get; }

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