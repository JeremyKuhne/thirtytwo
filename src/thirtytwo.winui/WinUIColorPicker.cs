// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.ExceptionServices;
using Microsoft.UI.Xaml.Controls;
using Windows.Win32.Foundation;
using WinUIColor = Windows.UI.Color;
using XamlColorSpectrumComponents = Microsoft.UI.Xaml.Controls.ColorSpectrumComponents;
using XamlColorSpectrumShape = Microsoft.UI.Xaml.Controls.ColorSpectrumShape;
using XamlElementTheme = Microsoft.UI.Xaml.ElementTheme;
using XamlOrientation = Microsoft.UI.Xaml.Controls.Orientation;

namespace Windows.WinUI;

/// <summary>
///  Hosts a WinUI color picker and projects its common color contract through .NET types.
/// </summary>
public sealed class WinUIColorPicker : XamlHostControl
{
    private ColorPicker? _colorPicker;

    /// <summary>Creates a WinUI color picker attached to <paramref name="parentWindow"/>.</summary>
    /// <param name="bounds">The host bounds in parent-client pixels.</param>
    /// <param name="parentWindow">The managed parent window.</param>
    public WinUIColorPicker(Rectangle bounds, Window parentWindow)
        : base(bounds, parentWindow)
    {
        ColorPicker? colorPicker = null;
        try
        {
            colorPicker = new();
            colorPicker.ColorChanged += ColorPickerColorChanged;
            _colorPicker = colorPicker;
            Content = colorPicker;
        }
        catch (Exception constructionFailure)
        {
            ThrowAfterFailedConstruction(constructionFailure, colorPicker);
        }
    }

    /// <summary>Occurs when the selected color changes.</summary>
    public event EventHandler<WinUIColorChangedEventArgs>? ColorChanged;

    /// <summary>Gets the hosted WinUI color picker.</summary>
    /// <remarks>
    ///  <para>
    ///   The typed wrapper fixes its content during construction. Assigning another element would detach the picker
    ///   controlled by this wrapper and is therefore rejected.
    ///  </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is not the hosted color picker.</exception>
    public override Microsoft.UI.Xaml.UIElement? Content
    {
        get => base.Content;
        set
        {
            if (!ReferenceEquals(value, _colorPicker))
            {
                throw new InvalidOperationException("WinUIColorPicker content cannot be replaced.");
            }

            base.Content = value;
        }
    }

    /// <summary>Gets or sets the selected color.</summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own this control.</exception>
    /// <exception cref="ObjectDisposedException">The host has been disposed.</exception>
    public Color Color
    {
        get
        {
            VerifyUsable();
            return ToDrawingColor(_colorPicker!.Color);
        }
        set
        {
            VerifyUsable();
            _colorPicker!.Color = WinUIColor.FromArgb(value.A, value.R, value.G, value.B);
        }
    }

