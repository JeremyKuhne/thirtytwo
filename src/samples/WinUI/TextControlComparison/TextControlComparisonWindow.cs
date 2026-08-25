// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows;
using Windows.ApplicationModel.DataTransfer;
using Windows.WinUI;
using NativeWindow = Windows.Window;
using ThirtyTwoLayout = Windows.Layout;
using XamlHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using XamlVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;

namespace TextControlComparison;

/// <summary>Compares legacy Win32 text controls with their WinUI wrapper counterparts.</summary>
internal sealed class TextControlComparisonWindow : MainWindow
{
    private const string PlainText = "Edit this plain text";
    private const string RichText =
        "Edit this rich text.\r\n\r\nCompare selection, scrolling, keyboard input, clipboard, and undo behavior.";

    private readonly TextLabelControl _legacyHeading;
    private readonly StaticControl _legacyTextBoxLabel;
    private readonly EditControl _legacyTextBox;
    private readonly StaticControl _legacyRichEditBoxLabel;
    private readonly RichEditControl _legacyRichEditBox;
    private readonly TextLabelControl _winUIHeading;
    private readonly StaticControl _winUITextBoxLabel;
    private readonly WinUITextBox _winUITextBox;
    private readonly XamlHostControl _winUITextBoxDragHandle;
    private readonly StaticControl _winUIRichEditBoxLabel;
    private readonly WinUIRichEditBox _winUIRichEditBox;
    private readonly XamlHostControl _winUIRichEditBoxDragHandle;
    private readonly NativeWindow[] _ownedControls;

    internal TextControlComparisonWindow()
        : base(
            bounds: new Rectangle(40, 30, 1100, 720),
            title: "Legacy and WinUI Text Controls")
    {
        List<NativeWindow> ownedControls = [];
        try
        {
            _legacyHeading = Track(CreateHeading("Legacy Win32 controls (source and target)"), ownedControls);
            _legacyTextBoxLabel = Track(CreateLabel("EditControl (drag selection or drop text)"), ownedControls);
            _legacyTextBox = Track(new EditControl(
                text: PlainText,
                editStyle: EditControl.Styles.Left | EditControl.Styles.AutoHorizontalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.Border,
                parentWindow: this)
            {
                EnableDrag = true,
                EnableDrop = true
            }, ownedControls);
            _legacyTextBox.SetFont("Segoe UI", 11);

            _legacyRichEditBoxLabel = Track(
                CreateLabel("RichEditControl 4.1 (drag selection or drop text)"),
                ownedControls);
            _legacyRichEditBox = Track(new RichEditControl(
                default,
                text: RichText,
                editStyle: RichEditControl.Styles.Multiline
                    | RichEditControl.Styles.AutoVerticalScroll
                    | RichEditControl.Styles.WantReturn,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop
                    | WindowStyles.Border | WindowStyles.VerticalScroll,
                parentWindow: this)
            {
                EnableDrag = true,
                EnableDrop = true
            }, ownedControls);

            _winUIHeading = Track(
                CreateHeading("WinUI wrappers (targets with explicit source handles)"),
                ownedControls);
            _winUITextBoxLabel = Track(CreateLabel("WinUITextBox (drop target)"), ownedControls);
            _winUITextBox = Track(new WinUITextBox(default, this)
            {
                EnableDrop = true,
                Text = PlainText,
                TextWrapping = WinUITextWrapping.NoWrap
            }, ownedControls);
            _winUITextBoxDragHandle = Track(
                CreateDragHandle("WinUITextBox", () => _winUITextBox.SelectedText),
                ownedControls);

            _winUIRichEditBoxLabel = Track(CreateLabel("WinUIRichEditBox (drop target)"), ownedControls);
            _winUIRichEditBox = Track(new WinUIRichEditBox(default, this)
            {
                AcceptsReturn = true,
                EnableDrop = true,
                Text = RichText,
                TextWrapping = WinUITextWrapping.Wrap
            }, ownedControls);
            _winUIRichEditBoxDragHandle = Track(
                CreateDragHandle("WinUIRichEditBox", () => _winUIRichEditBox.SelectedText),
                ownedControls);

            _ownedControls = [.. ownedControls];
            this.AddLayoutHandler(CreateLayout());
        }
        catch
        {
            for (int index = ownedControls.Count - 1; index >= 0; index--)
            {
                ownedControls[index].Dispose();
            }

            base.Dispose(disposing: true);
            throw;
        }
    }

