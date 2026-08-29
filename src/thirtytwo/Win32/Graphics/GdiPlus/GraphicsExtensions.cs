// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Support;
using Drawing = System.Drawing;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Provides extension methods for drawing with GDI+ graphics objects.
/// </summary>
public static unsafe class GraphicsExtensions
{
    /// <summary>
    ///  Sets the smoothing mode for rendering operations.
    /// </summary>
    /// <typeparam name="T">The graphics wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to update.</param>
    /// <param name="smoothingMode">The smoothing mode to apply.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public static void SetSmoothingMode<T>(this T graphics, SmoothingMode smoothingMode) where T : IPointer<GpGraphics>
    {
        PInvoke.GdipSetSmoothingMode(graphics.Pointer, smoothingMode).ThrowIfFailed();
        GC.KeepAlive(graphics);
    }

    /// <summary>
    ///  Draws connected line segments through the specified points.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TPen">The pen wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="pen">The pen used to draw line segments.</param>
    /// <param name="points">The points that define the connected line segments.</param>
    public static void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Drawing.Point> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen> =>
        DrawLines(graphics, pen, MemoryMarshal.Cast<Drawing.Point, Point>(points));

    /// <summary>
    ///  Draws connected line segments through the specified points.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TPen">The pen wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="pen">The pen used to draw line segments.</param>
    /// <param name="points">The points that define the connected line segments.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public static void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Point> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen>
    {
        fixed (Point* p = points)
        {
            PInvoke.GdipDrawLinesI(graphics.Pointer, pen.Pointer, p, points.Length).ThrowIfFailed();
        }

        GC.KeepAlive(graphics);
        GC.KeepAlive(pen);
    }

    /// <summary>
    ///  Draws connected line segments through the specified points.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TPen">The pen wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="pen">The pen used to draw line segments.</param>
    /// <param name="points">The points that define the connected line segments.</param>
    public static void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<Drawing.PointF> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen> =>
        DrawLines(graphics, pen, MemoryMarshal.Cast<Drawing.PointF, PointF>(points));

    /// <summary>
    ///  Draws connected line segments through the specified points.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TPen">The pen wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="pen">The pen used to draw line segments.</param>
    /// <param name="points">The points that define the connected line segments.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public static void DrawLines<TGraphics, TPen>(this TGraphics graphics, TPen pen, ReadOnlySpan<PointF> points)
        where TGraphics : IPointer<GpGraphics>
        where TPen : IPointer<GpPen>
    {
        fixed (PointF* p = points)
        {
            PInvoke.GdipDrawLines(graphics.Pointer, pen.Pointer, p, points.Length).ThrowIfFailed();
        }

        GC.KeepAlive(graphics);
        GC.KeepAlive(pen);
    }

    /// <summary>
    ///  Fills the interior of an ellipse defined by integer bounds.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TBrush">The brush wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="bounds">The bounding rectangle for the ellipse.</param>
    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, Drawing.Rectangle bounds)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, (float)bounds.X, bounds.Y, bounds.Width, bounds.Height);

    /// <summary>
    ///  Fills the interior of an ellipse defined by floating-point bounds.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TBrush">The brush wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="bounds">The bounding rectangle for the ellipse.</param>
    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, Drawing.RectangleF bounds)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

    /// <summary>
    ///  Fills the interior of an ellipse defined by integer coordinates.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TBrush">The brush wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle.</param>
    /// <param name="width">The width of the bounding rectangle.</param>
    /// <param name="height">The height of the bounding rectangle.</param>
    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, int x, int y, int width, int height)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
        => FillEllipse(graphics, brush, (float)x, y, width, height);

    /// <summary>
    ///  Fills the interior of an ellipse defined by floating-point coordinates.
    /// </summary>
    /// <typeparam name="TGraphics">The graphics wrapper type.</typeparam>
    /// <typeparam name="TBrush">The brush wrapper type.</typeparam>
    /// <param name="graphics">The graphics object to draw with.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle.</param>
    /// <param name="width">The width of the bounding rectangle.</param>
    /// <param name="height">The height of the bounding rectangle.</param>
    /// <remarks>
    ///  <para>
    ///   Coordinates are interpreted in the current world coordinate space of <paramref name="graphics"/>.
    ///  </para>
    /// </remarks>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public static void FillEllipse<TGraphics, TBrush>(this TGraphics graphics, TBrush brush, float x, float y, float width, float height)
        where TGraphics : IPointer<GpGraphics>
        where TBrush : IPointer<GpBrush>
    {
        PInvoke.GdipFillEllipse(graphics.Pointer, brush.Pointer, x, y, width, height).ThrowIfFailed();
        GC.KeepAlive(graphics);
        GC.KeepAlive(brush);
    }
}