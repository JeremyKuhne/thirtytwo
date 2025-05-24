// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  An interface for layout handlers.
/// </summary>
public interface ILayoutHandler
{
    /// <summary>
    ///  Layout the control using the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to layout within.</param>
    void Layout(Rectangle bounds);
}