// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  <see href="https://learn.microsoft.com/windows/win32/controls/edit-controls">Edit common control</see> wrapper.
/// </summary>
public partial class EditControl : EditBase
{
    private static readonly WindowClass s_editClass = new("Edit");
    private readonly bool _usesApplicationScrollBarTheme;

    public EditControl(
        Rectangle bounds = default,
        string? text = default,
        Styles editStyle = Styles.Left,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            s_editClass,
            style |= (WindowStyles)editStyle,
            text,
            extendedStyle,
            parentWindow,
            parameters)
    {
        _usesApplicationScrollBarTheme = (style
            & (WindowStyles.HorizontalScroll | WindowStyles.VerticalScroll)) != 0;
        ApplyApplicationTheme();
    }

    /// <inheritdoc/>
    protected override void OnColorModeChanged()
    {
        ApplyApplicationTheme();
        base.OnColorModeChanged();
    }

    private void ApplyApplicationTheme()
    {
        if (_usesApplicationScrollBarTheme)
        {
            ApplyApplicationDarkModeTheme("DarkMode_Explorer");
        }
    }
}