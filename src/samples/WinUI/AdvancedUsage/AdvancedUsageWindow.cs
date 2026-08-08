// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows;
using Windows.ApplicationModel.DataTransfer;
using Windows.WinUI;
using ThirtyTwoLayout = Windows.Layout;
using XamlHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using XamlVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace AdvancedUsage;

/// <summary>
///  Demonstrates generic and typed WinUI hosting in a thirtytwo window.
/// </summary>
internal sealed class AdvancedUsageWindow : MainWindow
{
    private readonly ButtonControl _beforeButton;
    private readonly XamlHostControl _overviewHost;
    private readonly WinUIColorPicker _colorPicker;
    private readonly TextLabelControl _statusLabel;
    private readonly ButtonControl _afterButton;

    internal AdvancedUsageWindow()
        : base(
            bounds: new Rectangle(24, 4, 960, 720),
            title: "thirtytwo WinUI Advanced Usage",
            backgroundColor: Color.White)
    {
        ButtonControl? beforeButton = null;
        XamlHostControl? overviewHost = null;
        WinUIColorPicker? colorPicker = null;
        TextLabelControl? statusLabel = null;
        ButtonControl? afterButton = null;

        try
        {
            beforeButton = new(
                text: "Native control before WinUI",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this);
            overviewHost = new(default, this, CreateOverviewContent);
            colorPicker = new(default, this)
            {
                IsAlphaEnabled = true,
                Color = Color.CornflowerBlue,
                Orientation = WinUIColorPickerOrientation.Horizontal,
                RequestedTheme = WinUIElementTheme.Light
            };
            statusLabel = new(
                text: string.Empty,
                parentWindow: this,
                textColor: Color.FromArgb(31, 41, 55),
                backgroundColor: Color.White,
                features: Features.EnableDirect2d);
            afterButton = new(
                text: "Native control after WinUI",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this);

            _beforeButton = beforeButton;
            _overviewHost = overviewHost;
            _colorPicker = colorPicker;
            _statusLabel = statusLabel;
            _afterButton = afterButton;
            _statusLabel.SetFont("Consolas", 10);

            _colorPicker.ColorChanged += ColorPickerColorChanged;
            this.AddLayoutHandler(ThirtyTwoLayout.Horizontal(
                (.0625f, ThirtyTwoLayout.Margin((16, 16, 16, 4), ThirtyTwoLayout.Fill(_beforeButton))),
                (.4375f, ThirtyTwoLayout.Margin((16, 4, 16, 4), ThirtyTwoLayout.Fill(_overviewHost))),
                (.4375f, ThirtyTwoLayout.Vertical(
                    (.75f, ThirtyTwoLayout.Margin((16, 4, 8, 4), ThirtyTwoLayout.Fill(_colorPicker))),
                    (.25f, ThirtyTwoLayout.Margin((8, 4, 16, 4), ThirtyTwoLayout.Fill(_statusLabel))))),
                (.0625f, ThirtyTwoLayout.Margin((16, 4, 16, 16), ThirtyTwoLayout.Fill(_afterButton)))));
            UpdateStatus(_colorPicker.Color);
        }
        catch
        {
            if (colorPicker is not null)
            {
                colorPicker.ColorChanged -= ColorPickerColorChanged;
            }

            afterButton?.Dispose();
            statusLabel?.Dispose();
            colorPicker?.Dispose();
            overviewHost?.Dispose();
            beforeButton?.Dispose();
            base.Dispose(disposing: true);
            throw;
        }
    }

