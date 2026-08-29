// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Base <see cref="Window"/> for custom controls.
/// </summary>
public class Control : Window
{
    /// <summary>
    ///  Initializes a control window.
    /// </summary>
    /// <param name="bounds">The initial bounds in parent client coordinates.</param>
    /// <param name="text">The initial window text.</param>
    /// <param name="style">The base window style flags.</param>
    /// <param name="extendedStyle">The extended window style flags.</param>
    /// <param name="parentWindow">The parent window that owns this control.</param>
    /// <param name="windowClass">The window class used to create the native window.</param>
    /// <param name="parameters">Additional creation parameters passed as <c>lpParam</c>.</param>
    /// <param name="menuHandle">The menu handle or child control identifier.</param>
    /// <param name="backgroundColor">The default background color for painting.</param>
    /// <param name="features">Optional control features.</param>
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
}