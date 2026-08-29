// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Graphics.Imaging.D2D;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICImagingFactory2</c> used to create WIC imaging components.
/// </summary>
/// <remarks>This type owns one COM reference to the wrapped factory and releases it when disposed.</remarks>
public unsafe class ImagingFactory : DirectDrawBase<IWICImagingFactory2>
{
    /// <summary>
    ///  Creates a new WIC imaging factory by calling <c>CoCreateInstance</c>.
    /// </summary>
    /// <exception cref="Exception">
    ///  The underlying COM activation call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public ImagingFactory() : base(Create()) { }

    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICImagingFactory2*</c>.
    /// </summary>
    /// <param name="pointer">Native factory pointer. The wrapper takes ownership of one existing COM reference.</param>
    public ImagingFactory(IWICImagingFactory2* pointer) : base(pointer) { }

    /// <summary>
    ///  Activates the WIC imaging factory COM class.
    /// </summary>
    /// <returns>A factory pointer with one COM reference owned by the caller.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM activation call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    private static IWICImagingFactory2* Create()
    {
        PInvoke.CoCreateInstance(
            PInvoke.CLSID_WICImagingFactory2,
            null,
            CLSCTX.CLSCTX_INPROC_SERVER,
            out IWICImagingFactory2* factory).ThrowOnFailure();

        return factory;
    }

    /// <summary>
    ///  Converts an <see cref="ImagingFactory"/> wrapper to its underlying <c>IWICImagingFactory2*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICImagingFactory2*</c> pointer.</returns>
    public static implicit operator IWICImagingFactory2*(ImagingFactory d) => d.Pointer;
}