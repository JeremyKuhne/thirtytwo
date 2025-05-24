// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  A layout handler that can be replaced at runtime.
/// </summary>
/// <param name="handler">The initial layout handler.</param>
public class ReplaceableLayout(ILayoutHandler handler) : ILayoutHandler
{
    private Rectangle _lastBounds;
    private float _lastScale = 1.0f;

    /// <summary>
    ///  Gets or sets the current layout handler. When set, immediately performs layout
    ///  with the last known bounds.
    /// </summary>
    public ILayoutHandler Handler
    {
        get => handler;
        set
        {
            handler = value;
            handler.Layout(_lastBounds, _lastScale);
        }
    }

    public void Layout(Rectangle bounds, float scale)
    {
        _lastBounds = bounds;
        _lastScale = scale;
        Handler.Layout(bounds, scale);
    }
}