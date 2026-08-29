// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Messages;

public partial class EnterIdleHandler
{
    /// <summary>
    ///  Handles a <see cref="MessageType.EnterIdle"/> notification.
    /// </summary>
    /// <param name="isDialog">
    ///  <see langword="true"/> when <c>wParam</c> is <c>MSGF_DIALOGBOX</c>; otherwise the source is a menu loop.
    /// </param>
    /// <param name="handle">
    ///  The native handle carried in <c>lParam</c>: the dialog HWND when <paramref name="isDialog"/> is
    ///  <see langword="true"/>, or the owner/parent window HWND for menu idle.
    /// </param>
    public delegate void EnterIdleEvent(bool isDialog, HWND handle);
}