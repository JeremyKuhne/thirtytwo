// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>Specifies how a text editor determines reading order.</summary>
public enum WinUITextReadingOrder
{
    /// <summary>Uses the platform default behavior.</summary>
    Default = 0,

    /// <summary>
    ///  Uses the editor's flow direction. WinUI aliases this value to <see cref="Default"/>, so getters return
    ///  <see cref="Default"/> after either value is assigned.
    /// </summary>
    UseFlowDirection = Default,

    /// <summary>Determines reading order from text content.</summary>
    DetectFromContent = 1
}