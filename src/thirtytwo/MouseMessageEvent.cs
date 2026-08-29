// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Delegate for processing mouse messages.
/// </summary>
/// <param name="window">The window receiving the mouse message.</param>
/// <param name="position">The mouse position in client coordinates, measured in physical pixels.</param>
/// <param name="button">The mouse button associated with the message.</param>
/// <param name="mouseState">Modifier and button state flags from the Win32 message payload.</param>
public delegate void MouseMessageEvent(
    Window window,
    Point position,
    MouseButton button,
    MouseKey mouseState);