// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

using Windows.Support;
using Windows.Win32.Graphics.Direct2D.Common;

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1SolidColorBrush"/>.
/// </summary>
public unsafe class SolidColorBrush : Brush, IPointer<ID2D1SolidColorBrush>
{
    /// <summary>
    ///  Gets the wrapped native <see cref="ID2D1SolidColorBrush"/> pointer.
    /// </summary>
    /// <returns>The native interface pointer represented by this wrapper.</returns>
    public new ID2D1SolidColorBrush* Pointer => (ID2D1SolidColorBrush*)base.Pointer;

    /// <summary>
    ///  Initializes a wrapper around an existing <see cref="ID2D1SolidColorBrush"/> pointer.
    /// </summary>
    /// <param name="brush">The native solid-color brush pointer to wrap.</param>
    /// <remarks>
    ///  This constructor does not call <c>AddRef</c>; disposal of this wrapper releases the wrapped COM pointer.
    /// </remarks>
    public SolidColorBrush(ID2D1SolidColorBrush* brush) : base((ID2D1Brush*)brush)
    {
    }

    /// <summary>
    ///  Converts a <see cref="SolidColorBrush"/> wrapper to its underlying <see cref="ID2D1SolidColorBrush"/> pointer.
    /// </summary>
    /// <param name="brush">The wrapper instance.</param>
    /// <returns>The wrapped native brush pointer.</returns>
    public static implicit operator ID2D1SolidColorBrush*(SolidColorBrush brush) => brush.Pointer;

    /// <summary>
    ///  Gets or sets the brush color.
    /// </summary>
    /// <value>The color value converted to or from <see cref="D2D1_COLOR_F"/>.</value>
    public Color Color
    {
        get
        {
            D2D1_COLOR_F color = Pointer->GetColorHack();
            return (Color)color;
        }
        set
        {
            D2D1_COLOR_F color = (D2D1_COLOR_F)value;
            Pointer->SetColor(&color);
        }
    }
}