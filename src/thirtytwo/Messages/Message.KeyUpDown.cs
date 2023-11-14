// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Windows;

public static partial class Message
{
#pragma warning disable CS9113 // LPARAM is needed for other details, just haven't implemented yet.
    public readonly ref struct KeyUpDown(WPARAM wParam, LPARAM lParam)
#pragma warning restore CS9113
    {
        public VIRTUAL_KEY Key { get; } = (VIRTUAL_KEY)(nuint)wParam;
    }
}