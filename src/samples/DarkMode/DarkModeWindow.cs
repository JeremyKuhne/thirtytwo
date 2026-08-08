// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;

namespace DarkModeSample;

/// <summary>Demonstrates application color modes across current native control wrappers.</summary>
internal sealed class DarkModeWindow : MainWindow
{
    private readonly TextLabelControl _title;
    private readonly StaticControl _modeLabel;
    private readonly ComboBoxControl _modeSelector;
    private readonly ButtonControl _cycleButton;
    private readonly StaticControl _staticLabel;
    private readonly EditControl _edit;
    private readonly ComboBoxControl _comboBox;
    private readonly RichEditControl _richEdit;
    private readonly EditControl _disabledEdit;
    private readonly ButtonControl _pushButton;
    private readonly ButtonControl _checkBox;
    private readonly ButtonControl _radioButton;
    private readonly ButtonControl _disabledButton;
    private readonly TextLabelControl _direct2dLabel;
    private readonly TextLabelControl _status;
    private readonly Window[] _ownedControls;

    internal DarkModeWindow()
        : base(
            bounds: new Rectangle(40, 30, 900, 680),
            title: "Dark Mode",
            backgroundColor: default)
    {
        List<Window> ownedControls = [];
        try
        {
            _title = Track(new TextLabelControl(
                text: "Application color mode",
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _title.SetFont("Segoe UI", 22);

            _modeLabel = Track(new StaticControl(
                text: "Requested mode",
                staticStyle: StaticControl.Styles.Left | StaticControl.Styles.CenterImage,
                parentWindow: this), ownedControls);
            _modeSelector = Track(new ComboBoxControl(
                comboBoxStyle: ComboBoxControl.Styles.DropDownList,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.VerticalScroll,
                parentWindow: this), ownedControls);
            _modeSelector.AddItems(["System", "Dark", "Light"]);
            _modeSelector.SelectedIndex = (int)Application.ColorMode;
            _modeSelector.SelectionChanged += ModeSelectorSelectionChanged;

            _cycleButton = Track(new ButtonControl(
                text: "Cycle mode",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);
            _cycleButton.Click += CycleButtonClick;

            _staticLabel = Track(new StaticControl(
                text: "Static text follows the application foreground",
                staticStyle: StaticControl.Styles.Left | StaticControl.Styles.CenterImage,
                parentWindow: this), ownedControls);
            _edit = Track(new EditControl(
                text: "Editable text",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.Border,
                parentWindow: this), ownedControls);
            _comboBox = Track(new ComboBoxControl(
                comboBoxStyle: ComboBoxControl.Styles.DropDownList,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.VerticalScroll,
                parentWindow: this), ownedControls);
            _comboBox.AddItems(["Combo box item", "Second item", "Third item"]);
            _comboBox.SelectedIndex = 0;
            _richEdit = Track(new RichEditControl(
                default,
                text: "Rich edit text\r\nSelection and scrollbars remain part of the compatibility review.",
                editStyle: RichEditControl.Styles.Multiline | RichEditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop
                    | WindowStyles.Border | WindowStyles.VerticalScroll,
                parentWindow: this), ownedControls);
            _disabledEdit = Track(new EditControl(
                text: "Disabled edit text",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.Border | WindowStyles.Disabled,
                parentWindow: this), ownedControls);

            _pushButton = Track(new ButtonControl(
                text: "Push button",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);
            _checkBox = Track(new ButtonControl(
                text: "Check box",
                buttonStyle: ButtonControl.Styles.AutoCheckBox | ButtonControl.Styles.Left,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);
            _checkBox.CheckState = ButtonCheckState.Checked;
            _radioButton = Track(new ButtonControl(
                text: "Radio button",
                buttonStyle: ButtonControl.Styles.AutoRadioButton | ButtonControl.Styles.Left,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);
            _radioButton.CheckState = ButtonCheckState.Checked;
            _disabledButton = Track(new ButtonControl(
                text: "Disabled button",
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.Disabled,
                parentWindow: this), ownedControls);
            _direct2dLabel = Track(new TextLabelControl(
                text: "Direct2D text and background",
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);

            _status = Track(new TextLabelControl(
                text: string.Empty,
                parentWindow: this,
                features: Features.EnableDirect2d), ownedControls);
            _status.SetFont("Consolas", 11);

            _ownedControls = [.. ownedControls];
            this.AddLayoutHandler(CreateLayout());
            UpdateStatus();
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

    private ILayoutHandler CreateLayout()
    {
        ILayoutHandler modeRow = Layout.Vertical(
            (.25f, Layout.Margin((20, 8, 8, 8), Layout.Fill(_modeLabel))),
            (.5f, Layout.Margin((8, 8, 8, 8), Layout.FixedPercent(1f, .55f, _modeSelector))),
            (.25f, Layout.Margin((8, 8, 20, 8), Layout.FixedPercent(.8f, .55f, _cycleButton))));

        ILayoutHandler textControls = Layout.Horizontal(
            (.15f, Layout.Margin((20, 8, 10, 8), Layout.Fill(_staticLabel))),
            (.15f, Layout.Margin((20, 8, 10, 8), Layout.FixedPercent(1f, .55f, _edit))),
            (.15f, Layout.Margin((20, 8, 10, 8), Layout.FixedPercent(1f, .55f, _comboBox))),
            (.4f, Layout.Margin((20, 8, 10, 8), Layout.Fill(_richEdit))),
            (.15f, Layout.Margin((20, 8, 10, 8), Layout.FixedPercent(1f, .55f, _disabledEdit))));

        ILayoutHandler buttonControls = Layout.Horizontal(
            (.16f, Layout.Margin((10, 8, 20, 8), Layout.FixedPercent(.7f, .6f, _pushButton))),
            (.16f, Layout.Margin((10, 8, 20, 8), Layout.Fill(_checkBox))),
            (.16f, Layout.Margin((10, 8, 20, 8), Layout.Fill(_radioButton))),
            (.16f, Layout.Margin((10, 8, 20, 8), Layout.FixedPercent(.7f, .6f, _disabledButton))),
            (.36f, Layout.Margin((10, 8, 20, 8), Layout.Fill(_direct2dLabel))));

        return Layout.Horizontal(
            (.12f, Layout.Margin((20, 12, 20, 4), Layout.Fill(_title))),
            (.12f, modeRow),
            (.64f, Layout.Vertical((.55f, textControls), (.45f, buttonControls))),
            (.12f, Layout.Margin((20, 8, 20, 12), Layout.Fill(_status))));
    }

    private void ModeSelectorSelectionChanged(object? sender, EventArgs eventArgs)
    {
        if (_modeSelector.SelectedIndex >= 0)
        {
            Application.ColorMode = (ApplicationColorMode)_modeSelector.SelectedIndex;
        }
    }

    private void CycleButtonClick(object? sender, EventArgs eventArgs)
    {
        ApplicationColorMode mode = Application.ColorMode switch
        {
            ApplicationColorMode.System => ApplicationColorMode.Dark,
            ApplicationColorMode.Dark => ApplicationColorMode.Light,
            _ => ApplicationColorMode.System
        };

        _modeSelector.SelectedIndex = (int)mode;
        Application.ColorMode = mode;
    }

    /// <inheritdoc/>
    protected override void OnColorModeChanged()
    {
        UpdateStatus();
        base.OnColorModeChanged();
    }

    private void UpdateStatus()
        => _status.Text = $"Requested: {Application.ColorMode}    Change the mode while controls are focused or dropped down.";

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _modeSelector.SelectionChanged -= ModeSelectorSelectionChanged;
            _cycleButton.Click -= CycleButtonClick;
            for (int index = _ownedControls.Length - 1; index >= 0; index--)
            {
                _ownedControls[index].Dispose();
            }
        }

        base.Dispose(disposing);
    }
}