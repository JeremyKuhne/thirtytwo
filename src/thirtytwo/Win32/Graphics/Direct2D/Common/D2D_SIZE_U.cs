// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;

public partial struct D2D_SIZE_U
{
    /// <summary>
    ///  Initializes a size with explicit dimensions.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    public D2D_SIZE_U(uint width, uint height)
    {
        this.width = width;
        this.height = height;
    }

    /// <summary>
    ///  Converts a <see cref="Size"/> to a <see cref="D2D_SIZE_U"/>.
    /// </summary>
    /// <param name="value">The source size.</param>
    /// <returns>A Direct2D size with matching dimensions.</returns>
    /// <exception cref="OverflowException">
    ///  Thrown when <paramref name="value"/> contains a negative or out-of-range dimension for <see cref="uint"/>.
    /// </exception>
    public static explicit operator D2D_SIZE_U(Size value) =>
        new(checked((uint)value.Width), checked((uint)value.Height));
}