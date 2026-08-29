// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public readonly partial struct DeviceContext
{
    /// <summary>
    ///  Tracks how a <see cref="DeviceContext"/> instance should release its native handle.
    /// </summary>
    [Flags]
    private enum ContextState
    {
        /// <summary>
        ///  Release with <c>DeleteDC</c>.
        /// </summary>
        UseDelete           = 0b00000000_00000001,

        /// <summary>
        ///  Release with <c>ReleaseDC</c>.
        /// </summary>
        UseRelease          = 0b00000000_00000010,

        /// <summary>
        ///  Release with <c>EndPaint</c>.
        /// </summary>
        UseEndPaint         = 0b00000000_00000100,

        /// <summary>
        ///  Restore a saved DC state before release.
        /// </summary>
        RestoreDc           = 0b00000000_00001000,

        /// <summary>
        ///  Do not release the handle on dispose.
        /// </summary>
        DoNotRelease        = 0b00000000_00010000,
    }
}