// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;
/// <remarks>Direct2D point coordinates are expressed in device-independent pixels (DIPs).</remarks>
public partial struct D2D_POINT_2F
{
    /// <summary>
    ///  Initializes a point with explicit coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate in DIPs.</param>
    /// <param name="y">The vertical coordinate in DIPs.</param>
    public D2D_POINT_2F(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    ///  Converts a <see cref="PointF"/> to a <see cref="D2D_POINT_2F"/>.
    /// </summary>
    /// <param name="value">The source point.</param>
    /// <returns>A Direct2D point with matching coordinates.</returns>
    public static implicit operator D2D_POINT_2F(PointF value) =>
        new(value.X, value.Y);
}