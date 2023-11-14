// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

public class FixedSizeLayout(
    ILayoutHandler handler,
    Size size,
    VerticalAlignment verticalAlignment = VerticalAlignment.Center,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center) : ILayoutHandler
{
    private readonly ILayoutHandler _handler = handler;
    private readonly Size _size = size;
    private readonly VerticalAlignment _verticalAlignment = verticalAlignment;
    private readonly HorizontalAlignment _horizontalAlignment = horizontalAlignment;

    public void Layout(Rectangle bounds)
    {
        int x = _horizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.Left,
            HorizontalAlignment.Right => bounds.Right - _size.Width,
            HorizontalAlignment.Center => (bounds.Width - _size.Width) / 2,
            _ => bounds.Left,
        };

        int y = _verticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Top,
            VerticalAlignment.Bottom => bounds.Bottom - _size.Height,
            VerticalAlignment.Center => (bounds.Height - _size.Height) / 2,
            _ => bounds.Top,
        };

        _handler.Layout(new Rectangle(new Point(x, y), _size));
    }
}