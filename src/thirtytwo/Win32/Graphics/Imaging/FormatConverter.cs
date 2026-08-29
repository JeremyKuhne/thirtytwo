// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows.Win32.Graphics.Imaging;

/// <summary>
///  Wraps an <c>IWICFormatConverter</c> used to project a source bitmap into a different WIC pixel format.
/// </summary>
/// <remarks>
///  Conversion changes pixel-format interpretation and conversion pipeline settings. Source pixel dimensions are
///  preserved. Stride and destination buffer sizes for later copy operations are byte counts derived from the
///  destination format and image width.
/// </remarks>
public unsafe class FormatConverter : BitmapSource, IPointer<IWICFormatConverter>
{
    /// <summary>
    ///  Gets the wrapped <c>IWICFormatConverter*</c> pointer.
    /// </summary>
    /// <value>The native <c>IWICFormatConverter*</c> represented by this wrapper.</value>
    public new IWICFormatConverter* Pointer => (IWICFormatConverter*)base.Pointer;

    /// <summary>
    ///  Initializes a new wrapper around an existing <c>IWICFormatConverter*</c>.
    /// </summary>
    /// <param name="pointer">
    ///  Native converter pointer. The wrapper takes ownership of one existing COM reference.
    /// </param>
    public FormatConverter(IWICFormatConverter* pointer) : base((IWICBitmapSource*)pointer) { }

    /// <summary>
    ///  Creates a new WIC format converter using <see cref="Application.ImagingFactory"/>.
    /// </summary>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public FormatConverter() : this(Create()) { }

    /// <summary>
    ///  Creates a converter and initializes it from the specified source using default conversion settings.
    /// </summary>
    /// <param name="source">Source bitmap interface to convert.</param>
    /// <exception cref="Exception">
    ///  A creation or initialization COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    public FormatConverter(BitmapSource source) : this(Create()) => Initialize(source);

    /// <summary>
    ///  Creates a native <c>IWICFormatConverter*</c> from <see cref="Application.ImagingFactory"/>.
    /// </summary>
    /// <returns>A converter pointer with one COM reference owned by the caller.</returns>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
    private static IWICFormatConverter* Create()
    {
        IWICFormatConverter* converter;
        Application.ImagingFactory.Pointer->CreateFormatConverter(&converter).ThrowOnFailure();
        return converter;
    }

    /// <summary>
    ///  Initializes this converter from a bitmap source.
    /// </summary>
    /// <typeparam name="T">Source wrapper type that provides an <c>IWICBitmapSource*</c> pointer.</typeparam>
    /// <param name="source">Source bitmap to convert.</param>
    /// <param name="destinationFormat">
    ///  Destination pixel format GUID. When empty, this method uses <c>GUID_WICPixelFormat32bppPBGRA</c>.
    /// </param>
    /// <param name="dither">Dithering strategy used during conversion.</param>
    /// <param name="palette">Optional palette pointer used for palette-based conversions.</param>
    /// <param name="alphaThresholdPercent">Alpha threshold percentage used by selected conversion modes.</param>
    /// <param name="paletteTranslate">Palette translation strategy.</param>
    /// <exception cref="Exception">
    ///  The underlying COM call fails and <c>ThrowOnFailure()</c> maps the HRESULT to a managed exception.
    /// </exception>
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
            destinationFormat = PInvoke.GUID_WICPixelFormat32bppPBGRA;
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

    /// <summary>
    ///  Converts a <see cref="FormatConverter"/> wrapper to its underlying <c>IWICFormatConverter*</c>.
    /// </summary>
    /// <param name="d">The wrapper to convert.</param>
    /// <returns>The underlying native <c>IWICFormatConverter*</c> pointer.</returns>
    public static implicit operator IWICFormatConverter*(FormatConverter d) => d.Pointer;
}