// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  A top level window.
/// </summary>
public class MainWindow : Window
{
    /// <summary>
    ///  Initializes a top-level managed window with configurable Win32 creation options.
    /// </summary>
    /// <param name="bounds">Initial bounds in screen coordinates.</param>
    /// <param name="title">Optional window caption text.</param>
    /// <param name="style">Window style flags.</param>
    /// <param name="extendedStyle">Extended window style flags.</param>
    /// <param name="windowClass">Optional class registration settings.</param>
    /// <param name="parameters">Application-defined creation data passed to <c>CreateWindowEx</c>.</param>
    /// <param name="menuHandle">Optional native menu handle.</param>
    /// <param name="backgroundColor">Optional managed background override.</param>
    /// <param name="features">Optional framework feature flags.</param>
    public MainWindow(
        Rectangle bounds = default,
        string? title = default,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        Color backgroundColor = default,
        Features features = default) : base(
            bounds,
            title,
            style,
            extendedStyle,
            default,
            windowClass,
            parameters,
            menuHandle,
            backgroundColor,
            features)
    { }
}