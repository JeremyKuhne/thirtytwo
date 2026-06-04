// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Controls;

[TestClass]
public class EditControlTests
{
    [TestMethod]
    [DataRow(null, true, 1)]
    [DataRow(null, false, 1)]
    [DataRow("Foo", true, 1)]
    [DataRow("Foo", false, 1)]
    [DataRow("Foo\r\nBar", true, 2)]
    [DataRow("Foo\r\nBar", false, 1)]
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

    [TestMethod]
    [DataRow(null, 1, true,"")]
    [DataRow(null, 1, false, "")]
    [DataRow(null, 3, true, "")]
    [DataRow(null, 3, false, "")]
    [DataRow("Foo", 0, true, "Foo")]
    [DataRow("Foo", 0, false, "Foo")]
    [DataRow("Foo", 2, true, "")]
    [DataRow("Foo", 2, false, "Foo")]
    [DataRow("Foo\r\nBar", 0, true, "Foo")]
    [DataRow("Foo\r\nBar", 0, false, "Foo\r\nBar")]
    [DataRow("Foo\r\nBar", 1, true, "Bar")]
    [DataRow("Foo\r\nBar", 1, false, "Foo\r\nBar")]
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

    [TestMethod]
    public void Selection_Modified_Undo()
    {
        using Window window = new(Window.DefaultBounds);
        using EditControl edit = new(Window.DefaultBounds, "Hello", parentWindow: window);

        edit.SetSelection(1, 3);
        edit.ReplaceSelection("i");

        edit.Text.Should().Be("Hilo");
        edit.Modified.Should().BeTrue();
        edit.CanUndo.Should().BeTrue();

        (int start, int end) = edit.GetSelection();
        start.Should().Be(2);
        end.Should().Be(2);

        edit.Undo().Should().BeTrue();
        edit.Text.Should().Be("Hello");

        edit.EmptyUndoBuffer();
        edit.CanUndo.Should().BeFalse();
        edit.Modified = false;
        edit.Modified.Should().BeFalse();
    }
}
