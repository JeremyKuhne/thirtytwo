// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1Brush"/>.
/// </summary>
public unsafe class Brush : Resource, IPointer<ID2D1Brush>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1Brush"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1Brush* Pointer => (ID2D1Brush*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1Brush"/> pointer.
    /// </summary>
    /// <param name="brush">The native brush pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public Brush(ID2D1Brush* brush) : base((ID2D1Resource*)brush)
    {
    }

    /// <summary>
    ///  Converts a <see cref="Brush"/> wrapper to its underlying <see cref="ID2D1Brush"/> pointer.
    /// </summary>
    /// <param name="brush">The wrapper instance.</param>
    /// <returns>The wrapped native brush pointer.</returns>
    public static implicit operator ID2D1Brush*(Brush brush) => brush.Pointer;
}