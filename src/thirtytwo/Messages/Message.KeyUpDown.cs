// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Windows;

public static partial class Message
{
    /// <summary>
    ///  Interprets keyboard-message <c>wParam</c> payload as a virtual-key value.
    /// </summary>
    /// <param name="wParam">The message <c>wParam</c> that carries a <c>VIRTUAL_KEY</c> value.</param>
    public readonly ref struct KeyUpDown(WPARAM wParam)
    {
        /// <summary>
        ///  Gets the virtual key code from <c>wParam</c>.
        /// </summary>
        public VIRTUAL_KEY Key { get; } = (VIRTUAL_KEY)(nuint)wParam;
    }
}