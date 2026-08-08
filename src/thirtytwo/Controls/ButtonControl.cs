// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public partial class ButtonControl : RegisteredControl
{
    private static readonly WindowClass s_buttonClass = new(registeredClassName: "Button");

    /// <summary>Occurs when the user clicks the button.</summary>
    public event EventHandler? Click;

    public ButtonControl(
        Rectangle bounds = default,
        string? text = default,
        Styles buttonStyle = Styles.PushButton,
        WindowStyles style = WindowStyles.Overlapped | WindowStyles.Child | WindowStyles.Visible,
        ExtendedWindowStyles extendedStyle = ExtendedWindowStyles.Default,
        int buttonId = default,
        Window? parentWindow = default,
        nint parameters = default) : base(
            bounds,
            text,
            style |= (WindowStyles)buttonStyle,
            extendedStyle,
            parentWindow,
            s_buttonClass,
            parameters,
            (HMENU)buttonId)
    {
        ApplyApplicationTheme();
    }

    /// <summary>Gets or sets the check state of a check box, radio button, or three-state button.</summary>
    public ButtonCheckState CheckState
    {
        get => (ButtonCheckState)(uint)(int)this.SendMessage((MessageType)PInvoke.BM_GETCHECK);
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)value, (uint)ButtonCheckState.Indeterminate);
            this.SendMessage((MessageType)PInvoke.BM_SETCHECK, (WPARAM)(uint)value);
        }
    }

    /// <inheritdoc/>
    protected override void OnCommand(int controlId, int notificationCode)
    {
        base.OnCommand(controlId, notificationCode);
        if ((uint)notificationCode == Interop.BN_CLICKED)
        {
            OnClick();
            Click?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    protected override void OnColorModeChanged()
    {
        ApplyApplicationTheme();
        base.OnColorModeChanged();
    }

    private void ApplyApplicationTheme()
        => ApplyApplicationDarkModeTheme(
            OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26200)
                ? "DarkMode_DarkTheme"
                : "DarkMode_Explorer");

    /// <summary>Raises the <see cref="Click"/> event.</summary>
    protected virtual void OnClick()
    {
    }
}