// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;

public partial struct D2D_RECT_F
{
    public D2D_RECT_F(float left, float top, float right, float bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    public static implicit operator D2D_RECT_F(RectangleF value) =>
        new(value.Left, value.Top, value.Right, value.Bottom);
}