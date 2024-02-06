// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;

public partial struct D2D_SIZE_U
{
    public D2D_SIZE_U(uint width, uint height)
    {
        this.width = width;
        this.height = height;
    }

    public static explicit operator D2D_SIZE_U(Size value) =>
        new(checked((uint)value.Width), checked((uint)value.Height));
}