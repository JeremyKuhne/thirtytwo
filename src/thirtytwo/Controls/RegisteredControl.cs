// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base <see cref="Control"/> for controls that use registered window classes.
/// </summary>
public class RegisteredControl : Control
{
    public RegisteredControl(
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

    public string Text
    {
        get => this.GetWindowText();
        set => this.SetWindowText(value);
    }
}