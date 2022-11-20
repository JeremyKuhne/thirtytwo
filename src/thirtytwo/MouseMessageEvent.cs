// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Delegate for processing mouse messages.
/// </summary>
public delegate void MouseMessageEvent(
    Window window,
    Point position,
    MouseButton button,
    MouseKey mouseState);