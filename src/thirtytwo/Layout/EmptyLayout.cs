// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  A layout handler that does nothing. Useful for designating a space that has no content.
/// </summary>
public class EmptyLayout : ILayoutHandler
{
    public void Layout(Rectangle bounds, float scale) { }
    private EmptyLayout() { }
    public static EmptyLayout Instance { get; } = new EmptyLayout();
}