// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICBitmapSource</c>, the common WIC source interface for pixel-producing objects.
/// </summary>
/// <remarks>
///  WIC source dimensions are expressed in pixels. Stride and pixel-buffer lengths used by native copy operations
///  are expressed in bytes. This wrapper owns one COM reference to the wrapped source and releases it when disposed.
/// </remarks>
public unsafe class BitmapSource : DirectDrawBase<IWICBitmapSource>
{
    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICBitmapSource*</c>.
    /// </summary>
    /// <param name="pointer">Native source pointer. The wrapper takes ownership of one existing COM reference.</param>
    public BitmapSource(IWICBitmapSource* pointer) : base(pointer) { }

    /// <summary>
    ///  Converts a <see cref="BitmapSource"/> wrapper to its underlying <c>IWICBitmapSource*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICBitmapSource*</c> pointer.</returns>
    public static implicit operator IWICBitmapSource*(BitmapSource d) => d.Pointer;
}