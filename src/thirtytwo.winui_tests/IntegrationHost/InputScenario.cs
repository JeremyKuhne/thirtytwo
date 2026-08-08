// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows;
using Windows.Threading;
using Windows.Win32;
using Windows.WinUI;
using FocusState = Microsoft.UI.Xaml.FocusState;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
using KeyboardAcceleratorInvokedEventArgs = Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs;
using KeyEventHandler = Microsoft.UI.Xaml.Input.KeyEventHandler;
using RoutedEventArgs = Microsoft.UI.Xaml.RoutedEventArgs;
using Thickness = Microsoft.UI.Xaml.Thickness;
using UIElement = Microsoft.UI.Xaml.UIElement;
using XamlButton = Microsoft.UI.Xaml.Controls.Button;
using XamlComboBox = Microsoft.UI.Xaml.Controls.ComboBox;
using XamlSlider = Microsoft.UI.Xaml.Controls.Slider;
using XamlTextBox = Microsoft.UI.Xaml.Controls.TextBox;

namespace IntegrationHost;

internal sealed class InputScenario : IDisposable
{
    private readonly XamlHostControl _host;
    private readonly XamlButton _button;
    private readonly XamlSlider _slider;
    private readonly XamlComboBox _comboBox;
    private readonly XamlTextBox _textBox;
    private readonly StackPanel _panel;
    private readonly ScenarioReporter _reporter;
    private readonly KeyboardAccelerator _accelerator;
    private readonly KeyEventHandler _buttonKeyDownHandler;
    private Action? _pendingCompletion;
    private bool _contentLoaded;
    private bool _menuStateInjected;
    private byte _previousMenuState;
    private int _acceleratorInvocationCount;
    private int _buttonClickCount;
    private int _enterKeyDownCount;

    internal InputScenario(Window parent, ScenarioReporter reporter)
    {
        _reporter = reporter;
        XamlButton? button = null;
        XamlSlider? slider = null;
        XamlComboBox? comboBox = null;
        XamlTextBox? textBox = null;
        StackPanel? panel = null;
        KeyboardAccelerator? accelerator = null;

        _host = new(new Rectangle(20, 20, 500, 360), parent, () =>
        {
            button = new() { Content = "Keyboard action" };
            accelerator = new()
            {
                Key = Windows.System.VirtualKey.A,
                Modifiers = Windows.System.VirtualKeyModifiers.Menu
            };
            button.KeyboardAccelerators.Add(accelerator);
            slider = new() { Minimum = 0, Maximum = 100, SmallChange = 5, Value = 50 };
            comboBox = new();
            comboBox.Items.Add("First item");
            comboBox.Items.Add("Second item");
            comboBox.SelectedIndex = 0;
            textBox = new() { PlaceholderText = "IME and text input" };
            panel = new() { Spacing = 12, Padding = new Thickness(20) };
            panel.Children.Add(button);
            panel.Children.Add(slider);
            panel.Children.Add(comboBox);
            panel.Children.Add(textBox);
            return panel;
        });

        _button = button ?? throw new InvalidOperationException("The XAML button was not created.");
        _slider = slider ?? throw new InvalidOperationException("The XAML slider was not created.");
        _comboBox = comboBox ?? throw new InvalidOperationException("The XAML combo box was not created.");
        _textBox = textBox ?? throw new InvalidOperationException("The XAML text box was not created.");
        _panel = panel ?? throw new InvalidOperationException("The XAML input panel was not created.");
        _accelerator = accelerator ?? throw new InvalidOperationException("The XAML keyboard accelerator was not created.");
        _panel.Loaded += PanelLoaded;
        _button.Click += ButtonClick;
        _accelerator.Invoked += AcceleratorInvoked;
        _buttonKeyDownHandler = ButtonKeyDown;
        _button.AddHandler(UIElement.KeyDownEvent, _buttonKeyDownHandler, handledEventsToo: true);
    }

