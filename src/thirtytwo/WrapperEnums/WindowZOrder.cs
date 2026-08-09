// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>Identifies a special native z-order position for <c>SetWindowPos</c>.</summary>
public enum WindowZOrder
{
    /// <summary>Places the window at the top of z-order.</summary>
    Top,

    /// <summary>Places the window at the bottom of z-order and removes topmost status.</summary>
    Bottom,

    /// <summary>Places the window above all non-topmost windows and retains that position when deactivated.</summary>
    TopMost,

    /// <summary>Removes topmost status and places the window behind all topmost windows.</summary>
    NotTopMost
}