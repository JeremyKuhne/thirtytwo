// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public class WindowExtensionTests
{
    [Fact]
    public void GetWindowText()
    {
        const string Title = "This is the window title I've always wanted.";
        using Window window = new(Window.DefaultBounds, Title);
        window.GetWindowText().Should().Be(Title);
    }

    [Fact]
    public void GetWindowText_NoText()
    {
        using Window window = new(Window.DefaultBounds);
        window.GetWindowText().Should().Be(string.Empty);
    }

    [Fact]
    public void SetWindowText()
    {
        using Window window = new(Window.DefaultBounds);
        window.SetWindowText("Golly");
        window.GetWindowText().Should().Be("Golly");
    }

    [Fact]
    public void GetExtendedWindowStyle_TopMost()
    {
        using Window window = new(Window.DefaultBounds, extendedStyle: ExtendedWindowStyles.TopMost);
        window.GetExtendedWindowStyle().HasFlag(ExtendedWindowStyles.TopMost).Should().BeTrue();
    }
}