    /// <summary>Gets or sets whether the picker allows alpha-channel selection.</summary>
    /// <exception cref="InvalidOperationException">The calling thread does not own this control.</exception>
    /// <exception cref="ObjectDisposedException">The host has been disposed.</exception>
    public bool IsAlphaEnabled
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsAlphaEnabled;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsAlphaEnabled = value;
        }
    }

    /// <summary>Gets or sets whether the color spectrum is visible.</summary>
    public bool IsColorSpectrumVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsColorSpectrumVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsColorSpectrumVisible = value;
        }
    }

    /// <summary>Gets or sets whether the color preview bar is visible.</summary>
    public bool IsColorPreviewVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsColorPreviewVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsColorPreviewVisible = value;
        }
    }

    /// <summary>Gets or sets whether the color-value slider is visible.</summary>
    public bool IsColorSliderVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsColorSliderVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsColorSliderVisible = value;
        }
    }

    /// <summary>Gets or sets whether the color-channel text inputs are visible.</summary>
    public bool IsColorChannelTextInputVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsColorChannelTextInputVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsColorChannelTextInputVisible = value;
        }
    }

    /// <summary>Gets or sets whether the alpha slider is visible when alpha is enabled.</summary>
    public bool IsAlphaSliderVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsAlphaSliderVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsAlphaSliderVisible = value;
        }
    }

    /// <summary>Gets or sets whether the alpha text input is visible when alpha is enabled.</summary>
    public bool IsAlphaTextInputVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsAlphaTextInputVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsAlphaTextInputVisible = value;
        }
    }

    /// <summary>Gets or sets whether the hexadecimal color input is visible.</summary>
    public bool IsHexInputVisible
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.IsHexInputVisible;
        }
        set
        {
            VerifyUsable();
            _colorPicker!.IsHexInputVisible = value;
        }
    }

    /// <summary>Gets or sets the shape of the color spectrum.</summary>
    public WinUIColorSpectrumShape ColorSpectrumShape
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.ColorSpectrumShape switch
            {
                XamlColorSpectrumShape.Box => WinUIColorSpectrumShape.Box,
                XamlColorSpectrumShape.Ring => WinUIColorSpectrumShape.Ring,
                _ => throw new InvalidOperationException("The color picker returned an unknown spectrum shape.")
            };
        }
        set
        {
            VerifyUsable();
            _colorPicker!.ColorSpectrumShape = value switch
            {
                WinUIColorSpectrumShape.Box => XamlColorSpectrumShape.Box,
                WinUIColorSpectrumShape.Ring => XamlColorSpectrumShape.Ring,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown color spectrum shape.")
            };
        }
    }

    /// <summary>Gets or sets how HSV components map to the color spectrum axes.</summary>
    public WinUIColorSpectrumComponents ColorSpectrumComponents
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.ColorSpectrumComponents switch
            {
                XamlColorSpectrumComponents.HueSaturation => WinUIColorSpectrumComponents.HueSaturation,
                XamlColorSpectrumComponents.HueValue => WinUIColorSpectrumComponents.HueValue,
                XamlColorSpectrumComponents.SaturationHue => WinUIColorSpectrumComponents.SaturationHue,
                XamlColorSpectrumComponents.SaturationValue => WinUIColorSpectrumComponents.SaturationValue,
                XamlColorSpectrumComponents.ValueHue => WinUIColorSpectrumComponents.ValueHue,
                XamlColorSpectrumComponents.ValueSaturation => WinUIColorSpectrumComponents.ValueSaturation,
                _ => throw new InvalidOperationException("The color picker returned an unknown spectrum-component mapping.")
            };
        }
        set
        {
            VerifyUsable();
            _colorPicker!.ColorSpectrumComponents = value switch
            {
                WinUIColorSpectrumComponents.HueSaturation => XamlColorSpectrumComponents.HueSaturation,
                WinUIColorSpectrumComponents.HueValue => XamlColorSpectrumComponents.HueValue,
                WinUIColorSpectrumComponents.SaturationHue => XamlColorSpectrumComponents.SaturationHue,
                WinUIColorSpectrumComponents.SaturationValue => XamlColorSpectrumComponents.SaturationValue,
                WinUIColorSpectrumComponents.ValueHue => XamlColorSpectrumComponents.ValueHue,
                WinUIColorSpectrumComponents.ValueSaturation => XamlColorSpectrumComponents.ValueSaturation,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown color spectrum-component mapping.")
            };
        }
    }

    /// <summary>Gets or sets the orientation of the color picker's editing controls.</summary>
    public WinUIColorPickerOrientation Orientation
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.Orientation switch
            {
                XamlOrientation.Vertical => WinUIColorPickerOrientation.Vertical,
                XamlOrientation.Horizontal => WinUIColorPickerOrientation.Horizontal,
                _ => throw new InvalidOperationException("The color picker returned an unknown orientation.")
            };
        }
        set
        {
            VerifyUsable();
            _colorPicker!.Orientation = value switch
            {
                WinUIColorPickerOrientation.Vertical => XamlOrientation.Vertical,
                WinUIColorPickerOrientation.Horizontal => XamlOrientation.Horizontal,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown color picker orientation.")
            };
        }
    }

    /// <summary>Gets or sets the theme requested for the hosted color picker.</summary>
    public WinUIElementTheme RequestedTheme
    {
        get
        {
            VerifyUsable();
            return _colorPicker!.RequestedTheme switch
            {
                XamlElementTheme.Default => WinUIElementTheme.Default,
                XamlElementTheme.Light => WinUIElementTheme.Light,
                XamlElementTheme.Dark => WinUIElementTheme.Dark,
                _ => throw new InvalidOperationException("The color picker returned an unknown requested theme.")
            };
        }
        set
        {
            VerifyUsable();
            _colorPicker!.RequestedTheme = value switch
            {
                WinUIElementTheme.Default => XamlElementTheme.Default,
                WinUIElementTheme.Light => XamlElementTheme.Light,
                WinUIElementTheme.Dark => XamlElementTheme.Dark,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown WinUI element theme.")
            };
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing: false);
            return;
        }

        VerifyAccess();
        Exception? eventCleanupFailure = null;
        try
        {
            DetachColorPicker();
        }
        catch (Exception exception)
        {
            eventCleanupFailure = exception;
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception hostCleanupFailure) when (eventCleanupFailure is not null)
        {
            throw new AggregateException(eventCleanupFailure, hostCleanupFailure);
        }

        if (eventCleanupFailure is not null)
        {
            ExceptionDispatchInfo.Capture(eventCleanupFailure).Throw();
        }
    }

    /// <inheritdoc/>
    protected override LRESULT WindowProcedure(HWND window, MessageType message, WPARAM wParam, LPARAM lParam)
    {
        if (message == MessageType.Destroy)
        {
            try
            {
                DetachColorPicker();
            }
            catch (Exception exception)
            {
                ReportNativeCallbackFailure("ColorPickerDestroy", exception);
            }
        }

        return base.WindowProcedure(window, message, wParam, lParam);
    }

    private static Color ToDrawingColor(WinUIColor color)
        => Color.FromArgb(color.A, color.R, color.G, color.B);

    private void ColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs eventArgs)
        => ColorChanged?.Invoke(
            this,
            new(ToDrawingColor(eventArgs.OldColor), ToDrawingColor(eventArgs.NewColor)));

    private void DetachColorPicker()
    {
        ColorPicker? colorPicker = _colorPicker;
        if (colorPicker is null)
        {
            return;
        }

        _colorPicker = null;
        colorPicker.ColorChanged -= ColorPickerColorChanged;
    }

    [DoesNotReturn]
    private void ThrowAfterFailedConstruction(Exception constructionFailure, ColorPicker? colorPicker)
    {
        List<Exception>? cleanupFailures = null;
        try
        {
            if (colorPicker is not null)
            {
                colorPicker.ColorChanged -= ColorPickerColorChanged;
            }
        }
        catch (Exception eventCleanupFailure)
        {
            cleanupFailures = [eventCleanupFailure];
        }

        try
        {
            base.Dispose(disposing: true);
        }
        catch (Exception hostCleanupFailure)
        {
            (cleanupFailures ??= []).Add(hostCleanupFailure);
        }

        if (cleanupFailures is not null)
        {
            cleanupFailures.Insert(0, constructionFailure);
            throw new AggregateException("WinUI color picker construction and cleanup failed.", cleanupFailures);
        }

        ExceptionDispatchInfo.Capture(constructionFailure).Throw();
        throw new UnreachableException();
    }

    private void VerifyUsable()
    {
        VerifyAccess();
        ObjectDisposedException.ThrowIf(IsXamlSourceDisposed, this);
        ObjectDisposedException.ThrowIf(_colorPicker is null, this);
    }
}