// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class EditControl : EditBase
{
    private static readonly WindowClass s_editClass = new("Edit");

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
    }
}