    internal void Start(Action completed)
    {
        ArgumentNullException.ThrowIfNull(completed);
        if (!_contentLoaded)
        {
            _pendingCompletion = completed;
            return;
        }

        Ensure(_button.Focus(FocusState.Programmatic), "The XAML button did not accept focus.");
        KeyboardInput.PostKeyPress(PInvoke.GetFocus(), VirtualKey.Space);
        QueueCheckpoint(VerifySpace);

        void VerifySpace()
        {
            Ensure(_buttonClickCount == 1, $"Space activated the XAML button {_buttonClickCount} times.");
            Ensure(ReferenceEquals(GetFocusedElement(), _button), "Space moved focus out of the XAML button.");
            _reporter.Write("input-space-single-activation", _host.Handle);
            KeyboardInput.PostKeyPress(PInvoke.GetFocus(), VirtualKey.Return);
            QueueCheckpoint(VerifyEnter);
        }

        void VerifyEnter()
        {
            Ensure(_enterKeyDownCount == 1, $"Enter reached the XAML button {_enterKeyDownCount} times.");
            Ensure(ReferenceEquals(GetFocusedElement(), _button), "Enter moved focus out of the XAML button.");
            _reporter.Write("input-enter-single-delivery", _host.Handle);
            _previousMenuState = KeyboardInput.SetKeyState(VirtualKey.Menu, pressed: true);
            _menuStateInjected = true;
            KeyboardInput.PostKeyPress(PInvoke.GetFocus(), VirtualKey.A, systemKey: true);
            QueueCheckpoint(VerifyAccelerator);
        }

        void VerifyAccelerator()
        {
            Ensure(_acceleratorInvocationCount == 1, $"Alt+A invoked the accelerator {_acceleratorInvocationCount} times.");
            Ensure(ReferenceEquals(GetFocusedElement(), _button), "The accelerator moved focus out of the XAML button.");
            _reporter.Write("input-accelerator-single-invocation", _host.Handle);
            Ensure(_slider.Focus(FocusState.Programmatic), "The XAML slider did not accept focus.");
            KeyboardInput.PostKeyPress(PInvoke.GetFocus(), VirtualKey.Right);
            QueueCheckpoint(VerifyArrow);
        }

        void VerifyArrow()
        {
            Ensure(_slider.Value == 55, $"Right Arrow changed the slider to {_slider.Value} instead of 55.");
            Ensure(ReferenceEquals(GetFocusedElement(), _slider), "Right Arrow moved focus out of the XAML slider.");
            _reporter.Write("input-arrow-remained-in-xaml", _host.Handle);
            Ensure(_comboBox.Focus(FocusState.Programmatic), "The XAML combo box did not accept focus.");
            _comboBox.IsDropDownOpen = true;
            Ensure(_comboBox.IsDropDownOpen, "The XAML combo-box popup did not open.");
            KeyboardInput.PostKeyPress(PInvoke.GetFocus(), VirtualKey.Escape);
            QueueCheckpoint(VerifyEscape);
        }

        void VerifyEscape()
        {
            Ensure(!_comboBox.IsDropDownOpen, "Escape did not close the XAML combo-box popup.");
            Ensure(Window.FromHandle(PInvoke.GetFocus(), walkParents: true) == _host, "Escape moved focus to a native sibling.");
            Ensure(ReferenceEquals(GetFocusedElement(), _comboBox), "Escape did not restore focus to the XAML combo box.");
            _reporter.Write("input-popup-closed-focus-retained", _host.Handle);
            Ensure(_textBox.Focus(FocusState.Programmatic), "The XAML text box did not accept focus.");
            Ensure(ReferenceEquals(GetFocusedElement(), _textBox), "The XAML text box did not retain focus.");
            _reporter.Write("input-text-page-ready", _host.Handle);
            completed();
        }
    }

    public void Dispose()
    {
        RestoreMenuState();
        _button.RemoveHandler(UIElement.KeyDownEvent, _buttonKeyDownHandler);
        _accelerator.Invoked -= AcceleratorInvoked;
        _button.Click -= ButtonClick;
        _panel.Loaded -= PanelLoaded;
        _host.Dispose();
    }

    private void ButtonClick(object sender, RoutedEventArgs eventArgs)
        => _buttonClickCount++;

    private void AcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs eventArgs)
    {
        _acceleratorInvocationCount++;
        eventArgs.Handled = true;
    }

    private void ButtonKeyDown(object sender, KeyRoutedEventArgs eventArgs)
    {
        if (eventArgs.Key == Windows.System.VirtualKey.Enter)
        {
            _enterKeyDownCount++;
        }
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private object? GetFocusedElement()
        => FocusManager.GetFocusedElement(_panel.XamlRoot);

    private void PanelLoaded(object sender, RoutedEventArgs eventArgs)
    {
        _contentLoaded = true;
        Action? pendingCompletion = _pendingCompletion;
        _pendingCompletion = null;
        if (pendingCompletion is not null)
        {
            QueueCheckpoint(() => Start(pendingCompletion));
        }
    }

    private void QueueCheckpoint(Action callback)
    {
        if (Dispatcher.Current?.TryPost(() =>
            {
                RestoreMenuState();
                callback();
            }) != true)
        {
            throw new InvalidOperationException("Failed to queue an input scenario checkpoint.");
        }
    }

    private void RestoreMenuState()
    {
        if (!_menuStateInjected)
        {
            return;
        }

        KeyboardInput.RestoreKeyState(VirtualKey.Menu, _previousMenuState);
        _menuStateInjected = false;
    }
}