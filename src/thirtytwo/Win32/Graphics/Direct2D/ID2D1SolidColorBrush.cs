// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.Graphics.Direct2D.Common;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe partial struct ID2D1SolidColorBrush
{
    public D2D1_COLOR_F GetColorHack()
    {
        return ((delegate* unmanaged[Stdcall, MemberFunction]<ID2D1SolidColorBrush*, D2D1_COLOR_F>)lpVtbl[9])((ID2D1SolidColorBrush*)Unsafe.AsPointer(ref this));
    }
}