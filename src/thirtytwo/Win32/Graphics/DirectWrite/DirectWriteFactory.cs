// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class DirectWriteFactory : DisposableBase.Finalizable, IPointer<IDWriteFactory>
{
    private readonly AgileComPointer<IDWriteFactory> _factory;

    public unsafe IDWriteFactory* Pointer { get; private set; }

    public DirectWriteFactory(DWRITE_FACTORY_TYPE factoryType = DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED)
    {
        IDWriteFactory* factory;
        Interop.DWriteCreateFactory(
            factoryType,
            IID.Get<IDWriteFactory>(),
            (void**)&factory).ThrowOnFailure();

        Pointer = factory;

        // Ensure that this can be disposed on the finalizer thread by giving the "last" ref count
        // to an agile pointer.
        _factory = new AgileComPointer<IDWriteFactory>(factory, takeOwnership: true);
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