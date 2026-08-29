// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Win32 static control wrapper.
/// </summary>
public partial class StaticControl : RegisteredControl
{
    private static readonly WindowClass s_buttonClass = new(registeredClassName: "Static");

    /// <summary>
    ///  Initializes a static control.
    /// </summary>
    /// <param name="bounds">The control bounds in parent client coordinates.</param>
    /// <param name="text">The initial control text.</param>
    /// <param name="staticStyle">The native static style flags.</param>
    /// <param name="style">The base window style flags.</param>
    /// <param name="extendedStyle">The extended window style flags.</param>
    /// <param name="parentWindow">The parent window that owns this control.</param>
    /// <param name="parameters">Additional creation parameters passed as <c>lpParam</c>.</param>
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