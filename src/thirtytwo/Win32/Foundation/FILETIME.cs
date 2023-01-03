// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe partial struct FILETIME
{
    public static explicit operator FILETIME(DateTime value)
    {
        long ft = value.ToFileTime();

        return new FILETIME()
        {
            dwLowDateTime = (uint)(ft & 0xFFFFFFFF),
            dwHighDateTime = (uint)(ft >> 32)
        };
    }

    public static explicit operator DateTime(FILETIME value)
        => DateTime.FromFileTime(((long)value.dwHighDateTime << 32) + value.dwLowDateTime);
}
