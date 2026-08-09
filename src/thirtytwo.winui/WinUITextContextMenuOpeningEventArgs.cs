// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.WinUI;

/// <summary>Provides a context-menu opening request.</summary>
public sealed class WinUITextContextMenuOpeningEventArgs(PointF cursorPosition) : EventArgs
{
    /// <summary>Gets the requested cursor position in editor coordinates.</summary>
    public PointF CursorPosition { get; } = cursorPosition;

    /// <summary>Gets or sets whether the request was handled.</summary>
    public bool Handled { get; set; }
}