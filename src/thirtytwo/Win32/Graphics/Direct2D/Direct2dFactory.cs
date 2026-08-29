// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Direct2D;

/// <summary>
///  Managed wrapper for <see cref="ID2D1Factory"/>.
/// </summary>
/// <remarks>
///  Wraps the factory created by <see cref="PInvoke.D2D1CreateFactory(D2D1_FACTORY_TYPE, Guid*, D2D1_FACTORY_OPTIONS*, void**)"/>.
/// </remarks>
public unsafe class Direct2dFactory : DirectDrawBase<ID2D1Factory>
{
    /// <summary>
    ///  Creates a Direct2D factory wrapper.
    /// </summary>
    /// <param name="factoryType">The threading model for the created factory.</param>
    /// <param name="factoryOptions">The Direct2D debug level used when creating the factory.</param>
    /// <remarks>
    ///  The native factory pointer returned from the create call is wrapped and released when this instance is disposed.
    ///  A failed native call results in exception flow from <c>ThrowOnFailure</c>.
    /// </remarks>
    public Direct2dFactory(
        D2D1_FACTORY_TYPE factoryType = D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_MULTI_THREADED,
        D2D1_DEBUG_LEVEL factoryOptions = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_NONE)
        : base(Create(factoryType, factoryOptions))
    {
    }

    private static ID2D1Factory* Create(D2D1_FACTORY_TYPE factoryType, D2D1_DEBUG_LEVEL factoryOptions)
    {
        ID2D1Factory* factory;
        PInvoke.D2D1CreateFactory(
            factoryType,
            IID.Get<ID2D1Factory>(),
            (D2D1_FACTORY_OPTIONS*)&factoryOptions,
            (void**)&factory).ThrowOnFailure();

        return factory;
    }
}