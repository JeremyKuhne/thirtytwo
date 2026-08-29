// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.WinUI;

/// <summary>
///  Provides the previous and current colors for a <see cref="WinUIColorPicker.ColorChanged"/> event.
/// </summary>
/// <param name="oldColor">
///  The previous color payload, represented as <see cref="Color"/> with alpha, red, green, and blue channels.
/// </param>
/// <param name="newColor">
///  The current color payload, represented as <see cref="Color"/> with alpha, red, green, and blue channels.
/// </param>
/// <remarks>
///  <para>
///   The owning picker converts WinUI colors to <see cref="Color"/> by mapping A, R, G, and B channels directly to
///   <see cref="Color.FromArgb(int, int, int, int)"/>.
///  </para>
/// </remarks>
public sealed class WinUIColorChangedEventArgs(Color oldColor, Color newColor) : EventArgs
{
    /// <summary>
    ///  Gets the color before the change.
    /// </summary>
    public Color OldColor { get; } = oldColor;

    /// <summary>
    ///  Gets the color after the change.
    /// </summary>
    public Color NewColor { get; } = newColor;
}