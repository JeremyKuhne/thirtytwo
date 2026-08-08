// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.ExceptionServices;
using Microsoft.UI.Xaml.Controls;
using Windows.Win32.Foundation;
using WinUIColor = Windows.UI.Color;

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
    ///   controlled by <see cref="Color"/> and <see cref="IsAlphaEnabled"/> and is therefore rejected.
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