// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  An interface for layout handlers.
/// </summary>
/// <remarks>
///  <para>
///   Layout is synchronous. Bounds are expressed in physical pixels in the current layout coordinate space. A root
///   bound by <see cref="LayoutBinder"/> receives the target window's client rectangle; rectangles passed to child
///   windows are relative to their parent's client area.
///  </para>
///  <para>
///   Scale is the number of physical pixels per logical layout unit and must be finite and greater than zero.
///   Percentage-based nodes preserve physical proportions and forward the scale unchanged. Nodes with fixed logical
///   dimensions use the scale when converting those dimensions to physical pixels.
///  </para>
///  <para>
///   Built-in nodes use checked arithmetic and throw <see cref="OverflowException"/> when requested geometry cannot
///   be represented by integer bounds. The standard <see cref="LayoutBinder"/> validates scale before dispatch.
///  </para>
/// </remarks>
public interface ILayoutHandler
{
    /// <summary>
    ///  Layout the control using the specified bounds.
    /// </summary>
    /// <param name="bounds">The available bounds in physical pixels in the current layout coordinate space.</param>
    /// <param name="scale">The finite, positive number of physical pixels per logical layout unit.</param>
    void Layout(Rectangle bounds, float scale);
}