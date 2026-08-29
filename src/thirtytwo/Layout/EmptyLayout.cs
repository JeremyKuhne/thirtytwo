// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows;

/// <summary>
///  Represents a layout handler that intentionally performs no layout work.
/// </summary>
/// <remarks>
///  <para>
///   Use this handler when a layout slot must be present but should not position or size any child content.
///   The provided bounds and scale are ignored.
///  </para>
/// </remarks>
public class EmptyLayout : ILayoutHandler
{
    /// <inheritdoc/>
    public void Layout(Rectangle bounds, float scale) { }

    private EmptyLayout() { }

    /// <summary>
    ///  Gets the shared singleton instance.
    /// </summary>
    /// <value>A reusable <see cref="EmptyLayout"/> instance.</value>
    public static EmptyLayout Instance { get; } = new EmptyLayout();
}