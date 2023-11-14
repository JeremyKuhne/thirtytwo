// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public unsafe readonly ref struct DpiChanged(WPARAM wParam, LPARAM lParam)
    {
        public uint Dpi { get; } = wParam.HIWORD;
        public Rectangle SuggestedBounds { get; } = *(RECT*)lParam;
    }
}