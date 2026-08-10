// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI.Text;
using XamlDisabledFormattingAccelerators = Microsoft.UI.Xaml.Controls.DisabledFormattingAccelerators;
using XamlRichEditClipboardFormat = Microsoft.UI.Xaml.Controls.RichEditClipboardFormat;

namespace Windows.WinUI;

/// <summary>Hosts a WinUI RichEditBox and projects its editing contract through .NET types.</summary>
public sealed class WinUIRichEditBox : WinUITextControl
{
    /// <summary>Creates a WinUI RichEditBox attached to <paramref name="parentWindow"/>.</summary>
    /// <param name="bounds">The host bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The managed parent window.</param>
    public WinUIRichEditBox(Rectangle bounds, Window parentWindow)
        : base(bounds, parentWindow, richEdit: true)
    {
    }

    /// <summary>Gets or sets the formats copied from the rich editor.</summary>
    public WinUIRichEditClipboardFormat ClipboardCopyFormat
    {
        get => GetRichEditBox().ClipboardCopyFormat switch
        {
            XamlRichEditClipboardFormat.AllFormats => WinUIRichEditClipboardFormat.AllFormats,
            XamlRichEditClipboardFormat.PlainText => WinUIRichEditClipboardFormat.PlainText,
            _ => throw new InvalidOperationException("The rich editor returned an unknown clipboard format.")
        };
        set => GetRichEditBox().ClipboardCopyFormat = value switch
        {
            WinUIRichEditClipboardFormat.AllFormats => XamlRichEditClipboardFormat.AllFormats,
            WinUIRichEditClipboardFormat.PlainText => XamlRichEditClipboardFormat.PlainText,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown rich-edit clipboard format.")
        };
    }

    /// <summary>Gets or sets formatting keyboard accelerators disabled by the rich editor.</summary>
    public WinUIRichEditDisabledFormattingAccelerators DisabledFormattingAccelerators
    {
        get
        {
            XamlDisabledFormattingAccelerators value = GetRichEditBox().DisabledFormattingAccelerators;
            if (value == XamlDisabledFormattingAccelerators.All)
            {
                return WinUIRichEditDisabledFormattingAccelerators.All;
            }

            XamlDisabledFormattingAccelerators knownAccelerators =
                XamlDisabledFormattingAccelerators.Bold
                | XamlDisabledFormattingAccelerators.Italic
                | XamlDisabledFormattingAccelerators.Underline;
            if ((value & ~knownAccelerators) != 0)
            {
                throw new InvalidOperationException("The rich editor returned unknown disabled formatting accelerators.");
            }

            return (WinUIRichEditDisabledFormattingAccelerators)(int)value;
        }
        set
        {
            if (value == WinUIRichEditDisabledFormattingAccelerators.All)
            {
                GetRichEditBox().DisabledFormattingAccelerators = XamlDisabledFormattingAccelerators.All;
                return;
            }

            WinUIRichEditDisabledFormattingAccelerators knownAccelerators =
                WinUIRichEditDisabledFormattingAccelerators.Bold
                | WinUIRichEditDisabledFormattingAccelerators.Italic
                | WinUIRichEditDisabledFormattingAccelerators.Underline;
            if ((value & ~knownAccelerators) != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Unknown disabled formatting accelerators.");
            }

            GetRichEditBox().DisabledFormattingAccelerators = (XamlDisabledFormattingAccelerators)(int)value;
        }
    }

    /// <summary>Gets the rich-text document.</summary>
    public RichEditTextDocument Document => GetRichEditBox().Document;

    /// <summary>Gets the rich-text document through the newer WinUI property name.</summary>
    public RichEditTextDocument TextDocument => GetRichEditBox().TextDocument;
}
