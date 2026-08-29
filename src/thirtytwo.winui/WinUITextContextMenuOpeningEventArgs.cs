// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.WinUI;

/// <summary>
///  Provides a context-menu opening request.
/// </summary>
/// <param name="cursorPosition">The requested cursor position in editor coordinates.</param>
public sealed class WinUITextContextMenuOpeningEventArgs(PointF cursorPosition) : EventArgs
{
    /// <summary>
    ///  Gets the requested cursor position in editor coordinates.
    /// </summary>
    public PointF CursorPosition { get; } = cursorPosition;

    /// <summary>
    ///  Gets or sets whether the request was handled.
    /// </summary>
    /// <remarks>
    ///  When set to <see langword="true"/>, the wrapper writes the value back to WinUI and suppresses default menu
    ///  handling.
    /// </remarks>
    public bool Handled { get; set; }
}