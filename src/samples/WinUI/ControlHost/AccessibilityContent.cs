// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace ControlHost;

internal sealed class AccessibilityContent : ContentControl
{
    internal const string RootAutomationId = "AccessibilityRoot";
    internal const string ColorPickerAutomationId = "AccessibilityColorPicker";
    internal const string ActionAutomationId = "AccessibilityAction";
    internal const string RangeAutomationId = "AccessibilityRange";
    internal const string ValueAutomationId = "AccessibilityValue";

    private readonly ScenarioReporter _reporter;
    private readonly ColorPicker _colorPicker;
    private readonly Button _actionButton;
    private readonly Slider _rangeSlider;
    private readonly TextBox _valueTextBox;

    internal AccessibilityContent(ScenarioReporter reporter)
    {
        _reporter = reporter;
        AutomationProperties.SetAutomationId(this, RootAutomationId);
        AutomationProperties.SetName(this, "Accessibility test root");

        _colorPicker = new()
        {
            IsAlphaEnabled = true,
            Width = 520,
            Height = 400,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AutomationProperties.SetAutomationId(_colorPicker, ColorPickerAutomationId);
        AutomationProperties.SetName(_colorPicker, "Accessibility color picker");

        _actionButton = new() { Content = "Accessibility action" };
        AutomationProperties.SetAutomationId(_actionButton, ActionAutomationId);
        AutomationProperties.SetName(_actionButton, "Accessibility action");

        _rangeSlider = new()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AutomationProperties.SetAutomationId(_rangeSlider, RangeAutomationId);
        AutomationProperties.SetName(_rangeSlider, "Accessibility range");

        _valueTextBox = new()
        {
            Text = "Accessible value",
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AutomationProperties.SetAutomationId(_valueTextBox, ValueAutomationId);
        AutomationProperties.SetName(_valueTextBox, "Accessibility value");

        StackPanel panel = new() { Spacing = 12, Padding = new Thickness(24) };
        panel.Children.Add(_colorPicker);
        panel.Children.Add(_actionButton);
        panel.Children.Add(_rangeSlider);
        panel.Children.Add(_valueTextBox);
        Content = panel;
        Loaded += ContentLoaded;
    }

    private void ContentLoaded(object sender, RoutedEventArgs eventArgs)
    {
        Loaded -= ContentLoaded;

        RequestedTheme = ElementTheme.Light;
        EnsureTheme(ElementTheme.Light, "Light");
        _reporter.Write("theme-light-applied");

        RequestedTheme = ElementTheme.Dark;
        EnsureTheme(ElementTheme.Dark, "Dark");
        _reporter.Write("theme-dark-applied");

        RequestedTheme = ElementTheme.Default;
        ElementTheme inheritedTheme = ActualTheme;
        Ensure(
            _colorPicker.ActualTheme == inheritedTheme
                && _actionButton.ActualTheme == inheritedTheme
                && _rangeSlider.ActualTheme == inheritedTheme
                && _valueTextBox.ActualTheme == inheritedTheme,
            "Default theme was not inherited by every raw accessibility control.");
        _reporter.Write("theme-system-applied");

        Ensure(_actionButton.Focus(FocusState.Programmatic), "The raw accessibility action did not accept focus.");
        _reporter.Write("accessibility-ready");
    }

    private void EnsureTheme(ElementTheme expected, string name)
    {
        Ensure(ActualTheme == expected, $"{name} theme did not reach the raw accessibility root.");
        Ensure(
            _colorPicker.ActualTheme == expected
                && _actionButton.ActualTheme == expected
                && _rangeSlider.ActualTheme == expected
                && _valueTextBox.ActualTheme == expected,
            $"{name} theme was not inherited by every raw accessibility control.");
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}