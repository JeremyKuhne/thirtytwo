// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe partial struct HICON
{
    public static HICON Invalid => new(-1);

    public static implicit operator HICON(IconId id) => Interop.LoadIcon(default, (PCWSTR)(char*)(uint)id);
}