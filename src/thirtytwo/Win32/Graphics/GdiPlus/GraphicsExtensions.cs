// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Support;
using Drawing = System.Drawing;

namespace Windows.Win32.Graphics.GdiPlus;

public static unsafe class GraphicsExtensions
{
    public static void SetSmoothingMode<T>(this T graphics, SmoothingMode smoothingMode) where T : IPointer<GpGraphics>
    {
        Interop.GdipSetSmoothingMode(graphics.Pointer, smoothingMode).ThrowIfFailed();
        GC.KeepAlive(graphics);
    }

    public static unsafe void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Drawing.Point> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen> =>
        DrawLines(graphics, pen, MemoryMarshal.Cast<Drawing.Point, Point>(points));

    public static unsafe void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Point> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen>
    {
        fixed (Point* p = points)
        {
            Interop.GdipDrawLinesI(graphics.Pointer, pen.Pointer, p, points.Length).ThrowIfFailed();
        }

        GC.KeepAlive(graphics);
        GC.KeepAlive(pen);
    }

    public static unsafe void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Drawing.PointF> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen> =>
        DrawLines(graphics, pen, MemoryMarshal.Cast<Drawing.PointF, PointF>(points));

    public static unsafe void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<PointF> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen>
    {
        fixed (PointF* p = points)
        {
            Interop.GdipDrawLines(graphics.Pointer, pen.Pointer, p, points.Length).ThrowIfFailed();
        }

        GC.KeepAlive(graphics);
        GC.KeepAlive(pen);
    }

    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, Drawing.Rectangle bounds)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, (float)bounds.X, bounds.Y, bounds.Width, bounds.Height);

    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, Drawing.RectangleF bounds)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, int x, int y, int width, int height)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, (float)x, y, width, height);

    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, float x, float y, float width, float height)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
    {
        Interop.GdipFillEllipse(graphics.Pointer, brush.Pointer, x, y, width, height).ThrowIfFailed();
        GC.KeepAlive(graphics);
        GC.KeepAlive(brush);
    }
}