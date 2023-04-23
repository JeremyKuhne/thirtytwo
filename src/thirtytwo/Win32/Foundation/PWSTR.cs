// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Foundation;

public unsafe partial struct PWSTR
{
    public bool IsNull => Value is null;

    public void LocalFree()
    {
        if (Value is not null)
        {
            Interop.LocalFree((nint)Value);
            Unsafe.AsRef(this) = default;
        }
    }
}