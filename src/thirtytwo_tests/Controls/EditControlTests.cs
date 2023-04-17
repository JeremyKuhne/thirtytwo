// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;

namespace Tests.Windows.Controls;

public class EditControlTests
{
    [Theory]
    [InlineData(null, true, 1)]
    [InlineData(null, false, 1)]
    [InlineData("Foo", true, 1)]
    [InlineData("Foo", false, 1)]
    [InlineData("Foo\r\nBar", true, 2)]
    [InlineData("Foo\r\nBar", false, 1)]
    public void LineCount(string? text, bool multiline, int expectedCount)
    {
        using Window window = new(Window.DefaultBounds);
        using EditControl edit = new(
            Window.DefaultBounds,
            text,
            editStyle: multiline ? EditControl.Styles.Left | EditControl.Styles.Multiline : EditControl.Styles.Left,
            parentWindow: window);

        edit.LineCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, 1, true,"")]
    [InlineData(null, 1, false, "")]
    [InlineData(null, 3, true, "")]
    [InlineData(null, 3, false, "")]
    [InlineData("Foo", 0, true, "Foo")]
    [InlineData("Foo", 0, false, "Foo")]
    [InlineData("Foo", 2, true, "")]
    [InlineData("Foo", 2, false, "Foo")]
    [InlineData("Foo\r\nBar", 0, true, "Foo")]
    [InlineData("Foo\r\nBar", 0, false, "Foo\r\nBar")]
    [InlineData("Foo\r\nBar", 1, true, "Bar")]
    [InlineData("Foo\r\nBar", 1, false, "Foo\r\nBar")]
    public void GetLine(string? text, int lineNumber, bool multiline, string? expectedLine)
    {
        using Window window = new(Window.DefaultBounds);
        using EditControl edit = new(
            Window.DefaultBounds,
            text,
            editStyle: multiline? EditControl.Styles.Left | EditControl.Styles.Multiline : EditControl.Styles.Left,
            parentWindow: window);

        edit.GetLine(lineNumber).Should().Be(expectedLine);
    }
}
