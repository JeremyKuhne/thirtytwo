// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Resource : DirectDrawBase<ID2D1Resource>, IPointer<ID2D1Resource>
{
    public Resource(ID2D1Resource* resource) : base(resource)
    {
    }
}