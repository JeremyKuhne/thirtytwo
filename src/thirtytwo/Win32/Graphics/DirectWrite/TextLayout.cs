// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows.Win32.Graphics.DirectWrite;

/// <summary>
///  Wraps <see cref="IDWriteTextLayout"/> for measuring and formatting text in a constrained layout box.
/// </summary>
/// <remarks>
///  Layout dimensions and font sizes are specified in DIPs.
///  Text ranges map to UTF-16 code unit positions in the source text.
///  Instances own the underlying <see cref="IDWriteTextLayout"/> COM interface pointer and release it when disposed.
///  Mutating members in this wrapper call DirectWrite methods directly and do not check returned <c>HRESULT</c>
///  values.
/// </remarks>
public unsafe class TextLayout : DirectDrawBase<IDWriteTextLayout>
{
    /// <summary>
    ///  Wraps an existing DirectWrite text layout pointer.
    /// </summary>
    /// <param name="layout">The existing text layout interface pointer to wrap.</param>
    /// <remarks>The wrapper takes responsibility for releasing this COM interface pointer when disposed.</remarks>
    public TextLayout(IDWriteTextLayout* layout) : base(layout)
    {
    }

    /// <summary>
    ///  Creates a text layout using a maximum layout size.
    /// </summary>
    /// <param name="text">The text to lay out.</param>
    /// <param name="format">The text format that controls font and paragraph defaults.</param>
    /// <param name="maxSize">The maximum layout width and height in DIPs.</param>
    /// <remarks>
    ///  This constructor calls <see cref="IDWriteFactory.CreateTextLayout(char*, uint, IDWriteTextFormat*, float, float, IDWriteTextLayout**)"/>
    ///  and throws when the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public TextLayout(string text, TextFormat format, SizeF maxSize)
        : this(Create(text, format, maxSize.Width, maxSize.Height))
    {
    }

    /// <summary>
    ///  Creates a text layout using explicit maximum dimensions.
    /// </summary>
    /// <param name="text">The text to lay out.</param>
    /// <param name="format">The text format that controls font and paragraph defaults.</param>
    /// <param name="maxWidth">The maximum layout width in DIPs.</param>
    /// <param name="maxHeight">The maximum layout height in DIPs.</param>
    /// <remarks>
    ///  This constructor calls <see cref="IDWriteFactory.CreateTextLayout(char*, uint, IDWriteTextFormat*, float, float, IDWriteTextLayout**)"/>
    ///  and throws when the underlying API returns a failing <c>HRESULT</c>.
    /// </remarks>
    public TextLayout(string text, TextFormat format, float maxWidth, float maxHeight)
        : this(Create(text, format, maxWidth, maxHeight))
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

    /// <summary>
    ///  Sets the font size for a text range.
    /// </summary>
    /// <param name="fontSize">The font size in DIPs.</param>
    /// <param name="textRange">The UTF-16 text position range to update.</param>
    public void SetFontSize(float fontSize, TextRange textRange)
    {
        Pointer->SetFontSize(fontSize, textRange);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Gets or sets the maximum layout width.
    /// </summary>
    /// <value>The maximum width, in DIPs.</value>
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

    /// <summary>
    ///  Gets or sets the maximum layout height.
    /// </summary>
    /// <value>The maximum height, in DIPs.</value>
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

    /// <summary>
    ///  Gets or sets paragraph-line text alignment for this layout.
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
            Pointer->SetTextAlignment((DWRITE_TEXT_ALIGNMENT)value);
            GC.KeepAlive(this);
        }
    }

    /// <summary>
    ///  Gets or sets paragraph alignment relative to the layout box height.
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
            Pointer->SetParagraphAlignment((DWRITE_PARAGRAPH_ALIGNMENT)value);
            GC.KeepAlive(this);
        }
    }

    /// <summary>
    ///  Sets typography features for a text range.
    /// </summary>
    /// <typeparam name="T">A wrapper type that exposes an <see cref="IDWriteTypography"/> pointer.</typeparam>
    /// <param name="typography">The typography object containing font features to apply.</param>
    /// <param name="textRange">The UTF-16 text position range to update.</param>
    public void SetTypography<T>(T typography, TextRange textRange) where T : IPointer<IDWriteTypography>
    {
        Pointer->SetTypography(typography.Pointer, textRange);
        GC.KeepAlive(this);
        GC.KeepAlive(typography);
    }

    /// <summary>
    ///  Sets underline formatting for a text range.
    /// </summary>
    /// <param name="hasUnderline"><see langword="true"/> to underline; otherwise <see langword="false"/>.</param>
    /// <param name="textRange">The UTF-16 text position range to update.</param>
    public void SetUnderline(bool hasUnderline, TextRange textRange)
    {
        Pointer->SetUnderline(hasUnderline, textRange);
        GC.KeepAlive(this);
    }

    /// <summary>
    ///  Sets font weight for a text range.
    /// </summary>
    /// <param name="fontWeight">The font weight to apply.</param>
    /// <param name="textRange">The UTF-16 text position range to update.</param>
    public void SetFontWeight(FontWeight fontWeight, TextRange textRange)
    {
        Pointer->SetFontWeight((DWRITE_FONT_WEIGHT)fontWeight, textRange);
        GC.KeepAlive(this);
    }
}