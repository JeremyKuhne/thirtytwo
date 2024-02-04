// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.UI.WindowsAndMessaging;

public unsafe partial struct HCURSOR : IDisposable
{
    public static HCURSOR Invalid => new(-1);

    public static implicit operator HCURSOR(CursorId id) => Interop.LoadCursor(default, (PCWSTR)(char*)(uint)id);

    public SetScope SetCursorScope() => new(this);
    public HCURSOR SetCursor() => Interop.SetCursor(this);

    public void Dispose()
    {
        if (!IsNull)
        {
            Interop.DestroyCursor(this);
        }

        Unsafe.AsRef(in this) = default;
    }
}