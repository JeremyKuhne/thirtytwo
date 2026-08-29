// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICBitmapFrameDecode</c> for one decoded frame from a WIC container.
/// </summary>
/// <remarks>
///  Frame wrappers are typically obtained from <see cref="BitmapDecoder.GetFrame(uint)"/> using a zero-based index.
///  This type owns one COM reference to the wrapped frame and releases it when disposed.
/// </remarks>
public unsafe class BitmapFrameDecode : BitmapSource, IPointer<IWICBitmapFrameDecode>
{
    /// <summary>
    ///  Gets the wrapped <c>IWICBitmapFrameDecode*</c> pointer.
    /// </summary>
    /// <value>The native <c>IWICBitmapFrameDecode*</c> represented by this wrapper.</value>
    public new IWICBitmapFrameDecode* Pointer => (IWICBitmapFrameDecode*)base.Pointer;

    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICBitmapFrameDecode*</c>.
    /// </summary>
    /// <param name="pointer">Native frame pointer. The wrapper takes ownership of one existing COM reference.</param>
    public BitmapFrameDecode(IWICBitmapFrameDecode* pointer) : base((IWICBitmapSource*)pointer) { }

    /// <summary>
    ///  Converts a <see cref="BitmapFrameDecode"/> wrapper to its underlying <c>IWICBitmapFrameDecode*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICBitmapFrameDecode*</c> pointer.</returns>
    public static implicit operator IWICBitmapFrameDecode*(BitmapFrameDecode d) => d.Pointer;
}