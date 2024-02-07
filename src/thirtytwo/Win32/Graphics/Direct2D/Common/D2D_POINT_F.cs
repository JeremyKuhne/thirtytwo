// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;

public partial struct D2D_POINT_2F
{
    public D2D_POINT_2F(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator D2D_POINT_2F(PointF value) =>
        new(value.X, value.Y);
}