// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Win32.Graphics.Direct2D.Common;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe partial struct ID2D1RenderTarget
{
    public D2D_SIZE_F GetSizeHack()
    {
        return ((delegate* unmanaged[Stdcall, MemberFunction]<ID2D1RenderTarget*, D2D_SIZE_F>)lpVtbl[53])((ID2D1RenderTarget*)Unsafe.AsPointer(ref this));
    }
}