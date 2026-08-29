// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1Bitmap"/>.
/// </summary>
/// <remarks>Represents a Direct2D bitmap resource that can be used as an image source by a render target.</remarks>
public unsafe class Bitmap : Image, IPointer<ID2D1Bitmap>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1Bitmap"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1Bitmap* Pointer => (ID2D1Bitmap*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1Bitmap"/> pointer.
    /// </summary>
    /// <param name="bitmap">The native bitmap pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public Bitmap(ID2D1Bitmap* bitmap) : base((ID2D1Image*)bitmap)
    {
    }

    /// <summary>
    ///  Converts a <see cref="Bitmap"/> wrapper to its underlying <see cref="ID2D1Bitmap"/> pointer.
    /// </summary>
    /// <param name="bitmap">The wrapper instance.</param>
    /// <returns>The wrapped native bitmap pointer.</returns>
    public static implicit operator ID2D1Bitmap*(Bitmap bitmap) => bitmap.Pointer;
}