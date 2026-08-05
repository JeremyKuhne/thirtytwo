// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class Message
{
    public readonly ref partial struct Size(WPARAM wParam, LPARAM lParam)
    {
        public System.Drawing.Size NewSize { get; } = new System.Drawing.Size(lParam.LOWORD, lParam.HIWORD);
        public SizeType Type { get; } = (SizeType)(int)wParam;
    }
}