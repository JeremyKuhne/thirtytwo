// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  WIC Bitmap Decoder.
/// </summary>
public unsafe class BitmapDecoder : DirectDrawBase<IWICBitmapDecoder>
{
    public BitmapDecoder(IWICBitmapDecoder* pointer) : base(pointer) { }

    public BitmapDecoder(string filename, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        : base(CreateDecoderFromFilename(Application.ImagingFactory, filename, metadataOptions))
    {
    }

    public static IWICBitmapDecoder* CreateDecoderFromFilename(
        ImagingFactory factory,
        string filename,
        WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        IWICBitmapDecoder* decoder;
        factory.Pointer->CreateDecoderFromFilename(
            filename,
            null,
            GENERIC_ACCESS_RIGHTS.GENERIC_READ,
            metadataOptions,
            &decoder).ThrowOnFailure();

        GC.KeepAlive(factory);
        return decoder;
    }

    public BitmapFrameDecode GetFrame(uint index)
    {
        IWICBitmapFrameDecode* frame;
        Pointer->GetFrame(index, &frame).ThrowOnFailure();
        GC.KeepAlive(this);
        return new BitmapFrameDecode(frame);
    }

    public static implicit operator IWICBitmapDecoder*(BitmapDecoder d) => d.Pointer;
}