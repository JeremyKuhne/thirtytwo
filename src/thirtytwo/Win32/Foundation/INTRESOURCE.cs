// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public readonly unsafe struct INTRESOURCE(ushort integer)
{
    // How can I tell that somebody used the MAKEINTRESOURCE macro to smuggle an integer inside a pointer?
    // https://devblogs.microsoft.com/oldnewthing/20130925-00/?p=3123

    public nint Value { get; } = integer;

    public static bool IsIntResource(nint value) => ((ulong)value) >> 16 == 0;
    public static bool IsIntResource(void* value) => ((ulong)value) >> 16 == 0;
}