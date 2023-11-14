// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

public unsafe partial struct HRGN : IDisposable
{
    public void Dispose()
    {
        if (!IsNull)
        {
            Interop.DeleteObject(this);
        }

        Unsafe.AsRef(in this) = default;
    }

    public static HRGN FromRectangle(Rectangle rectangle)
        => Interop.CreateRectRgn(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);

    public static HRGN CreateEmpty()
        => Interop.CreateRectRgn(0, 0, 0, 0);
}