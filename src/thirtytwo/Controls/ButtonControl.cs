// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class ButtonControl : Window
{
    private static readonly WindowClass s_buttonClass = new("Button");

    public ButtonControl(
        Rectangle bounds,
        string? text = default,
        Styles buttonStyle = Styles.PushButton,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        IntPtr parameters = default) : base(
            bounds,
            text,
            style |= (WindowStyles)buttonStyle,
            extendedStyle,
            parentWindow,
            s_buttonClass,
            parameters)
    {
    }
}