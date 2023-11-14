// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public class ReplaceableLayout(ILayoutHandler handler) : ILayoutHandler
{
    public Rectangle _lastBounds;
    private ILayoutHandler _layoutHandler = handler;

    public ILayoutHandler Handler
    {
        get => _layoutHandler;
        set
        {
            _layoutHandler = value;
            _layoutHandler.Layout(_lastBounds);
        }
    }

    public void Layout(Rectangle bounds)
    {
        _lastBounds = bounds;
        Handler.Layout(bounds);
    }
}