// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Represents floating-point padding values for the left, top, right, and bottom edges of a layout region.
/// </summary>
/// <param name="left">The left-edge padding in logical units.</param>
/// <param name="top">The top-edge padding in logical units.</param>
/// <param name="right">The right-edge padding in logical units.</param>
/// <param name="bottom">The bottom-edge padding in logical units.</param>
public readonly struct PaddingF(float left, float top, float right, float bottom)
{
    /// <summary>
    ///  The left-edge padding in logical units.
    /// </summary>
    public readonly float Left = left;

    /// <summary>
    ///  The top-edge padding in logical units.
    /// </summary>
    public readonly float Top = top;

    /// <summary>
    ///  The right-edge padding in logical units.
    /// </summary>
    public readonly float Right = right;

    /// <summary>
    ///  The bottom-edge padding in logical units.
    /// </summary>
    public readonly float Bottom = bottom;

    /// <summary>
    ///  Converts a single floating-point value to uniform padding on all four sides.
    /// </summary>
    /// <param name="padding">The value to apply to left, top, right, and bottom.</param>
    /// <returns>A <see cref="PaddingF"/> where all edges are set to <paramref name="padding"/>.</returns>
    public static implicit operator PaddingF(float padding) => new(padding, padding, padding, padding);

    /// <summary>
    ///  Converts a tuple of edge values to a <see cref="PaddingF"/> instance.
    /// </summary>
    /// <param name="padding">The tuple containing left, top, right, and bottom values.</param>
    /// <returns>A <see cref="PaddingF"/> initialized from the tuple values.</returns>
    public static implicit operator PaddingF((float Left, float Top, float Right, float Bottom) padding)
        => new(padding.Left, padding.Top, padding.Right, padding.Bottom);
}