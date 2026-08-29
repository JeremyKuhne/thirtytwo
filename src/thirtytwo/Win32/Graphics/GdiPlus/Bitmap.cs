// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

/// <summary>
///  Wraps a native GDI+ bitmap object.
/// </summary>
public unsafe class Bitmap : Image, IPointer<GpBitmap>
{
    /// <summary>
    ///  Gets the underlying native bitmap pointer.
    /// </summary>
    public new GpBitmap* Pointer => (GpBitmap*)base.Pointer;

    /// <summary>
    ///  Initializes a bitmap wrapper from an existing native pointer.
    /// </summary>
    /// <param name="bitmap">The native GDI+ bitmap pointer to wrap.</param>
    /// <remarks>
    ///  <para>
    ///   The wrapper assumes ownership of the native object and releases it when disposed.
    ///  </para>
    /// </remarks>
    public Bitmap(GpBitmap* bitmap) : base((GpImage*)bitmap) { }

    /// <summary>
    ///  Creates a bitmap from a file.
    /// </summary>
    /// <param name="filename">The path to the image file to load.</param>
    /// <exception cref="ArgumentNullException"><paramref name="filename"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public Bitmap(string filename) : this(Create(filename)) { }

    private static GpBitmap* Create(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);
        GdiPlus.Init();

        fixed (char* fn = filename)
        {
            GpBitmap* bitmap;
            PInvoke.GdipCreateBitmapFromFile(fn, &bitmap).ThrowIfFailed();
            return bitmap;
        }
    }

    /// <summary>
    ///  Locks a rectangular portion of this bitmap and provides a temporary buffer that you can use to read or write
    ///  pixel data in a specified format. Any pixel data that you write to the buffer is copied to the
    ///  <see cref="Bitmap"/> object when you call <see cref="UnlockBits(ref BitmapData)"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/windows/win32/api/gdiplusheaders/nf-gdiplusheaders-bitmap-lockbits">
    ///   </see>
    ///  </para>
    /// </remarks>
    /// <param name="rect">The rectangle, in pixels, to lock.</param>
    /// <param name="flags">The read or write lock mode.</param>
    /// <param name="format">The pixel format for the temporary buffer.</param>
    /// <param name="data">On success, receives information about the locked buffer.</param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public void LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, ref BitmapData data)
    {
        // LockBits always creates a temporary copy of the data.
        PInvoke.GdipBitmapLockBits(
            Pointer,
            (Rect*)&rect,
            (uint)flags,
            (int)format,
            (BitmapData*)Unsafe.AsPointer(ref data)).ThrowIfFailed();

        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Unlocks a region previously locked by <see cref="LockBits(Rectangle, ImageLockMode, PixelFormat, ref BitmapData)"/>.
    /// </summary>
    /// <param name="data">
    ///  The lock data previously returned by <see cref="LockBits(Rectangle, ImageLockMode, PixelFormat, ref BitmapData)"/>.
    /// </param>
    /// <exception cref="Exception">The underlying GDI+ operation failed.</exception>
    public void UnlockBits(ref BitmapData data)
    {
        PInvoke.GdipBitmapUnlockBits(Pointer, (BitmapData*)Unsafe.AsPointer(ref data)).ThrowIfFailed();
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Gets the native bitmap pointer for a wrapper instance.
    /// </summary>
    /// <param name="bitmap">The bitmap wrapper.</param>
    /// <returns>The native GDI+ bitmap pointer.</returns>
    public static implicit operator GpBitmap*(Bitmap bitmap) => bitmap.Pointer;
}