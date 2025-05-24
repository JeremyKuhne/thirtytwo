// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Simple layout handler that fills the available space.
/// </summary>
/// <remarks>
///  <para>
///   This usually isn't needed. <see cref="Window"/> itself is an <see cref="ILayoutHandler"/>
///   and it fills the available space by default.
///  </para>
/// </remarks>
public class FillLayout(ILayoutHandler handler) : ILayoutHandler
{
    public void Layout(Rectangle bounds) => handler.Layout(bounds);
}