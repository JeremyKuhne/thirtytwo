// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

public readonly struct ATOM
{
    // #define MAXINTATOM 0xC000
    // #define MAKEINTATOM(i)  (LPTSTR)((ULONG_PTR)((WORD)(i)))
    // #define INVALID_ATOM ((ATOM)0)

    // Strange uses for window class atoms
    // https://devblogs.microsoft.com/oldnewthing/20080501-00/?p=22503

    public ushort Value { get; }

    public ATOM(ushort atom) => Value = atom;

    public static ATOM Null { get; } = new(0);

    public bool IsValid => Value != 0;

    public static bool IsATOM(nint pointer)
    {
        // While MAXINTATOM is defined at 0xC000, this is not actually the maximum.
        // Any INTRESOURCE value is possible.

        // IS_INTRESOURCE(_r) ((((ULONG_PTR)(_r)) >> 16) == 0)
        ulong value = (ulong)pointer;
        return value != 0 && value >> 16 == 0;
    }

    public static implicit operator nint(ATOM atom) => atom.Value;
    public static implicit operator ATOM(ushort atom) => new(atom);
    public static implicit operator ushort(ATOM atom) => atom.Value;
}
