// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1Image"/>.
/// </summary>
public unsafe class Image : Resource, IPointer<ID2D1Image>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1Image"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1Image* Pointer => (ID2D1Image*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1Image"/> pointer.
    /// </summary>
    /// <param name="image">The native image pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public Image(ID2D1Image* image) : base((ID2D1Resource*)image)
    {
    }

    /// <summary>
    ///  Converts an <see cref="Image"/> wrapper to its underlying <see cref="ID2D1Image"/> pointer.
    /// </summary>
    /// <param name="image">The wrapper instance.</param>
    /// <returns>The wrapped native image pointer.</returns>
    public static implicit operator ID2D1Image*(Image image) => image.Pointer;
}