// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public readonly partial struct DeviceContext
{
    [Flags]
    private enum ContextState
    {
        UseDelete           = 0b00000000_00000001,
        UseRelease          = 0b00000000_00000010,
        UseEndPaint         = 0b00000000_00000100,
        RestoreDc           = 0b00000000_00001000,
        DoNotRelease        = 0b00000000_00010000,
    }
}