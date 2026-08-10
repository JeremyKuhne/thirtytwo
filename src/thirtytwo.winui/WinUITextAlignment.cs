// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies horizontal text alignment.</summary>
public enum WinUITextAlignment
{
    /// <summary>Centers text.</summary>
    Center = 0,

    /// <summary>Aligns text to the physical left edge.</summary>
    Left = 1,

    /// <summary>
    ///  Aligns text to the logical leading edge. WinUI aliases this value to <see cref="Left"/>, so getters return
    ///  <see cref="Left"/> after either value is assigned.
    /// </summary>
    Start = Left,

    /// <summary>Aligns text to the physical right edge.</summary>
    Right = 2,

    /// <summary>
    ///  Aligns text to the logical trailing edge. WinUI aliases this value to <see cref="Right"/>, so getters return
    ///  <see cref="Right"/> after either value is assigned.
    /// </summary>
    End = Right,

    /// <summary>Justifies text.</summary>
    Justify = 3,

    /// <summary>Determines alignment from text content.</summary>
    DetectFromContent = 4
}