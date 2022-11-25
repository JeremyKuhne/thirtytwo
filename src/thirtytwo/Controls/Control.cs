// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base Window for custom controls.
/// </summary>
/// <remarks>
///  <para>
///   This adds support for control-like semantics.
///  </para>
/// </remarks>
public class Control : Window
{
    // When I send a WM_GETFONT message to a window, why don't I get a font?
    // https://devblogs.microsoft.com/oldnewthing/20140724-00/?p=413

    // Who is responsible for destroying the font passed in the WM_SETFONT message?
    // https://devblogs.microsoft.com/oldnewthing/20080912-00/?p=20893

    private HFONT _font;

    public Control(
        Rectangle bounds,
        string? text = default,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        WindowClass? windowClass = default,
        nint parameters = 0,
        HMENU menuHandle = default) : base(bounds, text, style, extendedStyle, parentWindow, windowClass, parameters, menuHandle)
    {
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case MessageType.GetFont:
                return (LRESULT)_font.Value;
            case MessageType.SetFont:
                _font = (HFONT)(nint)wParam.Value;
                if ((BOOL)lParam.LOWORD)
                {
                    this.Invalidate();
                }
                return (LRESULT)0;
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}