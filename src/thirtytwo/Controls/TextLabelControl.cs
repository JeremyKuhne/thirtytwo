// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public class TextLabelControl : Control
{
    private static readonly WindowClass s_textLabelClass = new(className: "TextLabelClass");

    private DrawTextFormat _textFormat;

    public TextLabelControl(
        Rectangle bounds,
        DrawTextFormat textFormat = DrawTextFormat.Center | DrawTextFormat.VerticallyCenter,
        string? text = default,
        WindowStyles style = default,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            text,
            style,
            extendedStyle,
            parentWindow,
            s_textLabelClass,
            parameters)
    {
        _textFormat = textFormat;
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.Paint:
                {
                    using var deviceContext = window.BeginPaint(out Rectangle paintBounds);
                    using var selectionScope = deviceContext.SelectObject(this.GetFontHandle());
                    deviceContext.DrawText(Text, paintBounds, _textFormat);
                    break;
                }
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    public DrawTextFormat TextFormat
    {
        get => _textFormat;
        set
        {
            if (value == _textFormat)
            {
                return;
            }

            _textFormat = value;
            this.Invalidate();
        }
    }
}