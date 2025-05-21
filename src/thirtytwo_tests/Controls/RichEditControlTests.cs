// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Controls;

public class RichEditControlTests
{
    [Theory]
    [InlineData(null, true, 1)]
    [InlineData(null, false, 1)]
    [InlineData("Foo", true, 1)]
    [InlineData("Foo", false, 1)]
    [InlineData("Foo\nBar", true, 2)]
    [InlineData("Foo\nBar", false, 1)]
    [InlineData("Foo\r\nBar", true, 2)]
    [InlineData("Foo\r\nBar", false, 1)]
    [InlineData("Foo\rBar", true, 2)]
    [InlineData("Foo\rBar", false, 1)]
    public void LineCount(string? text, bool multiline, int expectedCount)
    {
        using Window window = new(Window.DefaultBounds);
        using RichEditControl edit = new(
            Window.DefaultBounds,
            text,
            editStyle: multiline ? RichEditControl.Styles.Left | RichEditControl.Styles.Multiline : RichEditControl.Styles.Left,
            parentWindow: window);

        edit.LineCount.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(null, 1, true, "")]
    [InlineData(null, 1, false, "")]
    [InlineData(null, 3, true, "")]
    [InlineData(null, 3, false, "")]
    [InlineData("Foo", 0, true, "Foo\r")]
    [InlineData("Foo", 0, false, "Foo\r")]
    [InlineData("Foo", 2, true, "")]
    [InlineData("Foo", 1, false, "")]
    [InlineData("Foo\r\nBar", 0, true, "Foo\r")]
    [InlineData("Foo\r\nBar", 0, false, "Foo\r")]
    [InlineData("Foo\r\nBar", 1, true, "Bar\r")]
    [InlineData("Foo\r\nBar", 1, false, "")]
    public void GetLine(string? text, int lineNumber, bool multiline, string? expectedLine)
    {
        // It isn't clear exactly what is going on with the RichEdit control's handling of EM_LINELENGTH and
        // EM_GETLINE. Why does '\r' get appended to every line? Is this configurable? Line numbers matter when
        // multi-line isn't set, which is also different than how the Edit control behaves (it ignores the line).

        using Window window = new(Window.DefaultBounds);
        using RichEditControl edit = new(
            Window.DefaultBounds,
            text,
            editStyle: multiline ? RichEditControl.Styles.Left | RichEditControl.Styles.Multiline : RichEditControl.Styles.Left,
            parentWindow: window);

        edit.GetLine(lineNumber).Should().Be(expectedLine);
    }

    [Fact]
    public void Selection_Modified_Undo()
    {
        using Window window = new(Window.DefaultBounds);
        using RichEditControl edit = new(Window.DefaultBounds, "Hello", parentWindow: window);

        edit.SetSelection(1, 3);
        edit.ReplaceSelection("i");

        edit.Text.Should().Be("Hilo");
        edit.Modified.Should().BeTrue();
        edit.CanUndo.Should().BeTrue();

        var selection = edit.GetSelection();
        selection.Start.Should().Be(1);

        edit.Undo().Should().BeTrue();
        edit.Text.Should().Be("Hello");

        edit.EmptyUndoBuffer();
        edit.CanUndo.Should().BeFalse();
        edit.Modified = false;
        edit.Modified.Should().BeFalse();
    }
}
