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
            handler.Layout(_lastBounds);
        }
    }

    public void Layout(Rectangle bounds)
    {
        _lastBounds = bounds;
        Handler.Layout(bounds);
    }
}