    private static UIElement CreateOverviewContent(XamlHostContext context)
    {
        string applicationOwnership = context.OwnsApplication ? "created" : "adopted";
        string queueOwnership = context.OwnsDispatcherQueue ? "created" : "borrowed";
        Border border = new()
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8),
            RequestedTheme = ElementTheme.Light
        };
        StackPanel panel = new() { Spacing = 4 };
        panel.Children.Add(new TextBlock
        {
            Text = "Generic XamlHostControl",
            FontSize = 18
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"Application: {applicationOwnership}  |  Dispatcher queue: {queueOwnership}",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        });
        TabView inputTabs = new()
        {
            Height = 210,
            IsAddTabButtonVisible = false,
            TabWidthMode = TabViewWidthMode.Equal
        };
        inputTabs.TabItems.Add(new TabViewItem
        {
            Header = "Keyboard",
            IsClosable = false,
            Content = CreateKeyboardInputPage()
        });
        inputTabs.TabItems.Add(new TabViewItem
        {
            Header = "Pointer",
            IsClosable = false,
            Content = CreatePointerInputPage()
        });
        inputTabs.TabItems.Add(new TabViewItem
        {
            Header = "Drag and drop",
            IsClosable = false,
            Content = CreateDragDropPage()
        });
        panel.Children.Add(inputTabs);
        border.Child = panel;
        return border;
    }

    private static UIElement CreateKeyboardInputPage()
    {
        Grid grid = new()
        {
            ColumnSpacing = 16,
            Padding = new Thickness(8)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        TextBlock actionStatus = new()
        {
            Text = "Waiting for Enter, Space, or Alt+A",
            TextWrapping = TextWrapping.Wrap
        };
        int buttonActivationCount = 0;
        int acceleratorInvocationCount = 0;
        Button actionButton = new() { Content = "Keyboard action (Alt+A)" };
        KeyboardAccelerator accelerator = new()
        {
            Key = global::Windows.System.VirtualKey.A,
            Modifiers = global::Windows.System.VirtualKeyModifiers.Menu
        };
        actionButton.KeyboardAccelerators.Add(accelerator);
        actionButton.Click += (sender, eventArgs)
            => actionStatus.Text = $"Button activations: {++buttonActivationCount}";
        accelerator.Invoked += (sender, eventArgs) =>
        {
            actionStatus.Text = $"Alt+A invocations: {++acceleratorInvocationCount}";
            eventArgs.Handled = true;
        };

        StackPanel textColumn = new() { Spacing = 8 };
        textColumn.Children.Add(new TextBox
        {
            Header = "Text and IME",
            PlaceholderText = "Type or compose text"
        });
        textColumn.Children.Add(actionButton);
        textColumn.Children.Add(actionStatus);
        grid.Children.Add(textColumn);

        StackPanel controlColumn = new() { Spacing = 8 };
        controlColumn.Children.Add(new TextBlock { Text = "Arrow-key slider" });
        controlColumn.Children.Add(new Slider { Minimum = 0, Maximum = 100, SmallChange = 5, Value = 50 });
        ComboBox popup = new() { Header = "Popup" };
        popup.Items.Add("First item");
        popup.Items.Add("Second item");
        popup.SelectedIndex = 0;
        controlColumn.Children.Add(popup);
        Grid.SetColumn(controlColumn, 1);
        grid.Children.Add(controlColumn);
        return grid;
    }

    private static UIElement CreatePointerInputPage()
    {
        TextBlock pointerStatus = new()
        {
            Text = "Pointer idle",
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = XamlHorizontalAlignment.Center,
            VerticalAlignment = XamlVerticalAlignment.Center
        };
        Border interactionSurface = new()
        {
            MinHeight = 150,
            Margin = new Thickness(8),
            Padding = new Thickness(20),
            Background = new SolidColorBrush(Microsoft.UI.Colors.AliceBlue),
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.SteelBlue),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(6),
            Child = pointerStatus
        };
        interactionSurface.PointerPressed += (sender, eventArgs) =>
        {
            bool captured = interactionSurface.CapturePointer(eventArgs.Pointer);
            string captureStatus = captured ? "captured" : "capture declined";
            pointerStatus.Text = $"{DescribePointer(eventArgs, interactionSurface, "Pressed")}; {captureStatus}";
            eventArgs.Handled = true;
        };
        interactionSurface.PointerMoved += (sender, eventArgs)
            => pointerStatus.Text = DescribePointer(eventArgs, interactionSurface, "Moved");
        interactionSurface.PointerReleased += (sender, eventArgs) =>
        {
            interactionSurface.ReleasePointerCapture(eventArgs.Pointer);
            pointerStatus.Text = DescribePointer(eventArgs, interactionSurface, "Released");
            eventArgs.Handled = true;
        };
        interactionSurface.PointerCaptureLost += (sender, eventArgs)
            => pointerStatus.Text = DescribePointer(eventArgs, interactionSurface, "Capture released");
        interactionSurface.PointerWheelChanged += (sender, eventArgs) =>
        {
            int wheelDelta = eventArgs.GetCurrentPoint(interactionSurface).Properties.MouseWheelDelta;
            pointerStatus.Text = $"{DescribePointer(eventArgs, interactionSurface, "Wheel")}; delta {wheelDelta}";
            eventArgs.Handled = true;
        };
        return interactionSurface;
    }

    private static UIElement CreateDragDropPage()
    {
        TextBlock dragText = new()
        {
            Text = "Drag source",
            HorizontalAlignment = XamlHorizontalAlignment.Center,
            VerticalAlignment = XamlVerticalAlignment.Center
        };
        Border dragSource = new()
        {
            Width = 260,
            MinHeight = 112,
            CanDrag = true,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Honeydew),
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.SeaGreen),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(6),
            Child = dragText
        };
        dragSource.DragStarting += (sender, eventArgs) =>
        {
            eventArgs.Data.SetText("Text transferred from WinUI");
            eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
            dragText.Text = "Dragging text";
        };

        TextBlock dropText = new()
        {
            Text = "Drop target",
            HorizontalAlignment = XamlHorizontalAlignment.Center,
            VerticalAlignment = XamlVerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        Border dropTarget = new()
        {
            Width = 260,
            MinHeight = 112,
            AllowDrop = true,
            Background = new SolidColorBrush(Microsoft.UI.Colors.MistyRose),
            BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.IndianRed),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(6),
            Child = dropText
        };
        dropTarget.DragOver += (sender, eventArgs) =>
        {
            eventArgs.AcceptedOperation = DataPackageOperation.Copy;
            eventArgs.Handled = true;
        };
        dropTarget.Drop += async (sender, eventArgs) =>
        {
            eventArgs.Handled = true;
            try
            {
                dropText.Text = eventArgs.DataView.Contains(StandardDataFormats.Text)
                    ? await eventArgs.DataView.GetTextAsync()
                    : "Unsupported drop data";
            }
            catch (Exception exception)
            {
                dropText.Text = $"Drop failed: {exception.Message}";
            }
        };

        StackPanel panel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Padding = new Thickness(12),
            HorizontalAlignment = XamlHorizontalAlignment.Center
        };
        panel.Children.Add(dragSource);
        panel.Children.Add(dropTarget);
        return panel;
    }

    private static string DescribePointer(PointerRoutedEventArgs eventArgs, UIElement relativeTo, string action)
    {
        Microsoft.UI.Input.PointerPoint pointerPoint = eventArgs.GetCurrentPoint(relativeTo);
        return $"{action}: {pointerPoint.PointerDeviceType}, X {Math.Round(pointerPoint.Position.X)}, Y {Math.Round(pointerPoint.Position.Y)}";
    }

    private void ColorPickerColorChanged(object? sender, WinUIColorChangedEventArgs eventArgs)
        => UpdateStatus(eventArgs.NewColor);

    private void UpdateStatus(Color color)
    {
        _statusLabel.Text = $"ARGB  {color.A}, {color.R}, {color.G}, {color.B}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _colorPicker.ColorChanged -= ColorPickerColorChanged;
            _afterButton.Dispose();
            _statusLabel.Dispose();
            _colorPicker.Dispose();
            _overviewHost.Dispose();
            _beforeButton.Dispose();
        }

        base.Dispose(disposing);
    }
}