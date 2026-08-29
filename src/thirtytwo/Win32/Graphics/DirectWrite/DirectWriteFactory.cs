// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Factory that is used to create DirectWrite resources. Use the <see cref="Application.DirectWriteFactory"/>
///  instance unless you need a custom factory.
/// </summary>
/// <remarks>
///  Instances own the underlying <see cref="IDWriteFactory"/> COM interface pointer and release it when disposed.
/// </remarks>
/// <inheritdoc cref="Interop.DWriteCreateFactory(DWRITE_FACTORY_TYPE, Guid*, void**)"/>
public unsafe sealed class DirectWriteFactory : DirectDrawBase<IDWriteFactory>
{
    /// <summary>
    ///  Creates a DirectWrite factory wrapper.
    /// </summary>
    /// <param name="factoryType">The factory threading model to request.</param>
    /// <remarks>
    ///  This constructor calls <see cref="PInvoke.DWriteCreateFactory(DWRITE_FACTORY_TYPE, Guid*, void**)"/> and throws
    ///  when the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public DirectWriteFactory(DWRITE_FACTORY_TYPE factoryType = DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED)
        : base(Create(factoryType))
    {
    }

    private static IDWriteFactory* Create(DWRITE_FACTORY_TYPE factoryType)
    {
        IDWriteFactory* factory;
        PInvoke.DWriteCreateFactory(
            factoryType,
            IID.Get<IDWriteFactory>(),
            (void**)&factory).ThrowOnFailure();

        return factory;
    }
}