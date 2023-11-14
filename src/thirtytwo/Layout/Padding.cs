// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public readonly struct Padding(int left, int top, int right, int bottom)
{
    public readonly int Left = left;
    public readonly int Top = top;
    public readonly int Right = right;
    public readonly int Bottom = bottom;

    public static implicit operator Padding(int padding) => new(padding, padding, padding, padding);
    public static implicit operator Padding((int Left, int Top, int Right, int Bottom) padding)
        => new(padding.Left, padding.Top, padding.Right, padding.Bottom);
}