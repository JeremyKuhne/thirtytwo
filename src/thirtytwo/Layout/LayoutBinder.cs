// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Binds a <see cref="Window"/> to an <see cref="ILayoutHandler"/> and listens for window position and DPI changes
///  to trigger layout updates.
/// </summary>
/// <remarks>
///  <para>
///   Construction performs an initial layout. Each binder is independent; multiple binders attached to the same
///   window receive the same notifications. Dispose a binder when its layout should stop before the window's
///   lifetime ends; otherwise the window's event subscription retains it for the window lifetime.
///  </para>
/// </remarks>
public class LayoutBinder : DisposableBase
{
    private readonly Window _window;
    private readonly ILayoutHandler _handler;
    private Rectangle _lastBounds;
    private float _lastScale;
    private bool _hasLayout;
    private Rectangle _activeBounds;
    private float _activeScale;
    private bool _layoutInProgress;
    private long _layoutVersion;

    /// <summary>
    ///  Initializes a new instance of the <see cref="LayoutBinder"/> class and attaches the layout handler
    ///  to the specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The window to bind to layout changes.</param>
    /// <param name="handler">The layout handler to invoke on layout events.</param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="window"/> or <paramref name="handler"/> is null.
    /// </exception>
    public LayoutBinder(Window window, ILayoutHandler handler)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(handler);

        _window = window;
        _handler = handler;
        window.MessageHandler += WindowMessageHandler;

        try
        {
            LayoutIfChanged();
        }
        catch
        {
            window.MessageHandler -= WindowMessageHandler;
            throw;
        }
    }

    private LRESULT? WindowMessageHandler(
        object sender,
        HWND window,
        MessageType message,
        WPARAM wParam,
        LPARAM lParam)
    {
        // A top-level DPI change applies suggested bounds and produces WindowPositionChanged. A child
        // DpiChangedAfterParent notification has no suggested move, so it must trigger layout directly.
        if (message == MessageType.WindowPositionChanged || message == MessageType.DpiChangedAfterParent)
        {
            LayoutIfChanged();
        }

        // Return null to indicate that the message was not handled.
        return null;
    }

    private void LayoutIfChanged()
    {
        if (Disposed)
        {
            return;
        }

        Rectangle bounds = _window.GetClientRectangle();
        float scale = _window.GetScale();
        LayoutValidation.ValidateScale(scale);
        if (_hasLayout && bounds == _lastBounds && scale == _lastScale)
        {
            return;
        }

        if (_layoutInProgress && bounds == _activeBounds && scale == _activeScale)
        {
            return;
        }

        long layoutVersion = ++_layoutVersion;
        Rectangle previousActiveBounds = _activeBounds;
        float previousActiveScale = _activeScale;
        bool layoutWasInProgress = _layoutInProgress;
        _activeBounds = bounds;
        _activeScale = scale;
        _layoutInProgress = true;

        try
        {
            _handler.Layout(bounds, scale);

            if (_layoutVersion == layoutVersion)
            {
                _lastBounds = bounds;
                _lastScale = scale;
                _hasLayout = true;
            }
        }
        finally
        {
            _activeBounds = previousActiveBounds;
            _activeScale = previousActiveScale;
            _layoutInProgress = layoutWasInProgress;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _window.MessageHandler -= WindowMessageHandler;
        }
    }
}