// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public readonly ref struct DpiChanged
    {
        public uint Dpi { get; }
        public Rectangle SuggestedBounds { get; }

        public unsafe DpiChanged(WPARAM wParam, LPARAM lParam)
        {
            Dpi = wParam.HIWORD;
            SuggestedBounds = *(RECT*)lParam;
        }
    }
}