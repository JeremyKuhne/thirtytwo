// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Windows;
using Windows.WinUI;

namespace BasicUsage;

/// <summary>Demonstrates WinUI wrapper controls with native thirtytwo controls and layout.</summary>
internal sealed class BasicUsageWindow : MainWindow
{
    private static readonly (string Label, Color Value)[] s_colorPresets =
    [
        ("Custom", Color.Empty),
        ("Cornflower blue", Color.CornflowerBlue),
        ("Crimson", Color.Crimson),
        ("Sea green", Color.SeaGreen),
        ("Gold", Color.Gold),
        ("Slate blue", Color.SlateBlue)
    ];

    private static readonly (string Label, WinUIColorSpectrumShape Value)[] s_spectrumShapes =
    [
        ("Square", WinUIColorSpectrumShape.Box),
        ("Circle", WinUIColorSpectrumShape.Ring)
    ];

    private static readonly (string Label, WinUIColorSpectrumComponents Value)[] s_spectrumComponents =
    [
        ("Hue / saturation", WinUIColorSpectrumComponents.HueSaturation),
        ("Hue / value", WinUIColorSpectrumComponents.HueValue),
        ("Saturation / hue", WinUIColorSpectrumComponents.SaturationHue),
        ("Saturation / value", WinUIColorSpectrumComponents.SaturationValue),
        ("Value / hue", WinUIColorSpectrumComponents.ValueHue),
        ("Value / saturation", WinUIColorSpectrumComponents.ValueSaturation)
    ];

    private static readonly (string Label, WinUIColorPickerOrientation Value)[] s_orientations =
    [
        ("Vertical", WinUIColorPickerOrientation.Vertical),
        ("Horizontal", WinUIColorPickerOrientation.Horizontal)
    ];

    private readonly Dictionary<ButtonControl, Action<bool>> _featureBindings = [];
    private readonly TextLabelControl _titleLabel;
    private readonly WinUIColorPicker _colorPicker;
    private readonly TextLabelControl _statusLabel;
    private readonly StaticControl[] _selectorLabels;
    private readonly ComboBoxControl _colorPresetSelector;
    private readonly ComboBoxControl _spectrumShapeSelector;
    private readonly ComboBoxControl _spectrumComponentsSelector;
    private readonly ComboBoxControl _orientationSelector;
    private readonly ButtonControl[] _featureToggles;
    private readonly Window[] _ownedControls;

