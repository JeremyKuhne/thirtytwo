// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.Foundation;

#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
public unsafe partial struct PCWSTR
#pragma warning restore CA2231
{
    public bool IsNull => Value is null;

    public void LocalFree()
    {
        if (Value is not null)
        {
            Interop.LocalFree((HLOCAL)(nint)Value);
            Unsafe.AsRef(in this) = default;
        }
    }
}