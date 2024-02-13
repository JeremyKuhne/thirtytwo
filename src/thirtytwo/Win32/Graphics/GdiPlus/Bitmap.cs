// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Graphics.GdiPlus;

public unsafe class Bitmap : Image, IPointer<GpBitmap>
{
    public unsafe new GpBitmap* Pointer => (GpBitmap*)base.Pointer;

    public Bitmap(GpBitmap* bitmap) : base((GpImage*)bitmap) { }
    public Bitmap(string filename) : this(Create(filename)) { }

    private static GpBitmap* Create(string filename)
    {
        ArgumentNullException.ThrowIfNull(filename);
        GdiPlus.Init();

        fixed (char* fn = filename)
        {
            GpBitmap* bitmap;
            Interop.GdipCreateBitmapFromFile(fn, &bitmap).ThrowIfFailed();
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
    public void LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, ref BitmapData data)
    {
        // LockBits always creates a temporary copy of the data.
        Interop.GdipBitmapLockBits(
            Pointer,
            (Rect*)&rect,
            (uint)flags,
            (int)format,
            (BitmapData*)Unsafe.AsPointer(ref data)).ThrowIfFailed();

        GC.KeepAlive(this);
    }

    public void UnlockBits(ref BitmapData data)
    {
        Interop.GdipBitmapUnlockBits(Pointer, (BitmapData*)Unsafe.AsPointer(ref data)).ThrowIfFailed();
        GC.KeepAlive(this);
    }

    public static implicit operator GpBitmap*(Bitmap bitmap) => bitmap.Pointer;
}