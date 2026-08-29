// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICBitmap</c> instance.
/// </summary>
/// <remarks>This type owns one COM reference to the supplied pointer and releases it when disposed.</remarks>
public unsafe class Bitmap : BitmapSource, IPointer<IWICBitmap>
{
    /// <summary>
    ///  Gets the wrapped <c>IWICBitmap*</c> pointer.
    /// </summary>
    /// <value>The native <c>IWICBitmap*</c> represented by this wrapper.</value>
    public new IWICBitmap* Pointer => (IWICBitmap*)base.Pointer;

    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICBitmap*</c>.
    /// </summary>
    /// <param name="bitmap">Native bitmap pointer. The wrapper takes ownership of one existing COM reference.</param>
    public Bitmap(IWICBitmap* bitmap) : base((IWICBitmapSource*)bitmap)
    {
    }

    /// <summary>
    ///  Converts a <see cref="Bitmap"/> wrapper to its underlying <c>IWICBitmap*</c>.
    /// </summary>
    /// <param name="bitmap">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICBitmap*</c> pointer.</returns>
    public static implicit operator IWICBitmap*(Bitmap bitmap) => bitmap.Pointer;
}