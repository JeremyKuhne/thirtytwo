// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.WinUI;

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
    private readonly StaticControl _winUIRichEditBoxLabel;
    private readonly WinUIRichEditBox _winUIRichEditBox;
    private readonly Window[] _ownedControls;

    internal TextControlComparisonWindow()
        : base(
            bounds: new Rectangle(40, 30, 1100, 720),
            title: "Legacy and WinUI Text Controls")
    {
        List<Window> ownedControls = [];
        try
        {
            _legacyHeading = Track(CreateHeading("Legacy Win32 controls"), ownedControls);
            _legacyTextBoxLabel = Track(CreateLabel("EditControl"), ownedControls);
            _legacyTextBox = Track(new EditControl(
                text: PlainText,
                editStyle: EditControl.Styles.Left | EditControl.Styles.AutoHorizontalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.Border,
                parentWindow: this), ownedControls);
            _legacyTextBox.SetFont("Segoe UI", 11);

            _legacyRichEditBoxLabel = Track(CreateLabel("RichEditControl 4.1"), ownedControls);
            _legacyRichEditBox = Track(new RichEditControl(
                default,
                text: RichText,
                editStyle: RichEditControl.Styles.Multiline
                    | RichEditControl.Styles.AutoVerticalScroll
                    | RichEditControl.Styles.WantReturn,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop
                    | WindowStyles.Border | WindowStyles.VerticalScroll,
                parentWindow: this), ownedControls);

            _winUIHeading = Track(CreateHeading("WinUI wrappers"), ownedControls);
            _winUITextBoxLabel = Track(CreateLabel("WinUITextBox"), ownedControls);
            _winUITextBox = Track(new WinUITextBox(default, this)
            {
                Text = PlainText,
                TextWrapping = WinUITextWrapping.NoWrap
            }, ownedControls);

            _winUIRichEditBoxLabel = Track(CreateLabel("WinUIRichEditBox"), ownedControls);
            _winUIRichEditBox = Track(new WinUIRichEditBox(default, this)
            {
                AcceptsReturn = true,
                Text = RichText,
                TextWrapping = WinUITextWrapping.Wrap
            }, ownedControls);

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

    private static TControl Track<TControl>(TControl control, List<Window> ownedControls)
        where TControl : Window
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

    private ILayoutHandler CreateLayout()
    {
        ILayoutHandler legacyColumn = Layout.Horizontal(
            (.10f, Layout.Margin((20, 16, 10, 4), Layout.Fill(_legacyHeading))),
            (.07f, Layout.Margin((20, 4, 10, 2), Layout.Fill(_legacyTextBoxLabel))),
            (.14f, Layout.Margin((20, 2, 10, 12), Layout.FixedPercent(1f, .55f, _legacyTextBox))),
            (.07f, Layout.Margin((20, 8, 10, 2), Layout.Fill(_legacyRichEditBoxLabel))),
            (.62f, Layout.Margin((20, 2, 10, 20), Layout.Fill(_legacyRichEditBox))));

        ILayoutHandler winUIColumn = Layout.Horizontal(
            (.10f, Layout.Margin((10, 16, 20, 4), Layout.Fill(_winUIHeading))),
            (.07f, Layout.Margin((10, 4, 20, 2), Layout.Fill(_winUITextBoxLabel))),
            (.14f, Layout.Margin((10, 2, 20, 12), Layout.FixedPercent(1f, .55f, _winUITextBox))),
            (.07f, Layout.Margin((10, 8, 20, 2), Layout.Fill(_winUIRichEditBoxLabel))),
            (.62f, Layout.Margin((10, 2, 20, 20), Layout.Fill(_winUIRichEditBox))));

        return Layout.Vertical(
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