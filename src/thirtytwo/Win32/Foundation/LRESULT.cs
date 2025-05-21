// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public unsafe readonly partial struct LRESULT
{
    public static explicit operator void*(LRESULT value) => (void*)value.Value;
    public static explicit operator LRESULT(void* value) => new((nint)value);

    // #define HIWORD(l)           ((WORD)((((DWORD_PTR)(l)) >> 16) & 0xffff))
    public ushort HIWORD => (ushort)((Value >> 16) & 0xffff);
    public short SIGNEDHIWORD => (short)HIWORD;

    // #define LOWORD(l)           ((WORD)(((DWORD_PTR)(l)) & 0xffff))
    public ushort LOWORD => (ushort)(Value & 0xffff);
    public short SIGNEDLOWORD => (short)LOWORD;
}