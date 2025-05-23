// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base <see cref="Window"/> for custom controls.
/// </summary>
public class Control : Window
{
    public Control(
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
    }

    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {

        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }
}