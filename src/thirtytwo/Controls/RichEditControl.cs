// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Support;

namespace Windows;

/// <summary>
///  <see href="https://learn.microsoft.com/windows/win32/controls/about-rich-edit-controls#rich-edit-version-41">RichEdit 4.1</see> control wrapper.
/// </summary>
public partial class RichEditControl : EditBase
{
    private static readonly WindowClass s_richEditClass;

    static RichEditControl()
    {
        // Ensure RichEdit 4.1 is loaded
        if (Interop.LoadLibrary("Msftedit.dll").IsNull)
        {
            Error.ThrowLastError();
        }

        s_richEditClass = new("RICHEDIT50W");
    }

    public RichEditControl(
        Rectangle bounds,
        string? text = default,
        Styles editStyle = Styles.Left,
        WindowStyles style = WindowStyles.Overlapped,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            s_richEditClass,
            style |= (WindowStyles)editStyle,
            text,
            extendedStyle,
            parentWindow,
            parameters)
    {
    }
}