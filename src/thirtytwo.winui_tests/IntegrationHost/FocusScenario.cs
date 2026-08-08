// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows;
using Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.WinUI;
using NativeButton = Windows.ButtonControl;
using RoutedEventArgs = Microsoft.UI.Xaml.RoutedEventArgs;
using Thickness = Microsoft.UI.Xaml.Thickness;
using XamlButton = Microsoft.UI.Xaml.Controls.Button;

namespace IntegrationHost;

internal sealed class FocusScenario : IDisposable
{
    private readonly NativeButton _beforeButton;
    private readonly Window _parent;
    private readonly Window _activationWindow;
    private readonly XamlHostControl _host;
    private readonly XamlButton _firstXamlButton;
    private readonly XamlButton _secondXamlButton;
    private readonly StackPanel _panel;
    private readonly NativeButton _afterButton;
    private readonly ScenarioReporter _reporter;
    private Action? _pendingCompletion;
    private bool _contentLoaded;
    private bool _shiftStateInjected;
    private byte _previousShiftState;
    private int _hostXamlFocusEntryCount;

    internal FocusScenario(Window parent, ScenarioReporter reporter)
    {
        _parent = parent;
        _reporter = reporter;
        _activationWindow = new(new Rectangle(0, 0, 100, 100), text: "Focus activation peer");
        _beforeButton = new(
            bounds: new Rectangle(20, 20, 180, 40),
            text: "Before XAML",
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
            parentWindow: parent);

        XamlButton? firstXamlButton = null;
        XamlButton? secondXamlButton = null;
        StackPanel? panel = null;
        _host = new(new Rectangle(20, 80, 360, 180), parent, () =>
        {
            firstXamlButton = new() { Content = "First XAML button" };
            secondXamlButton = new() { Content = "Second XAML button" };
            panel = new() { Spacing = 12, Padding = new Thickness(20) };
            panel.Children.Add(firstXamlButton);
            panel.Children.Add(secondXamlButton);
            return panel;
        });
        _firstXamlButton = firstXamlButton
            ?? throw new InvalidOperationException("The first XAML button was not created.");
        _secondXamlButton = secondXamlButton
            ?? throw new InvalidOperationException("The second XAML button was not created.");
        _panel = panel ?? throw new InvalidOperationException("The XAML focus panel was not created.");
        _panel.Loaded += PanelLoaded;

        _afterButton = new(
            bounds: new Rectangle(20, 280, 180, 40),
            text: "After XAML",
            style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
            parentWindow: parent);

        _host.XamlGotFocus += HostXamlGotFocus;
    }

