// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;
/// <remarks>Rectangle edges are expressed in device-independent pixels (DIPs).</remarks>
public partial struct D2D_RECT_F
{
    /// <summary>
    ///  Initializes a rectangle with explicit edge values.
    /// </summary>
    /// <param name="left">The left edge in DIPs.</param>
    /// <param name="top">The top edge in DIPs.</param>
    /// <param name="right">The right edge in DIPs.</param>
    /// <param name="bottom">The bottom edge in DIPs.</param>
    public D2D_RECT_F(float left, float top, float right, float bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    /// <summary>
    ///  Converts a <see cref="RectangleF"/> to a <see cref="D2D_RECT_F"/>.
    /// </summary>
    /// <param name="value">The source rectangle.</param>
    /// <returns>A Direct2D rectangle with matching edge values.</returns>
    public static implicit operator D2D_RECT_F(RectangleF value) =>
        new(value.Left, value.Top, value.Right, value.Bottom);
}