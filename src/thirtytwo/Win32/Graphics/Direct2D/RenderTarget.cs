// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class RenderTarget : Resource, IPointer<ID2D1RenderTarget>
{
    public unsafe new ID2D1RenderTarget* Pointer => (ID2D1RenderTarget*)base.Pointer;

    public RenderTarget(ID2D1RenderTarget* renderTarget) : base((ID2D1Resource*)renderTarget)
    {
    }

    public static implicit operator ID2D1RenderTarget*(RenderTarget renderTarget) => renderTarget.Pointer;
}