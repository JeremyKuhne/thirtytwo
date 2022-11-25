// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Messages;

public interface IMouseMessageHandler
{
    public void OnMouseMove(Point position, MouseKey mouseState) { }
    public void OnButtonDown(Point position, MouseButton button, MouseKey mouseState) { }
    public void OnButtonUp(Point position, MouseButton button, MouseKey mouseState) { }
}