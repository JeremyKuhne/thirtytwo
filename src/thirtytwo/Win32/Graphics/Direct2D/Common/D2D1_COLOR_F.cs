// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;
/// <remarks>Channel values are normalized floating-point components.</remarks>
public partial struct D2D1_COLOR_F
{
    /// <summary>
    ///  Initializes a color from normalized channel values.
    /// </summary>
    /// <param name="r">The red channel value.</param>
    /// <param name="g">The green channel value.</param>
    /// <param name="b">The blue channel value.</param>
    /// <param name="a">The alpha channel value.</param>
    public D2D1_COLOR_F(float r, float g, float b, float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    /// <summary>
    ///  Converts a <see cref="Color"/> to a <see cref="D2D1_COLOR_F"/>.
    /// </summary>
    /// <param name="value">The source color.</param>
    /// <returns>A Direct2D color whose channels are derived by dividing byte channels by 255.</returns>
    public static explicit operator D2D1_COLOR_F(Color value) =>
        new(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, value.A / 255.0f);

    /// <summary>
    ///  Converts a <see cref="D2D1_COLOR_F"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The source Direct2D color.</param>
    /// <returns>A color whose byte channels are computed by multiplying channel values by 255.</returns>
    public static explicit operator Color(D2D1_COLOR_F value) =>
        Color.FromArgb(
            (int)(value.a * 255.0f),
            (int)(value.r * 255.0f),
            (int)(value.g * 255.0f),
            (int)(value.b * 255.0f));
}