    internal void Start(Action completed)
    {
        ArgumentNullException.ThrowIfNull(completed);
        if (!_contentLoaded)
        {
            _pendingCompletion = completed;
            return;
        }

        HWND parent = PInvoke.GetParent(_beforeButton.Handle);
        HWND nextTabStop = PInvoke.GetNextDlgTabItem(parent, _beforeButton.Handle, false);
        Ensure(
            nextTabStop == _host.Handle,
            $"Native tab order did not resolve the XAML host after the before button. Actual HWND: {nextTabStop}.");
        _beforeButton.SetFocus();
        Ensure(PInvoke.GetFocus() == _beforeButton.Handle, "The before button did not receive initial focus.");
        _reporter.Write("focus-native-before", _beforeButton.Handle);

        PostTab(_beforeButton.Handle, backward: false);
        QueueCheckpoint(VerifyForwardFirst);

        void VerifyForwardFirst()
        {
            Ensure(
                ReferenceEquals(GetFocusedXamlElement(), _firstXamlButton),
                $"Forward Tab did not enter the first XAML element. Focus: {PInvoke.GetFocus()}, before: {_beforeButton.Handle}, host: {_host.Handle}, after: {_afterButton.Handle}.");
            _reporter.Write("focus-xaml-first", _host.Handle);
            PostTab(PInvoke.GetFocus(), backward: false);
            QueueCheckpoint(VerifyForwardSecond);
        }

        void VerifyForwardSecond()
        {
            Ensure(ReferenceEquals(GetFocusedXamlElement(), _secondXamlButton), "Forward Tab did not reach the second XAML element.");
            _reporter.Write("focus-xaml-second", _host.Handle);
            PostTab(PInvoke.GetFocus(), backward: false);
            QueueCheckpoint(VerifyForwardAfter);
        }

        void VerifyForwardAfter()
        {
            Ensure(PInvoke.GetFocus() == _afterButton.Handle, "Forward Tab did not leave XAML for the after button.");
            _reporter.Write("focus-native-after", _afterButton.Handle);
            PostTab(_afterButton.Handle, backward: false);
            QueueCheckpoint(VerifyForwardWrap);
        }

        void VerifyForwardWrap()
        {
            Ensure(PInvoke.GetFocus() == _beforeButton.Handle, "Forward Tab did not wrap to the before button.");
            _reporter.Write("focus-forward-wrapped", _beforeButton.Handle);
            PostTab(_beforeButton.Handle, backward: true);
            QueueCheckpoint(VerifyBackwardWrap);
        }

        void VerifyBackwardWrap()
        {
            Ensure(PInvoke.GetFocus() == _afterButton.Handle, "Backward Tab did not wrap to the after button.");
            _reporter.Write("focus-backward-wrapped", _afterButton.Handle);
            PostTab(_afterButton.Handle, backward: true);
            QueueCheckpoint(VerifyBackwardSecond);
        }

        void VerifyBackwardSecond()
        {
            Ensure(ReferenceEquals(GetFocusedXamlElement(), _secondXamlButton), "Backward Tab did not enter the last XAML element.");
            _reporter.Write("focus-backward-xaml-second", _host.Handle);
            PostTab(PInvoke.GetFocus(), backward: true);
            QueueCheckpoint(VerifyBackwardFirst);
        }

        void VerifyBackwardFirst()
        {
            object? focusedElement = GetFocusedXamlElement();
            Ensure(
                ReferenceEquals(focusedElement, _firstXamlButton),
                $"Backward Tab did not reach the first XAML element. XAML focus: {focusedElement?.GetType().FullName ?? "null"}; HWND focus: {PInvoke.GetFocus()}.");
            _reporter.Write("focus-backward-xaml-first", _host.Handle);
            PostTab(PInvoke.GetFocus(), backward: true);
            QueueCheckpoint(VerifyBackwardBefore);
        }

        void VerifyBackwardBefore()
        {
            Ensure(PInvoke.GetFocus() == _beforeButton.Handle, "Backward Tab did not leave XAML for the before button.");
            Ensure(_hostXamlFocusEntryCount == 2, $"The host reported {_hostXamlFocusEntryCount} XAML focus entries instead of 2.");
            _host.ShowWindow(ShowWindowCommand.Hide);
            PostTab(_beforeButton.Handle, backward: false);
            QueueCheckpoint(VerifyHiddenHostSkipped);
        }

        void VerifyHiddenHostSkipped()
        {
            Ensure(PInvoke.GetFocus() == _afterButton.Handle, "Forward Tab did not skip the hidden XAML host.");
            _reporter.Write("focus-hidden-host-skipped", _afterButton.Handle);
            _host.ShowWindow(ShowWindowCommand.Show);
            PInvoke.EnableWindow(_host.Handle, false);
            _beforeButton.SetFocus();
            PostTab(_beforeButton.Handle, backward: false);
            QueueCheckpoint(VerifyDisabledHostSkipped);
        }

        void VerifyDisabledHostSkipped()
        {
            Ensure(PInvoke.GetFocus() == _afterButton.Handle, "Forward Tab did not skip the disabled XAML host.");
            PInvoke.EnableWindow(_host.Handle, true);
            _reporter.Write("focus-disabled-host-skipped", _afterButton.Handle);
            for (int iteration = 0; iteration < 20; iteration++)
            {
                PInvoke.SetActiveWindow(_activationWindow.Handle);
                PInvoke.SetActiveWindow(_parent.Handle);
            }

            _beforeButton.SetFocus();
            Ensure(PInvoke.GetFocus() == _beforeButton.Handle, "Focus could not be restored after reactivation.");
            _reporter.Write("focus-reactivation-stable", _beforeButton.Handle);
            _reporter.Write("focus-traversal-completed", _afterButton.Handle);
            completed();
        }
    }

    public void Dispose()
    {
        RestoreShiftState();
        _panel.Loaded -= PanelLoaded;
        _host.XamlGotFocus -= HostXamlGotFocus;
        _afterButton.Dispose();
        _host.Dispose();
        _beforeButton.Dispose();
        _activationWindow.Dispose();
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void PostTab(HWND target, bool backward)
    {
        if (backward)
        {
            _previousShiftState = KeyboardInput.SetKeyState(VirtualKey.Shift, pressed: true);
            _shiftStateInjected = true;
        }

        KeyboardInput.PostKeyPress(target, VirtualKey.Tab);
    }

    private object? GetFocusedXamlElement()
        => FocusManager.GetFocusedElement(_panel.XamlRoot);

    private void HostXamlGotFocus(object? sender, EventArgs eventArgs)
    {
        _hostXamlFocusEntryCount++;
        _reporter.Write("focus-entered-xaml", _host.Handle);
    }

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
                RestoreShiftState();
                callback();
            }) != true)
        {
            throw new InvalidOperationException("Failed to queue a focus scenario checkpoint.");
        }
    }

    private void RestoreShiftState()
    {
        if (!_shiftStateInjected)
        {
            return;
        }

        KeyboardInput.RestoreKeyState(VirtualKey.Shift, _previousShiftState);
        _shiftStateInjected = false;
    }
}