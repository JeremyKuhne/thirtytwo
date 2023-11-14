// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public readonly struct PaddingF(float left, float top, float right, float bottom)
{
    public readonly float Left = left;
    public readonly float Top = top;
    public readonly float Right = right;
    public readonly float Bottom = bottom;

    public static implicit operator PaddingF(float padding) => new(padding, padding, padding, padding);
    public static implicit operator PaddingF((float Left, float Top, float Right, float Bottom) padding)
        => new(padding.Left, padding.Top, padding.Right, padding.Bottom);
}