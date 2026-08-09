// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public static partial class Message
{
    public unsafe readonly ref struct DpiChanged
    {
        public DpiChanged(WPARAM wParam, LPARAM lParam)
        {
            if (lParam == 0)
            {
                throw new ArgumentNullException(nameof(lParam));
            }

            Dpi = wParam.HIWORD;
            SuggestedBounds = *(RECT*)lParam;
        }

        public uint Dpi { get; }

        public Rectangle SuggestedBounds { get; }
    }
}