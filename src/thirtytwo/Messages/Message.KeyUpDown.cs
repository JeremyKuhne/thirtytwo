// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Windows;

public static partial class Message
{
    public readonly ref struct KeyUpDown
    {
        public KeyUpDown(WPARAM wParam, LPARAM lParam)
        {
            Key = (VIRTUAL_KEY)(nuint)wParam;
        }

        public VIRTUAL_KEY Key { get; }
    }
}