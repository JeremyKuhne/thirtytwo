// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Specifies mouse buttons and modifier keys held during an OLE drag-and-drop operation.</summary>
[Flags]
public enum DragDropKeyStates : uint
{
    /// <summary>No mouse buttons or modifier keys are held.</summary>
    None = 0,

    /// <summary>The left mouse button is held.</summary>
    LeftMouseButton = 0x0001,

    /// <summary>The right mouse button is held.</summary>
    RightMouseButton = 0x0002,

    /// <summary>The Shift key is held.</summary>
    ShiftKey = 0x0004,

    /// <summary>The Control key is held.</summary>
    ControlKey = 0x0008,

    /// <summary>The middle mouse button is held.</summary>
    MiddleMouseButton = 0x0010,

    /// <summary>The Alt key is held.</summary>
    AltKey = 0x0020
}
