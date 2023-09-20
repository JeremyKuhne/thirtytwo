// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public static class CATID
{
    /// <summary>
    ///  ActiveX control category id.
    /// </summary>
    public static Guid Control { get; }
        // 40fc6ed4-2438-11cf-a3db-080036f12502
        = new(0x40fc6ed4, 0x2438, 0x11cf, 0xa3, 0xdb, 0x08, 0x00, 0x36, 0xf1, 0x25, 0x02);
}