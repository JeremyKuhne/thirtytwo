// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class DirectWriteFactory : DirectDrawBase<IDWriteFactory>
{
    public DirectWriteFactory(DWRITE_FACTORY_TYPE factoryType = DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED)
        : base(Create(factoryType))
    {
    }

    private static IDWriteFactory* Create(DWRITE_FACTORY_TYPE factoryType)
    {
        IDWriteFactory* factory;
        Interop.DWriteCreateFactory(
            factoryType,
            IID.Get<IDWriteFactory>(),
            (void**)&factory).ThrowOnFailure();

        return factory;
    }
}