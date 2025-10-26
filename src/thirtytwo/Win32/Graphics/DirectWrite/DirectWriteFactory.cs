// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Factory that is used to create DirectWrite resources. Use the <see cref="Application.DirectWriteFactory"/>
///  instance unless you need a custom factory.
/// </summary>
/// <inheritdoc cref="Interop.DWriteCreateFactory(DWRITE_FACTORY_TYPE, Guid*, void**)"/>
public unsafe sealed class DirectWriteFactory : DirectDrawBase<IDWriteFactory>
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