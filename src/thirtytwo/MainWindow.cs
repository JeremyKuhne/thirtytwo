// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  A top level window.
/// </summary>
public class MainWindow : Window
{
    public MainWindow(
        Rectangle bounds = default,
        string? title = default,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        HBRUSH backgroundBrush = default) : base(
            bounds,
            title,
            style,
            extendedStyle,
            default,
            windowClass,
            parameters,
            menuHandle,
            backgroundBrush)
    { }
}