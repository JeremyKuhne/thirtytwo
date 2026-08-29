// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a Win32 atom value that can be used directly or encoded as an integer resource pointer.
/// </summary>
/// <param name="atom">The raw 16-bit atom value.</param>
public unsafe readonly struct ATOM(ushort atom)
{
    // #define MAXINTATOM 0xC000
    // #define MAKEINTATOM(i)  (LPTSTR)((ULONG_PTR)((WORD)(i)))
    // #define INVALID_ATOM ((ATOM)0)

    // "Strange uses for window class atoms"
    // https://devblogs.microsoft.com/oldnewthing/20080501-00/?p=22503

    /// <summary>
    ///  Gets the raw 16-bit atom value.
    /// </summary>
    public ushort Value { get; } = atom;

    /// <summary>
    ///  Gets the sentinel atom value <c>0</c>.
    /// </summary>
    public static ATOM Null { get; } = new(0);

    /// <summary>
    ///  Gets a value indicating whether this atom is nonzero.
    /// </summary>
    public bool IsValid => Value != 0;

    /// <summary>
    ///  Determines whether a pointer-sized value uses the integer-resource encoding accepted by Win32 atom APIs.
    /// </summary>
    /// <param name="pointer">The pointer-sized value to test.</param>
    /// <returns>
    ///  <see langword="true"/> when the value is nonzero and its high 16 bits are zero; otherwise,
    ///  <see langword="false"/>.
    /// </returns>
    public static bool IsATOM(nint pointer)
    {
        // While MAXINTATOM is defined at 0xC000, this is not actually the maximum.
        // Any INTRESOURCE value is possible.

        // IS_INTRESOURCE(_r) ((((ULONG_PTR)(_r)) >> 16) == 0)
        ulong value = (ulong)pointer;
        return value != 0 && value >> 16 == 0;
    }

    /// <summary>
    ///  Converts an <see cref="ATOM"/> to a pointer-sized integer preserving the raw 16-bit value.
    /// </summary>
    /// <param name="atom">The atom value to convert.</param>
    /// <returns>The converted pointer-sized integer value.</returns>
    public static implicit operator nint(ATOM atom) => atom.Value;

    /// <summary>
    ///  Converts a 16-bit value to an <see cref="ATOM"/>.
    /// </summary>
    /// <param name="atom">The raw atom value.</param>
    /// <returns>An <see cref="ATOM"/> wrapping <paramref name="atom"/>.</returns>
    public static implicit operator ATOM(ushort atom) => new(atom);

    /// <summary>
    ///  Converts an <see cref="ATOM"/> to its raw 16-bit value.
    /// </summary>
    /// <param name="atom">The atom value to convert.</param>
    /// <returns>The raw 16-bit atom value.</returns>
    public static implicit operator ushort(ATOM atom) => atom.Value;

    /// <summary>
    ///  Converts an <see cref="ATOM"/> to a <see cref="PCWSTR"/> using integer-resource pointer encoding.
    /// </summary>
    /// <param name="atom">The atom value to encode.</param>
    /// <returns>A pointer whose low word contains the atom value.</returns>
    public static implicit operator PCWSTR(ATOM atom) => (PCWSTR)(char*)atom.Value;
}