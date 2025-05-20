// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectWrite;

namespace Windows;

public class TextLabelControl : CustomControl
{
    private static readonly WindowClass s_textLabelClass = new(className: "TextLabelClass");

    private DrawTextFormat _drawTextFormat;
    private TextFormat? _textFormat;
    private SolidColorBrush? _textBrush;
    private Color _textColor;

    public TextLabelControl(
        Rectangle bounds = default,
        DrawTextFormat textFormat = DrawTextFormat.Center | DrawTextFormat.VerticallyCenter | DrawTextFormat.SingleLine,
        string? text = default,
        Color textColor = default,
        WindowStyles style = WindowStyles.Overlapped | WindowStyles.Visible | WindowStyles.Child,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default,
        Color backgroundColor = default,
        Features features = default) : base(
            bounds,
            text,
            style,
            extendedStyle,
            parentWindow,
            s_textLabelClass,
            parameters,
            backgroundColor: backgroundColor,
            features: features)
    {
        _drawTextFormat = textFormat;
        TextColor = textColor;
    }

    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (value.IsEmpty)
            {
                value = Color.Black;
            }

            if (value == _textColor)
            {
                return;
            }

            _textColor = value;
            if (_textBrush is { } brush)
            {
                brush.Color = _textColor;
            }

            this.Invalidate();
        }
    }

    protected override void RenderTargetCreated()
    {
        _textBrush?.Dispose();
        _textBrush = RenderTarget.CreateSolidColorBrush(TextColor);
        base.RenderTargetCreated();
    }

    private TextFormat GetTextFormat()
    {
        if (_textFormat is null)
        {
            HFONT hfont = this.GetFontHandle();
            _textFormat = new TextFormat(hfont, _drawTextFormat);
        }

        return _textFormat;
    }

    protected override void OnPaint()
    {
        if (IsDirect2dEnabled())
        {
            Size size = this.GetClientRectangle().Size;
            using TextLayout textLayout = new(Text, GetTextFormat(), size);
            Debug.Assert(_textBrush is not null);
            RenderTarget.DrawTextLayout(default, textLayout, _textBrush!);
        }
        else
        {
            using var deviceContext = this.BeginPaint();
            Rectangle bounds = this.GetClientRectangle();
            deviceContext.DrawText(Text, bounds, _drawTextFormat, this.GetFontHandle(), TextColor);
        }

        base.OnPaint();
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.SetFont:
                if (_textFormat is not null)
                {
                    // The font isn't set until we call base, so we need to wait to recreate the text format.
                    _textFormat.Dispose();
                    _textFormat = null;
                }

                break;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    public DrawTextFormat TextFormat
    {
        get => _drawTextFormat;
        set
        {
            if (value == _drawTextFormat)
            {
                return;
            }

            _drawTextFormat = value;
            this.Invalidate();
        }
    }
}