// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;
using Windows.Win32.Foundation;

namespace Windows.Controls;

public unsafe class CustomControlTests
{
    [Fact]
    public void TextProperty_RoundTrip()
    {
        using Window window = new(Window.DefaultBounds);
        using CustomControl control = new(Window.DefaultBounds, text: "Foo", parentWindow: window);

        control.Text.Should().Be("Foo");
        control.Text = "Bar";
        control.Text.Should().Be("Bar");
        control.GetWindowText().Should().Be("Bar");
    }

    [Fact]
    public unsafe void SetTextMessage_UpdatesText()
    {
        using Window window = new(Window.DefaultBounds);
        using CustomControl control = new(Window.DefaultBounds, parentWindow: window);

        const string MessageText = "FromMessage";
        fixed (char* c = MessageText)
        {
            control.SendMessage(MessageType.SetText, 0, (LPARAM)c);
        }

        control.Text.Should().Be(MessageText);
        control.GetWindowText().Should().Be(MessageText);
    }

    [Fact]
    public unsafe void GetTextMessage_ReturnsText()
    {
        using Window window = new(Window.DefaultBounds);
        using CustomControl control = new(Window.DefaultBounds, text: "Hello", parentWindow: window);

        int length = (int)control.SendMessage(MessageType.GetTextLength);
        length.Should().Be(5);

        using BufferScope<char> buffer = new(stackalloc char[length + 1]);
        fixed (char* c = buffer)
        {
            int copied = (int)control.SendMessage(MessageType.GetText, (WPARAM)buffer.Length, (LPARAM)c);
            copied.Should().Be(length);
            buffer[..copied].ToString().Should().Be("Hello");
        }
    }

    [Fact]
    public unsafe void GetTextMessage_BufferSmallerThanTextTruncates()
    {
        using Window window = new(Window.DefaultBounds);
        using CustomControl control = new(Window.DefaultBounds, text: "HelloWorld", parentWindow: window);

        int length = (int)control.SendMessage(MessageType.GetTextLength);
        length.Should().Be(10);

        // Create a buffer smaller than the text length
        using BufferScope<char> buffer = new(stackalloc char[5]); // Only enough for "Hello" without null terminator
        fixed (char* c = buffer)
        {
            int copied = (int)control.SendMessage(MessageType.GetText, (WPARAM)buffer.Length, (LPARAM)c);
            copied.Should().Be(4); // Only 4 characters should be copied (to allow for null terminator)
            buffer[..copied].ToString().Should().Be("Hell"); // Should only copy "Hell"
        }
    }
}
