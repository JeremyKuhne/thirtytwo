// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.WinUI;

/// <summary>Provides a candidate-window bounds change.</summary>
public sealed class WinUICandidateWindowBoundsChangedEventArgs(RectangleF bounds) : EventArgs
{
    /// <summary>Gets the candidate-window bounds in editor coordinates.</summary>
    public RectangleF Bounds { get; } = bounds;
}