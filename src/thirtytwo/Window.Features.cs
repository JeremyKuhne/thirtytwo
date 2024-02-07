// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public unsafe partial class Window
{
    [Flags]
    public enum Features
    {
        /// <summary>
        ///  Set this flag to enable Direct2D rendering.
        /// </summary>
        EnableDirect2d          = 0b0000_0000_0000_0000_0000_0000_0000_0001,
    }
}