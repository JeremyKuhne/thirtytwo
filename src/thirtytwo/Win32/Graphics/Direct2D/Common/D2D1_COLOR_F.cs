// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Graphics.Direct2D.Common;

public partial struct D2D1_COLOR_F
{
    public D2D1_COLOR_F(float r, float g, float b, float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public static explicit operator D2D1_COLOR_F(Color value) =>
        new(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f, value.A / 255.0f);
}