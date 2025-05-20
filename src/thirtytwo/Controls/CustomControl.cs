// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base <see cref="Control"/> for custom window classes.
/// </summary>
public class CustomControl : Control
{
    private string? _text;

    public CustomControl(
        Rectangle bounds = default,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped | WindowStyles.Child | WindowStyles.Visible,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        WindowClass? windowClass = default,
        nint parameters = 0,
        HMENU menuHandle = default,
        Color backgroundColor = default,
        Features features = default) : base(
            bounds,
            text,
            style,
            extendedStyle,
            parentWindow,
            windowClass,
            parameters,
            menuHandle,
            backgroundColor,
            features: features)
    {
        _text = text;
    }

    public string Text
    {
        get => _text ?? string.Empty;
        set
        {
            this.SetWindowText(value);
            _text = value;
        }
    }

    protected override unsafe LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.SetText:
                if (lParam == 0)
                {
                    _text = null;
                }
                else
                {
                    Message.SetText setText = new(lParam);
                    if (!setText.Text.Equals(_text, StringComparison.Ordinal))
                    {
                        _text = setText.Text.ToString();
                    }
                }

                break;

            case MessageType.GetTextLength:
                return (LRESULT)(_text?.Length ?? 0);

            case MessageType.GetText:
                int bufferLength = (int)(nint)wParam.Value;
                char* buffer = (char*)lParam.Value;
                string text = _text ?? string.Empty;
                int copyLength = Math.Min(text.Length, Math.Max(bufferLength - 1, 0));
                if (buffer is not null && bufferLength > 0)
                {
                    text.AsSpan(0, copyLength).CopyTo(new Span<char>(buffer, copyLength));
                    buffer[copyLength] = '\0';
                }

                return (LRESULT)copyLength;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}