    private static TControl Track<TControl>(TControl control, List<NativeWindow> ownedControls)
        where TControl : NativeWindow
    {
        ownedControls.Add(control);
        return control;
    }

    private TextLabelControl CreateHeading(string text)
    {
        TextLabelControl heading = new(
            text: text,
            parentWindow: this,
            features: Features.EnableDirect2d);
        heading.SetFont("Segoe UI", 18);
        return heading;
    }

    private StaticControl CreateLabel(string text)
        => new(
            text: text,
            staticStyle: StaticControl.Styles.Left | StaticControl.Styles.CenterImage,
            parentWindow: this);

    private XamlHostControl CreateDragHandle(string editorName, Func<string> getSelectedText)
        => new(default, this, () =>
        {
            string accessibleName = $"Drag {editorName} selection";
            TextBlock label = new()
            {
                HorizontalAlignment = XamlHorizontalAlignment.Center,
                Text = accessibleName,
                VerticalAlignment = XamlVerticalAlignment.Center
            };
            Border handle = new()
            {
                Background = new SolidColorBrush(Colors.LightGray),
                BorderBrush = new SolidColorBrush(Colors.DimGray),
                BorderThickness = new Thickness(1),
                CanDrag = true,
                Child = label,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8)
            };
            AutomationProperties.SetName(handle, accessibleName);
            handle.DragStarting += (_, eventArgs) =>
            {
                string selectedText = getSelectedText();
                if (selectedText.Length == 0)
                {
                    eventArgs.Cancel = true;
                    label.Text = "Select text first";
                    return;
                }

                eventArgs.Data.SetText(selectedText);
                eventArgs.Data.RequestedOperation = DataPackageOperation.Copy;
                eventArgs.AllowedOperations = DataPackageOperation.Copy;
                eventArgs.DragUI.SetContentFromDataPackage();
                label.Text = $"Dragging {selectedText.Length} characters";
            };
            handle.DropCompleted += (_, eventArgs) =>
                label.Text = eventArgs.DropResult == DataPackageOperation.Copy
                    ? "Copied - drag again"
                    : "Canceled - drag again";
            return handle;
        });

    private ILayoutHandler CreateLayout()
    {
        ILayoutHandler legacyColumn = ThirtyTwoLayout.Horizontal(
            (.10f, ThirtyTwoLayout.Margin((20, 16, 10, 4), ThirtyTwoLayout.Fill(_legacyHeading))),
            (.07f, ThirtyTwoLayout.Margin((20, 4, 10, 2), ThirtyTwoLayout.Fill(_legacyTextBoxLabel))),
            (.14f, ThirtyTwoLayout.Margin((20, 2, 10, 12), ThirtyTwoLayout.FixedPercent(1f, .55f, _legacyTextBox))),
            (.07f, ThirtyTwoLayout.Margin((20, 8, 10, 2), ThirtyTwoLayout.Fill(_legacyRichEditBoxLabel))),
            (.62f, ThirtyTwoLayout.Margin((20, 2, 10, 20), ThirtyTwoLayout.Fill(_legacyRichEditBox))));

        ILayoutHandler winUIColumn = ThirtyTwoLayout.Horizontal(
            (.10f, ThirtyTwoLayout.Margin((10, 16, 20, 4), ThirtyTwoLayout.Fill(_winUIHeading))),
            (.07f, ThirtyTwoLayout.Margin((10, 4, 20, 2), ThirtyTwoLayout.Fill(_winUITextBoxLabel))),
            (.12f, ThirtyTwoLayout.Margin((10, 2, 20, 4), ThirtyTwoLayout.FixedPercent(1f, .55f, _winUITextBox))),
            (.07f, ThirtyTwoLayout.Margin((10, 2, 20, 8), ThirtyTwoLayout.Fill(_winUITextBoxDragHandle))),
            (.07f, ThirtyTwoLayout.Margin((10, 4, 20, 2), ThirtyTwoLayout.Fill(_winUIRichEditBoxLabel))),
            (.50f, ThirtyTwoLayout.Margin((10, 2, 20, 4), ThirtyTwoLayout.Fill(_winUIRichEditBox))),
            (.07f, ThirtyTwoLayout.Margin((10, 2, 20, 20), ThirtyTwoLayout.Fill(_winUIRichEditBoxDragHandle))));

        return ThirtyTwoLayout.Vertical(
            (.5f, legacyColumn),
            (.5f, winUIColumn));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            for (int index = _ownedControls.Length - 1; index >= 0; index--)
            {
                _ownedControls[index].Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
