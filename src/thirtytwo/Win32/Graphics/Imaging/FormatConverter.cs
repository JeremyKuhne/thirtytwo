// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

public unsafe class FormatConverter : BitmapSource, IPointer<IWICFormatConverter>
{
    public new IWICFormatConverter* Pointer => (IWICFormatConverter*)base.Pointer;

    public FormatConverter(IWICFormatConverter* pointer) : base((IWICBitmapSource*)pointer) { }
    public FormatConverter() : this(Create()) { }
    public FormatConverter(BitmapSource source) : this(Create()) => Initialize(source);


    private static IWICFormatConverter* Create()
    {
        IWICFormatConverter* converter;
        Application.ImagingFactory.Pointer->CreateFormatConverter(&converter).ThrowOnFailure();
        return converter;
    }

    public void Initialize<T>(
        T source,
        Guid destinationFormat = default,
        WICBitmapDitherType dither = WICBitmapDitherType.WICBitmapDitherTypeNone,
        IWICPalette* palette = null,
        float alphaThresholdPercent = 0.0f,
        WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
        where T : IPointer<IWICBitmapSource>
    {
        if (destinationFormat == Guid.Empty)
        {
            destinationFormat = Interop.GUID_WICPixelFormat32bppPBGRA;
        }

        Pointer->Initialize(
            source.Pointer,
            &destinationFormat,
            dither,
            palette,
            alphaThresholdPercent,
            paletteTranslate).ThrowOnFailure();

        GC.KeepAlive(source);
    }

    public static implicit operator IWICFormatConverter*(FormatConverter d) => d.Pointer;
}