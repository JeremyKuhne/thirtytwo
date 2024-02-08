﻿// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class SolidColorBrush : Brush, IPointer<ID2D1SolidColorBrush>
{
    public unsafe new ID2D1SolidColorBrush* Pointer => (ID2D1SolidColorBrush*)base.Pointer;

    public SolidColorBrush(ID2D1SolidColorBrush* brush) : base((ID2D1Brush*)brush)
    {
    }

    public static implicit operator ID2D1SolidColorBrush*(SolidColorBrush brush) => brush.Pointer;
}