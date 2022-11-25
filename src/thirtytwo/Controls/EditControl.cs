// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class EditControl : Window
{
    private static readonly WindowClass s_editClass = new("Edit");

    public EditControl(
        Rectangle bounds,
        string? text = default,
        Styles editStyle = Styles.Left,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        IntPtr parameters = default) : base(
            bounds,
            text,
            style |= (WindowStyles)editStyle,
            extendedStyle,
            parentWindow,
            s_editClass,
            parameters)
    {
    }
}