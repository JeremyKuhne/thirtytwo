// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>Provides data and target feedback for an OLE drag-and-drop operation.</summary>
public sealed class DragEventArgs : EventArgs
{
    internal DragEventArgs(
        DropDataObject data,
        DragDropKeyStates keyState,
        Point screenLocation,
        DragDropEffects allowedEffect)
    {
        Data = data;
        KeyState = keyState;
        ScreenLocation = screenLocation;
        AllowedEffect = allowedEffect;
    }

    /// <summary>Gets the data being dragged.</summary>
    /// <remarks>The data object is valid only while the event handler is running.</remarks>
    public DropDataObject Data { get; }

    /// <summary>Gets the mouse-button and modifier-key state.</summary>
    public DragDropKeyStates KeyState { get; }

    /// <summary>Gets the cursor location in physical screen pixels.</summary>
    public Point ScreenLocation { get; }

    /// <summary>Gets the effects permitted by the drag source.</summary>
    public DragDropEffects AllowedEffect { get; }

    /// <summary>Gets or sets the effect selected by the drop target.</summary>
    public DragDropEffects Effect { get; set; }
}
