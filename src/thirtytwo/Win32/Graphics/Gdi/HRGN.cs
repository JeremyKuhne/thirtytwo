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

    public static HRGN FromRectangle(Rectangle rectangle) =>
        Interop.CreateRectRgn(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);

    public static HRGN FromRectangle(int x1, int y1, int x2, int y2) => Interop.CreateRectRgn(x1, y1, x2, y2);

    public static HRGN FromEllipse(Rectangle bounds) =>
        Interop.CreateEllipticRgn(bounds.X, bounds.Y, bounds.Right, bounds.Bottom);

    public static HRGN FromEllipse(int x1, int y1, int x2, int y2) => Interop.CreateEllipticRgn(x1, y1, x2, y2);

    public static HRGN CreateEmpty() => Interop.CreateRectRgn(0, 0, 0, 0);

    public static HRGN CombineRegion(HRGN first, HRGN second, RegionCombineMode combineMode) =>
        CombineRegion(first, second, combineMode, out _);

    public static HRGN CombineRegion(HRGN first, HRGN second, RegionCombineMode combineMode, out RegionType type)
    {
        HRGN hrgn = CreateEmpty();
        type = (RegionType)Interop.CombineRgn(hrgn, first, second, (RGN_COMBINE_MODE)combineMode);
        if (type == RegionType.Error)
        {
            hrgn.Dispose();
        }

        return hrgn;
    }

    public static HRGN FromHdc(HDC hdc)
    {
        HRGN region = Interop.CreateRectRgn(0, 0, 0, 0);
        int result = Interop.GetClipRgn(hdc, region);
        Debug.Assert(result != -1, "GetClipRgn failed");

        if (result == 1)
        {
            return region;
        }
        else
        {
            // No region, delete our temporary region
            Interop.DeleteObject(region);
            return default;
        }
    }
}