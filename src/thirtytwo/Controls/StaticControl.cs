// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class StaticControl : RegisteredControl
{
    private static readonly WindowClass s_buttonClass = new(registeredClassName: "Static");

    public StaticControl(
        Rectangle bounds = default,
        string? text = default,
        Styles staticStyle = Styles.Center | Styles.EditControl,
        WindowStyles style = WindowStyles.Overlapped | WindowStyles.Child | WindowStyles.Visible,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            text,
            style |= (WindowStyles)staticStyle,
            extendedStyle,
            parentWindow,
            s_buttonClass,
            parameters)
    {
    }
}