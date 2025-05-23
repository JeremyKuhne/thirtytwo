// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.Controls;

public class ComboBoxControlTests
{
    [Fact]
    public void Add_Select_Clear_Works()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(
            bounds: new Rectangle(0, 0, 100, 24),
            comboBoxStyle: ComboBoxControl.Styles.DropDownList,
            parentWindow: window);

        combo.Count.Should().Be(0);
        combo.AddItem("One");
        combo.AddItem("Two");
        combo.AddItem("Three");
        combo.Count.Should().Be(3);

        combo.SelectedIndex = 1;
        combo.SelectedIndex.Should().Be(1);
        combo.SelectedItem.Should().Be("Two");

        combo.SelectedItem = "Three";
        combo.SelectedIndex.Should().Be(2);
        combo.SelectedItem.Should().Be("Three");

        combo.Clear();
        combo.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_DefaultValues_Created()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.Count.Should().Be(0);
        combo.SelectedIndex.Should().Be(-1);
        combo.SelectedItem.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithStyles_Created()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(
            bounds: new Rectangle(10, 10, 150, 30),
            text: "Test ComboBox",
            comboBoxStyle: ComboBoxControl.Styles.DropDown,
            parentWindow: window);

        combo.Count.Should().Be(0);
    }

    [Fact]
    public void AddItem_WithValidItem_AddsAndReturnsIndex()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = combo.AddItem("Item1");

        count.Should().Be(1);
        combo.Count.Should().Be(1);
    }

    [Fact]
    public void AddItem_WithNullOrEmpty_ThrowsArgumentException()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        FluentActions.Invoking(() => combo.AddItem(null!))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => combo.AddItem(string.Empty))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItems_WithValidCollection_AddsAllAndReturnsCount()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        var items = new List<string> { "Item1", "Item2", "Item3" };
        int count = combo.AddItems(items);

        count.Should().Be(3);
        combo.Count.Should().Be(3);
    }

    [Fact]
    public void AddItems_WithNullCollection_ThrowsArgumentNullException()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        FluentActions.Invoking(() => combo.AddItems(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddItems_WithEmptyCollection_ReturnsCurrentCount()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);
        combo.AddItem("Test"); // Add one item to ensure count isn't zero

        int count = combo.AddItems(Array.Empty<string>());

        count.Should().Be(1);
        combo.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveItem_WithValidIndex_RemovesAndReturnsRemainingCount()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.AddItem("Item2");
        combo.AddItem("Item3");

        int remaining = combo.RemoveItem(1);

        remaining.Should().Be(2);
        combo.Count.Should().Be(2);
        combo.GetItemText(0).Should().Be("Item1");
        combo.GetItemText(1).Should().Be("Item3");
    }

    [Fact]
    public void RemoveItem_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");

        FluentActions.Invoking(() => combo.RemoveItem(-1))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => combo.RemoveItem(1)) // Out of bounds
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.AddItem("Item2");

        combo.Clear();

        combo.Count.Should().Be(0);
    }

    [Fact]
    public void SelectedIndex_SetAndGet_UpdatesCorrectly()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.AddItem("Item2");

        combo.SelectedIndex = 1;
        combo.SelectedIndex.Should().Be(1);

        combo.SelectedIndex = 0;
        combo.SelectedIndex.Should().Be(0);

        combo.SelectedIndex = -1; // No selection
        combo.SelectedIndex.Should().Be(-1);
    }

    [Fact]
    public void GetItemText_WithValidIndex_ReturnsText()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.AddItem("Item2");

        string text = combo.GetItemText(1);

        text.Should().Be("Item2");
    }

    [Fact]
    public void GetItemText_WithInvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");

        FluentActions.Invoking(() => combo.GetItemText(-1))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => combo.GetItemText(1)) // Out of bounds
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SelectedItem_SetAndGet_UpdatesCorrectly()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.AddItem("Item2");

        combo.SelectedItem = "Item2";
        combo.SelectedItem.Should().Be("Item2");
        combo.SelectedIndex.Should().Be(1);

        combo.SelectedItem = "Item1";
        combo.SelectedItem.Should().Be("Item1");
        combo.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void SelectedItem_SetToNull_ClearsSelection()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.SelectedIndex = 0;

        combo.SelectedItem = null;

        combo.SelectedIndex.Should().Be(-1);
        combo.SelectedItem.Should().BeNull();
    }

    [Fact]
    public void SelectedItem_SetToNonExistentItem_SetsIndexToNegativeOne()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.AddItem("Item1");
        combo.SelectedIndex = 0;

        combo.SelectedItem = "NonExistentItem";

        combo.SelectedIndex.Should().Be(-1);
        combo.SelectedItem.Should().BeNull();
    }

    [Fact]
    public void Count_ReflectsNumberOfItems()
    {
        using Window window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        combo.Count.Should().Be(0);

        combo.AddItem("Item1");
        combo.Count.Should().Be(1);

        combo.AddItem("Item2");
        combo.Count.Should().Be(2);

        combo.RemoveItem(0);
        combo.Count.Should().Be(1);

        combo.Clear();
        combo.Count.Should().Be(0);
    }

    [Fact]
    public void SelectionChanged_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(bounds: new(default, new(400, 40)), parentWindow: window);

        int changeCount = 0;
        combo.AddItems(new[] { "Item1", "Item2", "Item3" });
        combo.SelectionChanged += (s, e) =>
        {
            changeCount++;
        };

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_SELCHANGE), (LPARAM)combo.Handle);
        changeCount.Should().Be(1);
    }

    [Fact]
    public void GotFocus_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.GotFocus += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_SETFOCUS), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void LostFocus_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.LostFocus += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_KILLFOCUS), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void DropDown_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.DropDown += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_DROPDOWN), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void CloseUp_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.CloseUp += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_CLOSEUP), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void DoubleClicked_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.DoubleClicked += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_DBLCLK), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void TextChanged_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.TextChanged += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_EDITCHANGE), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void TextUpdated_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.TextUpdated += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_EDITUPDATE), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void SelectionCommitted_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.SelectionCommitted += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_SELENDOK), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }

    [Fact]
    public void SelectionCanceled_FiresFromCommandMessage()
    {
        using MainWindow window = new(Window.DefaultBounds);
        using ComboBoxControl combo = new(parentWindow: window);

        int count = 0;
        combo.SelectionCanceled += (s, e) => count++;

        window.SendMessage(MessageType.Command, WPARAM.MAKEWPARAM(0, (int)Interop.CBN_SELENDCANCEL), (LPARAM)combo.Handle);
        count.Should().Be(1);
    }
}
