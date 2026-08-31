// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Drawing;
using Windows;

namespace PowerManagementSample;

/// <summary>Demonstrates using power requests to keep the display and system awake.</summary>
internal sealed class PowerManagementWindow : MainWindow
{
    private readonly EditControl _status;
    private readonly ButtonControl _presentationModeToggle;
    private PresentationPowerRequest? _powerRequest;

    internal PowerManagementWindow()
        : base(
            bounds: new Rectangle(40, 30, 620, 320),
            title: "Power Management",
            backgroundColor: default)
    {
        List<Window> ownedControls = [];
        try
        {
            _status = Track(new EditControl(
                editStyle: EditControl.Styles.Multiline | EditControl.Styles.ReadOnly
                    | EditControl.Styles.AutoVerticalScroll,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.Border,
                parentWindow: this), ownedControls);
            _status.SetFont("Segoe UI", 11);

            _presentationModeToggle = Track(new ButtonControl(
                text: "Request presentation mode",
                buttonStyle: ButtonControl.Styles.AutoCheckBox | ButtonControl.Styles.PushLike
                    | ButtonControl.Styles.Center | ButtonControl.Styles.VerticallyCenter,
                style: WindowStyles.Child | WindowStyles.Visible | WindowStyles.TabStop,
                parentWindow: this), ownedControls);
            _presentationModeToggle.Click += PresentationModeToggleClick;

            this.AddLayoutHandler(Layout.Rows(
                (.76f, Layout.Margin((20, 20, 20, 8), Layout.Fill(_status))),
                (.24f, Layout.Margin((20, 8, 20, 20), Layout.FixedPercent(.55f, .7f, _presentationModeToggle)))));

            UpdateStatus();
        }
        catch
        {
            for (int index = ownedControls.Count - 1; index >= 0; index--)
            {
                ownedControls[index].Dispose();
            }

            _powerRequest?.Dispose();
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

    private void PresentationModeToggleClick(object? sender, EventArgs eventArgs)
    {
        try
        {
            if (_presentationModeToggle.CheckState == ButtonCheckState.Checked)
            {
                _powerRequest = PresentationPowerRequest.Request();
            }
            else
            {
                _powerRequest?.Dispose();
            }

            UpdateStatus();
        }
        catch (Win32Exception exception)
        {
            UpdateStatus($"The power request failed: {exception.Message} ({exception.NativeErrorCode}).");
        }
    }

    private void UpdateStatus(string? error = null)
    {
        bool isActive = _powerRequest?.IsActive ?? false;
        _presentationModeToggle.CheckState = isActive
            ? ButtonCheckState.Checked
            : ButtonCheckState.Unchecked;
        _presentationModeToggle.Text = isActive
            ? "Release presentation mode"
            : "Request presentation mode";

        string state = isActive
            ? "Presentation mode is active.\r\n\r\nThe display and system sleep timers are being held awake."
            : "Presentation mode is inactive.\r\n\r\nWindows can turn off the display and put the system to sleep normally.";

        _status.Text = error is null ? state : $"{state}\r\n\r\n{error}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _presentationModeToggle.Click -= PresentationModeToggleClick;
            _powerRequest?.Dispose();
            _presentationModeToggle.Dispose();
            _status.Dispose();
        }

        base.Dispose(disposing);
    }
}