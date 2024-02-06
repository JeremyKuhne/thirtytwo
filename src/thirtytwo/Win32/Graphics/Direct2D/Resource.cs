// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Resource : DisposableBase.Finalizable, IPointer<ID2D1Resource>
{
    private readonly AgileComPointer<ID2D1Resource> _resource;

    public unsafe ID2D1Resource* Pointer { get; private set; }

    public Resource(ID2D1Resource* resource)
    {
        Pointer = resource;

        // Ensure that this can be disposed on the finalizer thread by giving the "last" ref count
        // to an agile pointer.
        _resource = new AgileComPointer<ID2D1Resource>(resource, takeOwnership: true);
    }

    protected override void Dispose(bool disposing)
    {
        Pointer = null;

        if (disposing)
        {
            _resource.Dispose();
        }
    }
}