// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;
using Windows.Win32.System.Com;

namespace Windows.Win32.Graphics.DirectWrite;

public unsafe class TextLayout : DisposableBase.Finalizable, IPointer<IDWriteTextLayout>
{
    private readonly AgileComPointer<IDWriteTextLayout> _layout;

    public unsafe IDWriteTextLayout* Pointer { get; private set; }

    public TextLayout(IDWriteTextLayout* layout)
    {
        Pointer = layout;

        // Ensure that this can be disposed on the finalizer thread by giving the "last" ref count
        // to an agile pointer.
        _layout = new AgileComPointer<IDWriteTextLayout>(layout, takeOwnership: true);
    }

    public TextLayout(
        string text,
        TextFormat format,
        SizeF maxSize) : this(Create(text, format, maxSize.Width, maxSize.Height))
    {
    }

    public TextLayout(
        string text,
        TextFormat format,
        float maxWidth,
        float maxHeight) : this(Create(text, format, maxWidth, maxHeight))
    {
    }

    private static IDWriteTextLayout* Create(
        string text,
        TextFormat format,
        float maxWidth,
        float maxHeight)
    {
        IDWriteTextLayout* layout;

        fixed (char* t = text)
        {
            Application.DirectWriteFactory.Pointer->CreateTextLayout(
                text,
                (uint)text.Length,
                format.Pointer,
                maxWidth,
                maxHeight,
                &layout).ThrowOnFailure();
        }

        return layout;
    }

    public void SetFontSize(float fontSize, TextRange textRange)
    {
        Pointer->SetFontSize(fontSize, textRange);
        GC.KeepAlive(this);
    }

    public float MaxWidth
    {
        get
        {
            float maxWidth = Pointer->GetMaxWidth();
            GC.KeepAlive(this);
            return maxWidth;
        }
        set
        {
            Pointer->SetMaxWidth(value);
            GC.KeepAlive(this);
        }
    }

    public float MaxHeight
    {
        get
        {
            float maxHeight = Pointer->GetMaxHeight();
            GC.KeepAlive(this);
            return maxHeight;
        }
        set
        {
            Pointer->SetMaxHeight(value);
            GC.KeepAlive(this);
        }
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
            Pointer->SetTextAlignment((DWRITE_TEXT_ALIGNMENT)value);
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
            Pointer->SetParagraphAlignment((DWRITE_PARAGRAPH_ALIGNMENT)value);
            GC.KeepAlive(this);
        }
    }

    public void SetTypography<T>(T typography, TextRange textRange) where T : IPointer<IDWriteTypography>
    {
        Pointer->SetTypography(typography.Pointer, textRange);
        GC.KeepAlive(this);
        GC.KeepAlive(typography);
    }

    public void SetUnderline(bool hasUnderline, TextRange textRange)
    {
        Pointer->SetUnderline(hasUnderline, textRange);
        GC.KeepAlive(this);
    }

    public void SetFontWeight(FontWeight fontWeight, TextRange textRange)
    {
        Pointer->SetFontWeight((DWRITE_FONT_WEIGHT)fontWeight, textRange);
        GC.KeepAlive(this);
    }

    protected override void Dispose(bool disposing)
    {
        Pointer = null;

        if (disposing)
        {
            _layout.Dispose();
        }
    }
}