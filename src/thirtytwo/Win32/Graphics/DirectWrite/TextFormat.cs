// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Wraps <see cref="IDWriteTextFormat"/> and exposes commonly used text-format settings.
/// </summary>
/// <remarks>
///  Instances own the underlying <see cref="IDWriteTextFormat"/> COM interface pointer and release it when disposed.
///  Property setters in this type throw when the corresponding DirectWrite API returns a failing <c>HRESULT</c>.
/// </remarks>
public unsafe class TextFormat : DirectDrawBase<IDWriteTextFormat>
{
    /// <summary>
    ///  Wraps an existing DirectWrite text format pointer.
    /// </summary>
    /// <param name="format">The existing text format interface pointer to wrap.</param>
    /// <remarks>The wrapper takes responsibility for releasing this COM interface pointer when disposed.</remarks>
    public TextFormat(IDWriteTextFormat* format) : base(format)
    {
    }

    /// <summary>
    ///  Creates a text format from DirectWrite font characteristics.
    /// </summary>
    /// <param name="fontFamilyName">The font family name.</param>
    /// <param name="fontSize">The font size in DIPs.</param>
    /// <param name="fontWeight">The weight of the font.</param>
    /// <param name="fontStyle">The style of the font.</param>
    /// <param name="fontStretch">The stretch of the font.</param>
    /// <param name="localeName">The locale name used for font fallback and shaping.</param>
    /// <remarks>
    ///  This constructor calls <see cref="IDWriteFactory.CreateTextFormat(char*, IDWriteFontCollection*, DWRITE_FONT_WEIGHT, DWRITE_FONT_STYLE, DWRITE_FONT_STRETCH, float, char*, IDWriteTextFormat**)"/>
    ///  and throws when the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public TextFormat(
        string fontFamilyName,
        float fontSize,
        FontWeight fontWeight = FontWeight.Normal,
        FontStyle fontStyle = FontStyle.Normal,
        FontStretch fontStretch = FontStretch.Normal,
        string localeName = "en-us") : this(Create(fontFamilyName, fontSize, fontWeight, fontStyle, fontStretch, localeName))
    {
    }

    /// <summary>
    ///  Creates a text format from a GDI logical font description.
    /// </summary>
    /// <param name="logfont">
    ///  The logical font values used to resolve the DirectWrite font family, style, and size.
    /// </param>
    /// <remarks>
    ///  This constructor uses DirectWrite and GDI interop APIs and throws when those APIs return a failing
    ///  <c>HRESULT</c>.
    /// </remarks>
    public TextFormat(in LOGFONTW logfont) : this(Create(logfont))
    {
    }

    /// <summary>
    ///  Create a <see cref="TextFormat"/> from a GDI font and format.
    /// </summary>
    /// <param name="hfont">The GDI font handle used to obtain a logical font.</param>
    /// <param name="format">The GDI draw-text format flags used to map alignment and wrapping settings.</param>
    /// <remarks>
    ///  <para>
    ///   The mapping of options is only partially implmenented.
    ///  </para>
    ///  <para>
    ///   The underlying font conversion uses DirectWrite and GDI interop APIs and throws when those APIs return a
    ///   failing <c>HRESULT</c>.
    ///  </para>
    /// </remarks>
    public TextFormat(HFONT hfont, DrawTextFormat format) : this(Create(hfont.GetLogicalFont()))
    {
        bool rtl = format.HasFlag(DrawTextFormat.RightToLeftReading);
        TextAlignment = format.HasFlag(DrawTextFormat.Center)
            ? TextAlignment.Center
            : format.HasFlag(DrawTextFormat.Right)
                ? rtl ? TextAlignment.Leading : TextAlignment.Trailing
                : rtl ? TextAlignment.Trailing : TextAlignment.Leading;

        if (format.HasFlag(DrawTextFormat.SingleLine))
        {
            ParagraphAlignment = format.HasFlag(DrawTextFormat.VerticallyCenter)
                ? ParagraphAlignment.Center
                : format.HasFlag(DrawTextFormat.Bottom)
                    ? ParagraphAlignment.Far
                    : ParagraphAlignment.Near;

            WordWrapping = WordWrapping.NoWrap;
        }
    }

    private static IDWriteTextFormat* Create(in LOGFONTW logfont)
    {
        string locale = Application.GetUserDefaultLocaleName();

        using ComScope<IDWriteFont> font = new(null);
        fixed (LOGFONTW* lf = &logfont)
        {
            Application.DirectWriteGdiInterop.Pointer->CreateFontFromLOGFONT(lf, font).ThrowOnFailure();
        }

        using ComScope<IDWriteFontFamily> family = new(null);
        font.Pointer->GetFontFamily(family).ThrowOnFailure();

        using ComScope<IDWriteLocalizedStrings> strings = new(null);
        family.Pointer->GetFamilyNames(strings).ThrowOnFailure();

        strings.Pointer->FindLocaleName(locale, out uint nameIndex, out BOOL exists).ThrowOnFailure();

        if (!exists)
        {
            locale = "en-us";
            strings.Pointer->FindLocaleName(locale, out nameIndex, out exists).ThrowOnFailure();
        }

        if (!exists)
        {
            nameIndex = 0;
        }

        strings.Pointer->GetStringLength(nameIndex, out uint length).ThrowOnFailure();

        // Add one for the null terminator.
        length++;

        Span<char> name = stackalloc char[(int)length];
        fixed (char* n = name)
        {
            strings.Pointer->GetString(nameIndex, n, length).ThrowOnFailure();
        }

        float fontSize = 0;
        int height = logfont.lfHeight;
        if (height < 0)
        {
            // Negative height is em size.
            fontSize = -height;
        }
        else if (height > 0)
        {
            // Cell height.
            DWRITE_FONT_METRICS metrics = default;
            font.Pointer->GetMetrics(&metrics);
            float cellHeight = (metrics.ascent + metrics.descent) / (float)metrics.designUnitsPerEm;
            fontSize = height / cellHeight;
        }

        return Create(
            name,
            fontSize,
            (FontWeight)font.Pointer->GetWeight(),
            (FontStyle)font.Pointer->GetStyle(),
            (FontStretch)font.Pointer->GetStretch(),
            locale);
    }

    private static IDWriteTextFormat* Create(
        ReadOnlySpan<char> fontFamilyName,
        float fontSize,
        FontWeight fontWeight,
        FontStyle fontStyle,
        FontStretch fontStretch,
        string localeName)
    {
        IDWriteTextFormat* format;

        fixed (char* fn = fontFamilyName)
        fixed (char* ln = localeName)
        {
            Application.DirectWriteFactory.Pointer->CreateTextFormat(
                fn,
                null,
                (DWRITE_FONT_WEIGHT)fontWeight,
                (DWRITE_FONT_STYLE)fontStyle,
                (DWRITE_FONT_STRETCH)fontStretch,
                fontSize,
                ln,
                &format).ThrowOnFailure();
        }

        return format;
    }

    /// <summary>
    ///  Gets or sets paragraph-line text alignment.
    /// </summary>
    /// <value>The horizontal text alignment setting.</value>
    public TextAlignment TextAlignment
    {
        get
        {
            TextAlignment alignment = (TextAlignment)Pointer->GetTextAlignment();
            GC.KeepAlive(this);
            return alignment;
        }
        set
        {
            Pointer->SetTextAlignment((DWRITE_TEXT_ALIGNMENT)value).ThrowOnFailure();
            GC.KeepAlive(this);
        }
    }

    /// <summary>
    ///  Gets or sets alignment of each paragraph relative to the layout box height.
    /// </summary>
    /// <value>The vertical paragraph alignment setting.</value>
    public ParagraphAlignment ParagraphAlignment
    {
        get
        {
            ParagraphAlignment alignment = (ParagraphAlignment)Pointer->GetParagraphAlignment();
            GC.KeepAlive(this);
            return alignment;
        }
        set
        {
            Pointer->SetParagraphAlignment((DWRITE_PARAGRAPH_ALIGNMENT)value).ThrowOnFailure();
            GC.KeepAlive(this);
        }
    }

    /// <summary>
    ///  Gets or sets word-wrapping behavior.
    /// </summary>
    /// <value>The wrapping mode used during text layout.</value>
    public WordWrapping WordWrapping
    {
        get
        {
            WordWrapping wrapping = (WordWrapping)Pointer->GetWordWrapping();
            GC.KeepAlive(this);
            return wrapping;
        }
        set
        {
            Pointer->SetWordWrapping((DWRITE_WORD_WRAPPING)value).ThrowOnFailure();
            GC.KeepAlive(this);
        }
    }
}