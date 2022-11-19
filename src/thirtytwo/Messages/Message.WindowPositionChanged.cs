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
        public Rectangle Bounds { get; }
        public Rectangle ClientBounds { get; }

        public unsafe WindowPositionChanged(LPARAM lParam)
        {
            WINDOWPOS* position = (WINDOWPOS*)lParam;
            InsertAfter = position->hwndInsertAfter;
            Handle = position->hwnd;
            Rectangle bounds = new(position->x, position->y, position->cx, position->cy);
            Handle.ScreenToClient(ref bounds);
            Bounds = bounds;
            ClientBounds = Handle.GetClientRectangle();
        }
    }
}