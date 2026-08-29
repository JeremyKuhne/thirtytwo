// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Represents integer padding values for the left, top, right, and bottom edges of a layout region.
/// </summary>
/// <param name="left">The left-edge padding in logical pixels.</param>
/// <param name="top">The top-edge padding in logical pixels.</param>
/// <param name="right">The right-edge padding in logical pixels.</param>
/// <param name="bottom">The bottom-edge padding in logical pixels.</param>
public readonly struct Padding(int left, int top, int right, int bottom)
{
    /// <summary>
    ///  The left-edge padding in logical pixels.
    /// </summary>
    public readonly int Left = left;

    /// <summary>
    ///  The top-edge padding in logical pixels.
    /// </summary>
    public readonly int Top = top;

    /// <summary>
    ///  The right-edge padding in logical pixels.
    /// </summary>
    public readonly int Right = right;

    /// <summary>
    ///  The bottom-edge padding in logical pixels.
    /// </summary>
    public readonly int Bottom = bottom;

    /// <summary>
    ///  Converts a single integer value to uniform padding on all four sides.
    /// </summary>
    /// <param name="padding">The value to apply to left, top, right, and bottom.</param>
    /// <returns>A <see cref="Padding"/> where all edges are set to <paramref name="padding"/>.</returns>
    public static implicit operator Padding(int padding) => new(padding, padding, padding, padding);

    /// <summary>
    ///  Converts a tuple of edge values to a <see cref="Padding"/> instance.
    /// </summary>
    /// <param name="padding">The tuple containing left, top, right, and bottom values.</param>
    /// <returns>A <see cref="Padding"/> initialized from the tuple values.</returns>
    public static implicit operator Padding((int Left, int Top, int Right, int Bottom) padding)
        => new(padding.Left, padding.Top, padding.Right, padding.Bottom);
}