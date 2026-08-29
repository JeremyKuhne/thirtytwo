// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Windows.Win32.Graphics.Gdi;

/// <summary>
///  GDI region handle (<c>HRGN</c>).
/// </summary>
public unsafe partial struct HRGN : IDisposable
{
    /// <summary>
    ///  Deletes the region with <c>DeleteObject</c> when it is neither null nor <see cref="Full"/>, then clears this wrapper.
    /// </summary>
    public void Dispose()
    {
        if (IsFull)
        {
            return;
        }

        if (!IsNull)
        {
            PInvoke.DeleteObject(this);
        }

        Unsafe.AsRef(in this) = default;
    }

    /// <summary>
    ///  Special <see cref="HRGN"/> sent during WM_NCPAINT to indicate the entire window.
    /// </summary>
    public static HRGN Full { get; } = (HRGN)(nint)1;

    /// <summary>
    ///  Is special <see cref="HRGN.Full"/> value.
    /// </summary>
    public bool IsFull => Value == Full.Value;

    // There is also a special HRGN_MONITOR (2) that is used when maximized. Not sure if this escapes.

    /// <summary>
    ///  Creates a rectangular region from a rectangle.
    /// </summary>
    /// <param name="rectangle">Rectangle bounds.</param>
    /// <returns>A region created by <c>CreateRectRgn</c>.</returns>
    public static HRGN FromRectangle(Rectangle rectangle) =>
        PInvoke.CreateRectRgn(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);

    /// <summary>
    ///  Creates a rectangular region.
    /// </summary>
    /// <param name="x1">Left edge, in logical units.</param>
    /// <param name="y1">Top edge, in logical units.</param>
    /// <param name="x2">Right edge, in logical units.</param>
    /// <param name="y2">Bottom edge, in logical units.</param>
    /// <returns>A region created by <c>CreateRectRgn</c>.</returns>
    public static HRGN FromRectangle(int x1, int y1, int x2, int y2) => PInvoke.CreateRectRgn(x1, y1, x2, y2);

    /// <summary>
    ///  Creates an elliptic region from a bounding rectangle.
    /// </summary>
    /// <param name="bounds">Bounding rectangle of the ellipse.</param>
    /// <returns>A region created by <c>CreateEllipticRgn</c>.</returns>
    public static HRGN FromEllipse(Rectangle bounds) =>
        PInvoke.CreateEllipticRgn(bounds.X, bounds.Y, bounds.Right, bounds.Bottom);

    /// <summary>
    ///  Creates an elliptic region.
    /// </summary>
    /// <param name="x1">Left edge, in logical units.</param>
    /// <param name="y1">Top edge, in logical units.</param>
    /// <param name="x2">Right edge, in logical units.</param>
    /// <param name="y2">Bottom edge, in logical units.</param>
    /// <returns>A region created by <c>CreateEllipticRgn</c>.</returns>
    public static HRGN FromEllipse(int x1, int y1, int x2, int y2) => PInvoke.CreateEllipticRgn(x1, y1, x2, y2);

    /// <summary>
    ///  Creates an empty region.
    /// </summary>
    /// <returns>A region created by <c>CreateRectRgn(0, 0, 0, 0)</c>.</returns>
    public static HRGN CreateEmpty() => PInvoke.CreateRectRgn(0, 0, 0, 0);

    /// <inheritdoc cref="Combine(HRGN, RegionCombineMode, out RegionType)"/>
    /// <param name="region">The second source region.</param>
    /// <param name="combineMode">The region combine operation.</param>
    /// <returns>The combined region, or <see langword="default"/> when combination fails.</returns>
    public HRGN Combine(HRGN region, RegionCombineMode combineMode) =>
        Combine(region, combineMode, out _);

    /// <summary>
    ///  Combines this region with another region and returns a new region handle.
    /// </summary>
    /// <param name="region">The second source region.</param>
    /// <param name="combineMode">The region combine operation passed to <c>CombineRgn</c>.</param>
    /// <param name="type">Receives the resulting region type from <c>CombineRgn</c>.</param>
    /// <returns>
    ///  A newly created region containing the result. If <paramref name="type"/> is <see cref="RegionType.Error"/>,
    ///  the temporary destination is disposed and <see langword="default"/> is returned.
    /// </returns>
    public HRGN Combine(HRGN region, RegionCombineMode combineMode, out RegionType type)
    {
        HRGN hrgn = CreateEmpty();
        type = (RegionType)PInvoke.CombineRgn(hrgn, this, region, (RGN_COMBINE_MODE)combineMode);
        if (type == RegionType.Error)
        {
            hrgn.Dispose();
        }

        return hrgn;
    }

    /// <inheritdoc cref="Copy(out RegionType)"/>
    /// <returns>A copy of this region, or <see langword="default"/> on failure.</returns>
    public HRGN Copy() => Copy(out _);

    /// <summary>
    ///  Copies this region.
    /// </summary>
    /// <param name="type">Receives the resulting region type from <c>CombineRgn</c>.</param>
    /// <returns>
    ///  A newly created copy. If <paramref name="type"/> is <see cref="RegionType.Error"/>,
    ///  the temporary destination is disposed and <see langword="default"/> is returned.
    /// </returns>
    public HRGN Copy(out RegionType type)
    {
        HRGN hrgn = CreateEmpty();
        type = (RegionType)PInvoke.CombineRgn(hrgn, this, default, (RGN_COMBINE_MODE)RegionCombineMode.Copy);
        if (type == RegionType.Error)
        {
            hrgn.Dispose();
        }

        return hrgn;
    }

    /// <summary>
    ///  Gets a copy of the clipping region currently selected in a device context.
    /// </summary>
    /// <param name="hdc">The device context to query.</param>
    /// <returns>
    ///  A region copy when <c>GetClipRgn</c> returns 1; otherwise <see langword="default"/> when no clipping
    ///  region is present. On failure (<c>-1</c>), this method only asserts in debug builds and returns
    ///  <see langword="default"/>.
    /// </returns>
    public static HRGN FromHdc(HDC hdc)
    {
        HRGN region = PInvoke.CreateRectRgn(0, 0, 0, 0);
        int result = PInvoke.GetClipRgn(hdc, region);
        Debug.Assert(result != -1, "GetClipRgn failed");

        if (result == 1)
        {
            return region;
        }
        else
        {
            // No region, delete our temporary region
            PInvoke.DeleteObject(region);
            return default;
        }
    }
}