    internal BasicUsageWindow()
        : base(
            bounds: new Rectangle(24, 4, 960, 720),
            title: "thirtytwo WinUI Basic Usage",
            backgroundColor: Color.White)
    {
        List<Window> ownedControls = [];

        try
        {
            _titleLabel = Track(new TextLabelControl(
                text: "WinUI ColorPicker",
                textColor: Color.FromArgb(17, 24, 39),
                parentWindow: this,
                backgroundColor: Color.White,
                features: Features.EnableDirect2d), ownedControls);
            _titleLabel.SetFont("Segoe UI", 20);

            _colorPicker = Track(new WinUIColorPicker(default, this)
            {
                Color = Color.CornflowerBlue,
                IsAlphaEnabled = true,
                RequestedTheme = WinUIElementTheme.Light
            }, ownedControls);

            _statusLabel = Track(new TextLabelControl(
                textColor: Color.FromArgb(31, 41, 55),
                parentWindow: this,
                backgroundColor: Color.FromArgb(243, 244, 246),
                features: Features.EnableDirect2d), ownedControls);
            _statusLabel.SetFont("Consolas", 12);

            _selectorLabels =
            [
                Track(CreateLabel("Color preset"), ownedControls),
                Track(CreateLabel("Spectrum shape"), ownedControls),
                Track(CreateLabel("Spectrum axes"), ownedControls),
                Track(CreateLabel("Editor layout"), ownedControls)
            ];

            _colorPresetSelector = Track(CreateSelector(), ownedControls);
            _spectrumShapeSelector = Track(CreateSelector(), ownedControls);
            _spectrumComponentsSelector = Track(CreateSelector(), ownedControls);
            _orientationSelector = Track(CreateSelector(), ownedControls);

            PopulateSelector(_colorPresetSelector, s_colorPresets.Select(item => item.Label));
            PopulateSelector(_spectrumShapeSelector, s_spectrumShapes.Select(item => item.Label));
            PopulateSelector(_spectrumComponentsSelector, s_spectrumComponents.Select(item => item.Label));
            PopulateSelector(_orientationSelector, s_orientations.Select(item => item.Label));

            _spectrumShapeSelector.SelectedIndex = FindIndex(
                s_spectrumShapes,
                _colorPicker.ColorSpectrumShape);
            _spectrumComponentsSelector.SelectedIndex = FindIndex(
                s_spectrumComponents,
                _colorPicker.ColorSpectrumComponents);
            _orientationSelector.SelectedIndex = FindIndex(
                s_orientations,
                _colorPicker.Orientation);

            _featureToggles =
            [
                CreateFeatureToggle("Color spectrum", _colorPicker.IsColorSpectrumVisible,
                    value => _colorPicker.IsColorSpectrumVisible = value, ownedControls),
                CreateFeatureToggle("Color preview", _colorPicker.IsColorPreviewVisible,
                    value => _colorPicker.IsColorPreviewVisible = value, ownedControls),
                CreateFeatureToggle("Color slider", _colorPicker.IsColorSliderVisible,
                    value => _colorPicker.IsColorSliderVisible = value, ownedControls),
                CreateFeatureToggle("Channel inputs", _colorPicker.IsColorChannelTextInputVisible,
                    value => _colorPicker.IsColorChannelTextInputVisible = value, ownedControls),
                CreateFeatureToggle("Hex input", _colorPicker.IsHexInputVisible,
                    value => _colorPicker.IsHexInputVisible = value, ownedControls),
                CreateFeatureToggle("Alpha enabled", _colorPicker.IsAlphaEnabled,
                    value => _colorPicker.IsAlphaEnabled = value, ownedControls),
                CreateFeatureToggle("Alpha slider", _colorPicker.IsAlphaSliderVisible,
                    value => _colorPicker.IsAlphaSliderVisible = value, ownedControls),
                CreateFeatureToggle("Alpha input", _colorPicker.IsAlphaTextInputVisible,
                    value => _colorPicker.IsAlphaTextInputVisible = value, ownedControls)
            ];

            _ownedControls = [.. ownedControls];
            _colorPicker.ColorChanged += ColorPickerColorChanged;
            _colorPresetSelector.SelectionChanged += ColorPresetSelectionChanged;
            _spectrumShapeSelector.SelectionChanged += SpectrumShapeSelectionChanged;
            _spectrumComponentsSelector.SelectionChanged += SpectrumComponentsSelectionChanged;
            _orientationSelector.SelectionChanged += OrientationSelectionChanged;

            this.AddLayoutHandler(CreateWindowLayout());
            SynchronizeSelectedColor(_colorPicker.Color);
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

    private StaticControl CreateLabel(string text)
        => new(
            text: text,
            staticStyle: StaticControl.Styles.Left | StaticControl.Styles.CenterImage | StaticControl.Styles.NoPrefix,
            parentWindow: this);

    private ComboBoxControl CreateSelector()
        => new(
            comboBoxStyle: ComboBoxControl.Styles.DropDownList,
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop | WindowStyles.VerticalScroll,
            parentWindow: this);

    private ButtonControl CreateFeatureToggle(
        string text,
        bool initialValue,
        Action<bool> updateFeature,
        List<Window> ownedControls)
    {
        ButtonControl toggle = Track(new ButtonControl(
            text: text,
            buttonStyle: ButtonControl.Styles.AutoCheckBox | ButtonControl.Styles.Left
                | ButtonControl.Styles.VerticallyCenter,
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
            parentWindow: this), ownedControls);
        toggle.CheckState = initialValue ? ButtonCheckState.Checked : ButtonCheckState.Unchecked;
        toggle.Click += FeatureToggleClick;
        _featureBindings.Add(toggle, updateFeature);
        return toggle;
    }

    private static void PopulateSelector(ComboBoxControl selector, IEnumerable<string> labels)
    {
        foreach (string label in labels)
        {
            selector.AddItem(label);
        }
    }

    private static int FindIndex<TValue>((string Label, TValue Value)[] items, TValue value)
        where TValue : struct, Enum
    {
        for (int index = 0; index < items.Length; index++)
        {
            if (EqualityComparer<TValue>.Default.Equals(items[index].Value, value))
            {
                return index;
            }
        }

        return -1;
    }

    private ILayoutHandler CreateWindowLayout()
    {
        ILayoutHandler selectorLayout = Layout.Horizontal(
            (.25f, CreateSelectorRow(_selectorLabels[0], _colorPresetSelector)),
            (.25f, CreateSelectorRow(_selectorLabels[1], _spectrumShapeSelector)),
            (.25f, CreateSelectorRow(_selectorLabels[2], _spectrumComponentsSelector)),
            (.25f, CreateSelectorRow(_selectorLabels[3], _orientationSelector)));

        ILayoutHandler featureLayout = Layout.Horizontal(
            (.25f, CreateFeatureRow(_featureToggles[0], _featureToggles[1])),
            (.25f, CreateFeatureRow(_featureToggles[2], _featureToggles[3])),
            (.25f, CreateFeatureRow(_featureToggles[4], _featureToggles[5])),
            (.25f, CreateFeatureRow(_featureToggles[6], _featureToggles[7])));

        ILayoutHandler settingsLayout = Layout.Horizontal(
            (.5f, selectorLayout),
            (.5f, featureLayout));

        ILayoutHandler contentLayout = Layout.Vertical(
            (.375f, Layout.Margin((16, 0, 8, 0), settingsLayout)),
            (.625f, Layout.Margin((8, 0, 16, 0), Layout.Fill(_colorPicker))));

        return Layout.Horizontal(
            (.0625f, Layout.Margin((16, 4, 16, 0), Layout.Fill(_titleLabel))),
            (.875f, contentLayout),
            (.0625f, Layout.Margin((16, 4, 16, 8), Layout.Fill(_statusLabel))));
    }

    private static ILayoutHandler CreateSelectorRow(StaticControl label, ComboBoxControl selector)
        => Layout.Vertical(
            (.375f, Layout.Margin((0, 8, 8, 8), Layout.FixedPercent(1f, .5f, Layout.Fill(label)))),
            (.625f, Layout.Margin((8, 8, 0, 8), Layout.FixedPercent(1f, .5f, Layout.Fill(selector)))));

    private static ILayoutHandler CreateFeatureRow(ButtonControl left, ButtonControl right)
        => Layout.Vertical(
            (.5f, Layout.Margin((0, 8, 8, 8), Layout.Fill(left))),
            (.5f, Layout.Margin((8, 8, 0, 8), Layout.Fill(right))));

    private void ColorPresetSelectionChanged(object? sender, EventArgs eventArgs)
    {
        int selectedIndex = _colorPresetSelector.SelectedIndex;
        if (selectedIndex > 0)
        {
            _colorPicker.Color = s_colorPresets[selectedIndex].Value;
        }
    }

    private void SpectrumShapeSelectionChanged(object? sender, EventArgs eventArgs)
    {
        int selectedIndex = _spectrumShapeSelector.SelectedIndex;
        if (selectedIndex >= 0)
        {
            _colorPicker.ColorSpectrumShape = s_spectrumShapes[selectedIndex].Value;
        }
    }

    private void SpectrumComponentsSelectionChanged(object? sender, EventArgs eventArgs)
    {
        int selectedIndex = _spectrumComponentsSelector.SelectedIndex;
        if (selectedIndex >= 0)
        {
            _colorPicker.ColorSpectrumComponents = s_spectrumComponents[selectedIndex].Value;
        }
    }

    private void OrientationSelectionChanged(object? sender, EventArgs eventArgs)
    {
        int selectedIndex = _orientationSelector.SelectedIndex;
        if (selectedIndex >= 0)
        {
            _colorPicker.Orientation = s_orientations[selectedIndex].Value;
        }
    }

    private void FeatureToggleClick(object? sender, EventArgs eventArgs)
    {
        if (sender is ButtonControl toggle && _featureBindings.TryGetValue(toggle, out Action<bool>? updateFeature))
        {
            updateFeature(toggle.CheckState == ButtonCheckState.Checked);
        }
    }

    private void ColorPickerColorChanged(object? sender, WinUIColorChangedEventArgs eventArgs)
        => SynchronizeSelectedColor(eventArgs.NewColor);

    private void SynchronizeSelectedColor(Color color)
    {
        int selectedIndex = 0;
        for (int index = 1; index < s_colorPresets.Length; index++)
        {
            if (s_colorPresets[index].Value.ToArgb() == color.ToArgb())
            {
                selectedIndex = index;
                break;
            }
        }

        _colorPresetSelector.SelectedIndex = selectedIndex;
        _statusLabel.Text = $"Selected  #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}    ARGB  {color.A}, {color.R}, {color.G}, {color.B}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _colorPicker.ColorChanged -= ColorPickerColorChanged;
            _colorPresetSelector.SelectionChanged -= ColorPresetSelectionChanged;
            _spectrumShapeSelector.SelectionChanged -= SpectrumShapeSelectionChanged;
            _spectrumComponentsSelector.SelectionChanged -= SpectrumComponentsSelectionChanged;
            _orientationSelector.SelectionChanged -= OrientationSelectionChanged;
            foreach (ButtonControl toggle in _featureToggles)
            {
                toggle.Click -= FeatureToggleClick;
            }

            _featureBindings.Clear();
            for (int index = _ownedControls.Length - 1; index >= 0; index--)
            {
                _ownedControls[index].Dispose();
            }
        }

        base.Dispose(disposing);
    }
}