// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Represents a layout handler whose child handler can be replaced at runtime.
/// </summary>
/// <remarks>
///  <para>
///   This type stores the most recent layout bounds and scale. Assigning <see cref="Handler"/> immediately relays
///   layout using the last stored values so the new child can update without waiting for another layout pass.
///  </para>
/// </remarks>
/// <param name="handler">The initial child handler.</param>
/// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
public class ReplaceableLayout(ILayoutHandler handler) : ILayoutHandler
{
    private ILayoutHandler _handler = LayoutValidation.ValidateHandler(handler);
    private Rectangle _lastBounds;
    private float _lastScale = 1.0f;

    /// <summary>
    ///  Gets or sets the current child layout handler.
    /// </summary>
    /// <value>The handler currently used to process layout requests.</value>
    /// <remarks>
    ///  <para>
    ///   Setting this property immediately invokes layout on the assigned handler using the most recent bounds and
    ///   scale captured by <see cref="Layout(Rectangle, float)"/>.
    ///  </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
    public ILayoutHandler Handler
    {
        get => _handler;
        set
        {
            _handler = LayoutValidation.ValidateHandler(value);
            _handler.Layout(_lastBounds, _lastScale);
        }
    }

    /// <inheritdoc/>
    public void Layout(Rectangle bounds, float scale)
    {
        _lastBounds = bounds;
        _lastScale = scale;
        Handler.Layout(bounds, scale);
    }
}