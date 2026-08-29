// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Represents a layout wrapper that forwards the full available bounds to another handler.
/// </summary>
/// <remarks>
///  <para>
///   This usually is not needed for top-level windows. <see cref="Window"/> already implements
///   <see cref="ILayoutHandler"/> and fills its available space by default.
///  </para>
/// </remarks>
/// <param name="handler">The child handler that receives the unmodified layout bounds.</param>
/// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
public class FillLayout(ILayoutHandler handler) : ILayoutHandler
{
    private readonly ILayoutHandler _handler = LayoutValidation.ValidateHandler(handler);

    /// <inheritdoc/>
    public void Layout(Rectangle bounds, float scale) => _handler.Layout(bounds, scale);
}