// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Direct2D;

public unsafe class Factory : DisposableBase.Finalizable, IPointer<ID2D1Factory>
{
    private readonly AgileComPointer<ID2D1Factory> _factory;

    public unsafe ID2D1Factory* Pointer { get; private set; }

    public Factory(
        D2D1_FACTORY_TYPE factoryType = D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED,
        D2D1_DEBUG_LEVEL factoryOptions = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_NONE)
    {
        ID2D1Factory* factory;
        Interop.D2D1CreateFactory(
            factoryType,
            IID.Get<ID2D1Factory>(),
            (D2D1_FACTORY_OPTIONS*)&factoryOptions,
            (void**)&factory).ThrowOnFailure();

        Pointer = factory;

        // Ensure that this can be disposed on the finalizer thread by giving the "last" ref count
        // to an agile pointer.
        _factory = new AgileComPointer<ID2D1Factory>(factory, takeOwnership: true);
    }

    protected override void Dispose(bool disposing)
    {
        Pointer = null;

        if (disposing)
        {
            _factory.Dispose();
        }
    }
}