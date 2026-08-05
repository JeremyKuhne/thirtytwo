// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Messages;

public partial class EnterIdleHandler
{
    /// <summary>
    ///  Delegate for processing idle events.
    /// </summary>
    /// <param name="isDialog"><see langword="true"/> if dialog is displayed, otherwise a menu is displayed.</param>
    /// <param name="handle">Dialog handle if is <see langword="true"/>, or parent window handle.</param>
    public delegate void EnterIdleEvent(bool isDialog, HWND handle);
}