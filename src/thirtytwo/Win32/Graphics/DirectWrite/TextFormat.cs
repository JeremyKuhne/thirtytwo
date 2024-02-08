// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

    private static IDWriteTextFormat* Create(
        string fontFamilyName,
        float fontSize,
        FontWeight fontWeight,
        FontStyle fontStyle,
        FontStretch fontStretch,
        string localeName)
    {
        IDWriteTextFormat* format;

        fixed (char* fn = fontFamilyName)
        {
            Application.DirectWriteFactory.Pointer->CreateTextFormat(
                fontFamilyName,
                null,
                (DWRITE_FONT_WEIGHT)fontWeight,
                (DWRITE_FONT_STYLE)fontStyle,
                (DWRITE_FONT_STRETCH)fontStretch,
                fontSize,
                localeName,
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
}