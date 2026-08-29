// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a signed pointer-sized Win32 window-procedure result (<c>LRESULT</c>).
/// </summary>
public unsafe readonly partial struct LRESULT
{
    /// <summary>
    ///  Converts an <see cref="LRESULT"/> to an unmanaged pointer.
    /// </summary>
    /// <param name="value">The source value.</param>
    /// <returns>The converted pointer value.</returns>
    public static explicit operator void*(LRESULT value) => (void*)value.Value;

    /// <summary>
    ///  Converts an unmanaged pointer to an <see cref="LRESULT"/>.
    /// </summary>
    /// <param name="value">The pointer to convert.</param>
    /// <returns>The converted result value.</returns>
    public static explicit operator LRESULT(void* value) => new((nint)value);

    // #define HIWORD(l)           ((WORD)((((DWORD_PTR)(l)) >> 16) & 0xffff))
    /// <summary>
    ///  Gets the high 16 bits of the value as an unsigned word.
    /// </summary>
    public ushort HIWORD => (ushort)((Value >> 16) & 0xffff);

    /// <summary>
    ///  Gets the high 16 bits of the value as a signed word.
    /// </summary>
    public short SIGNEDHIWORD => (short)HIWORD;

    // #define LOWORD(l)           ((WORD)(((DWORD_PTR)(l)) & 0xffff))
    /// <summary>
    ///  Gets the low 16 bits of the value as an unsigned word.
    /// </summary>
    public ushort LOWORD => (ushort)(Value & 0xffff);

    /// <summary>
    ///  Gets the low 16 bits of the value as a signed word.
    /// </summary>
    public short SIGNEDLOWORD => (short)LOWORD;
}