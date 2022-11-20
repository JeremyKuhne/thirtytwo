// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class StaticControl : Window
{
    private static readonly WindowClass s_buttonClass = new("Static");

    public StaticControl(
        Rectangle bounds,
        string? text = default,
        Styles staticStyle = Styles.Center | Styles.EditControl,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        IntPtr parameters = default) : base(
            s_buttonClass,
            bounds,
            text,
            style |= (WindowStyles)staticStyle,
            extendedStyle,
            parentWindow,
            parameters)
    {
    }
}