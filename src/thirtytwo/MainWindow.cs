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
        string? title = default,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        HBRUSH backgroundBrush = default) : this(
            DefaultBounds,
            title,
            style,
            extendedStyle,
            windowClass,
            parameters,
            menuHandle,
            backgroundBrush)
    { }

    public MainWindow(
        Rectangle bounds,
        string? text = default,
        WindowStyles style = WindowStyles.OverlappedWindow,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        WindowClass? windowClass = default,
        nint parameters = default,
        HMENU menuHandle = default,
        HBRUSH backgroundBrush = default) : base(
            bounds,
            text,
            style,
            extendedStyle,
            default,
            windowClass,
            parameters,
            menuHandle,
            backgroundBrush)
    { }
}