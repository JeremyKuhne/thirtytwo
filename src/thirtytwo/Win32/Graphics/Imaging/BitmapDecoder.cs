// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICBitmapDecoder</c> used to decode image containers through WIC.
/// </summary>
/// <remarks>This type owns one COM reference to the wrapped decoder and releases it when disposed.</remarks>
public unsafe class BitmapDecoder : DirectDrawBase<IWICBitmapDecoder>
{
    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICBitmapDecoder*</c>.
    /// </summary>
    /// <param name="pointer">Native decoder pointer. The wrapper takes ownership of one existing COM reference.</param>
    public BitmapDecoder(IWICBitmapDecoder* pointer) : base(pointer) { }

    /// <summary>
    ///  Opens a bitmap decoder for a file.
    /// </summary>
    /// <param name="filename">Path to the image container file.</param>
    /// <param name="metadataOptions">Metadata cache behavior used by WIC while decoding.</param>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public BitmapDecoder(string filename, WICDecodeOptions metadataOptions = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        : base(CreateDecoderFromFilename(Application.ImagingFactory, filename, metadataOptions))
    {
    }

    /// <summary>
    ///  Creates an <c>IWICBitmapDecoder*</c> for the specified file.
    /// </summary>
    /// <param name="factory">Imaging factory used to create the decoder.</param>
    /// <param name="filename">Path to the image container file.</param>
    /// <param name="metadataOptions">Metadata cache behavior used by WIC while decoding.</param>
    /// <returns>A decoder pointer with one COM reference owned by the caller.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
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

    /// <summary>
    ///  Gets a decoded frame by zero-based index.
    /// </summary>
    /// <param name="index">Zero-based frame index in the container.</param>
    /// <returns>A <see cref="BitmapFrameDecode"/> that owns one COM reference to the requested frame.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public BitmapFrameDecode GetFrame(uint index)
    {
        IWICBitmapFrameDecode* frame;
        Pointer->GetFrame(index, &frame).ThrowOnFailure();
        GC.KeepAlive(this);
        return new BitmapFrameDecode(frame);
    }

    /// <summary>
    ///  Converts a <see cref="BitmapDecoder"/> wrapper to its underlying <c>IWICBitmapDecoder*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICBitmapDecoder*</c> pointer.</returns>
    public static implicit operator IWICBitmapDecoder*(BitmapDecoder d) => d.Pointer;
}