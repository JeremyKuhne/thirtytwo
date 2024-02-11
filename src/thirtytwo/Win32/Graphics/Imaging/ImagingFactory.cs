// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Graphics.Imaging.D2D;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.Imaging;

public unsafe class ImagingFactory : DirectDrawBase<IWICImagingFactory2>
{
    public ImagingFactory() : base(Create()) { }

    public ImagingFactory(IWICImagingFactory2* pointer) : base(pointer) { }

    private static IWICImagingFactory2* Create()
    {
        Interop.CoCreateInstance(
            Interop.CLSID_WICImagingFactory2,
            null,
            CLSCTX.CLSCTX_INPROC_SERVER,
            out IWICImagingFactory2* factory).ThrowOnFailure();

        return factory;
    }

    public static implicit operator IWICImagingFactory2*(ImagingFactory d) => d.Pointer;
}