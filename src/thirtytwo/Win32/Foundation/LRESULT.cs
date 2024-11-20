// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe readonly partial struct LRESULT
{
    public static explicit operator void*(LRESULT value) => (void*)value.Value;
    public static explicit operator LRESULT(void* value) => new((nint)value);
}