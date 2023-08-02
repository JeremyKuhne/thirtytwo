// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Support;
using Windows.Win32.Foundation;
using Windows.Win32.System.Ole;

namespace Tests.Windows;

[Collection(nameof(ClipboardTestCollection))]
public unsafe class ClipboardTests
{
    [Fact]
    public void Clipboard_GetClipboardFormatName()
    {
        uint format = Clipboard.RegisterClipboardFormat("MyText");
        Clipboard.GetClipboardFormatName(format).Should().Be("MyText");
    }

    [Fact]
    public void Clipboard_RegisterClipboardFormat()
    {
        uint format = Clipboard.RegisterClipboardFormat("MyText");
        format.Should().NotBe(0);
    }

    [Fact]
    public void Clipboard_OpenClipboard_AlreadyOpenWithWindow()
    {
        using Window window = new(Window.DefaultBounds);
        try
        {
            Clipboard.OpenClipboard(window).Should().BeTrue();
            Clipboard.OpenClipboard().Should().BeFalse();
        }
        finally
        {
            Clipboard.CloseClipboard();
        }
    }

    [Fact]
    public void Clipboard_OpenClipboard_AlreadyOpenWithoutWindow()
    {
        try
        {
            Clipboard.OpenClipboard().Should().BeTrue();
            Clipboard.OpenClipboard().Should().BeTrue();
        }
        finally
        {
            Clipboard.CloseClipboard();
        }
    }

    [Fact]
    public void Clipboard_GetAvailableClipboardFormats_NoData()
    {
        try
        {
            Clipboard.OpenClipboard().Should().BeTrue();
            Clipboard.EmptyClipboard();
            Clipboard.GetAvailableClipboardFormats().Should().BeEmpty();
        }
        finally
        {
            Clipboard.CloseClipboard();
        }
    }

    [Fact]
    public void Clipboard_GetOpenClipboardWindow_Window()
    {
        using Window window = new(Window.DefaultBounds);
        try
        {
            Clipboard.OpenClipboard(window).Should().BeTrue();
            Clipboard.GetOpenClipboardWindow().Should().Be(window.Handle);
        }
        finally
        {
            Clipboard.CloseClipboard();
        }
    }

    [Fact]
    public void Clipboard_GetOpenClipboardWindow_NoWindow()
    {
        try
        {
            Clipboard.OpenClipboard().Should().BeTrue();
            Clipboard.GetOpenClipboardWindow().IsNull.Should().BeTrue();
        }
        finally
        {
            Clipboard.CloseClipboard();
        }
    }

    [Fact]
    public void Clipboard_CloseClipboard_NotOpen()
    {
        Clipboard.CloseClipboard();
    }

    [Fact]
    public void Clipboard_EmptyClipboard_NotOpen()
    {
        Action action = () => Clipboard.EmptyClipboard();
        action.Should().Throw<ThirtyTwoException>()
            .And.HResult.Should().Be(WIN32_ERROR.ERROR_CLIPBOARD_NOT_OPEN.ToHRESULT());
    }

    [Fact]
    public void Clipboard_SetClipboardData_UnicodeText()
    {
        try
        {
            Clipboard.OpenClipboard().Should().BeTrue();
            Clipboard.EmptyClipboard();
            Clipboard.SetClipboardText("Test string.");
            var formats = Clipboard.GetAvailableClipboardFormats();
            formats.Length.Should().Be(1);
            formats[0].Should().Be((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);
            Clipboard.IsClipboardFormatAvailable((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT).Should().BeTrue();
            Clipboard.IsClipboardFormatAvailable((uint)CLIPBOARD_FORMAT.CF_TEXT).Should().BeFalse();
        }
        finally
        {
            Clipboard.CloseClipboard();
        }

        try
        {
            // After closing the clipboard, Windows adds the synthesized formats.
            Clipboard.OpenClipboard().Should().BeTrue();
            var formats = Clipboard.GetAvailableClipboardFormats();
            formats.Should().BeEquivalentTo(new uint[]
            {
                (uint)CLIPBOARD_FORMAT.CF_UNICODETEXT,
                (uint)CLIPBOARD_FORMAT.CF_LOCALE,
                (uint)CLIPBOARD_FORMAT.CF_TEXT,
                (uint)CLIPBOARD_FORMAT.CF_OEMTEXT
            });
        }
        finally
        {
            Clipboard.CloseClipboard();
        }

        Clipboard.GetClipboardText().Should().Be("Test string.");
    }

    [Fact]
    public void Clipboard_IsClipboardFormatAvailable_No()
    {
        Clipboard.IsClipboardFormatAvailable(uint.MaxValue).Should().BeFalse();
    }
}
