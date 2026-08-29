// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Messages;

/// <summary>
///  Receives interpreted mouse input callbacks from a window message stream.
/// </summary>
public interface IMouseMessageHandler
{
    /// <summary>
    ///  Called for client-area pointer movement.
    /// </summary>
    /// <param name="position">
    ///  The cursor location in client coordinates decoded from the message <c>lParam</c> point payload.
    /// </param>
    /// <param name="mouseState">
    ///  Bit flags decoded from message <c>wParam</c> indicating button and modifier key state (for example
    ///  <c>MK_LBUTTON</c>, <c>MK_SHIFT</c>, and <c>MK_CONTROL</c>).
    /// </param>
    public void OnMouseMove(Point position, MouseKey mouseState) { }

    /// <summary>
    ///  Called when a mouse button is pressed in the client area.
    /// </summary>
    /// <param name="position">
    ///  The cursor location in client coordinates decoded from the message <c>lParam</c> point payload.
    /// </param>
    /// <param name="button">The logical button associated with the message.</param>
    /// <param name="mouseState">
    ///  Bit flags decoded from message <c>wParam</c> indicating button and modifier key state.
    /// </param>
    public void OnButtonDown(Point position, MouseButton button, MouseKey mouseState) { }

    /// <summary>
    ///  Called when a mouse button is released in the client area.
    /// </summary>
    /// <param name="position">
    ///  The cursor location in client coordinates decoded from the message <c>lParam</c> point payload.
    /// </param>
    /// <param name="button">The logical button associated with the message.</param>
    /// <param name="mouseState">
    ///  Bit flags decoded from message <c>wParam</c> indicating button and modifier key state.
    /// </param>
    public void OnButtonUp(Point position, MouseButton button, MouseKey mouseState) { }
}