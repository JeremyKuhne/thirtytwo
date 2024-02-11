// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class TextFormat : DirectDrawBase<IDWriteTextFormat>
{
    public TextFormat(IDWriteTextFormat* format) : base(format)
    {
    }

    public TextFormat(
        string fontFamilyName,
        float fontSize,
        FontWeight fontWeight = FontWeight.Normal,
        FontStyle fontStyle = FontStyle.Normal,
        FontStretch fontStretch = FontStretch.Normal,
        string localeName = "en-us") : this(Create(fontFamilyName, fontSize, fontWeight, fontStyle, fontStretch, localeName))
    {
    }

    public TextFormat(in LOGFONTW logfont) : this(Create(logfont))
    {
    }

    /// <summary>
    ///  Create a <see cref="TextFormat"/> from a GDI font and format.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The mapping of options is only partially implmenented.
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