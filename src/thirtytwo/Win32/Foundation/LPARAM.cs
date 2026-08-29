// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a signed pointer-sized Win32 message parameter (<c>LPARAM</c>).
/// </summary>
public readonly partial struct LPARAM
{
    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to an unmanaged pointer.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted pointer value.</returns>
    public static unsafe implicit operator void*(LPARAM value) => (void*)value.Value;

    /// <summary>
    ///  Converts an unmanaged pointer to an <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The pointer value to convert.</param>
    /// <returns>The converted <see cref="LPARAM"/>.</returns>
    public static unsafe implicit operator LPARAM(void* value) => new((nint)value);

    /// <summary>
    ///  Converts a Win32 boolean to an <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The Win32 boolean value.</param>
    /// <returns>The converted <see cref="LPARAM"/>.</returns>
    public static explicit operator LPARAM(BOOL value) => new((nint)value);

    /// <summary>
    ///  Converts a 32-bit signed integer to an <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The source integer value.</param>
    /// <returns>The converted <see cref="LPARAM"/>.</returns>
    public static implicit operator LPARAM(int value) => new((nint)value);

    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to a 32-bit signed integer.
    /// </summary>
    /// <param name="value">The source <see cref="LPARAM"/>.</param>
    /// <returns>The low 32-bit signed integer representation.</returns>
    public static explicit operator int(LPARAM value) => (int)value.Value;

    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to a 32-bit unsigned integer via signed 32-bit truncation.
    /// </summary>
    /// <param name="value">The source <see cref="LPARAM"/>.</param>
    /// <returns>The converted 32-bit unsigned value.</returns>
    public static explicit operator uint(LPARAM value) => (uint)(int)value.Value;

    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to an unsigned pointer-sized integer.
    /// </summary>
    /// <param name="value">The source <see cref="LPARAM"/>.</param>
    /// <returns>The converted unsigned pointer-sized value.</returns>
    public static explicit operator nuint(LPARAM value) => (nuint)value.Value;

    /// <summary>
    ///  Converts a 32-bit unsigned integer to an <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The source unsigned integer.</param>
    /// <returns>The converted <see cref="LPARAM"/>.</returns>
    public static explicit operator LPARAM(uint value) => new((nint)(nuint)value);

    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to an <see cref="HWND"/> handle.
    /// </summary>
    /// <param name="value">The source <see cref="LPARAM"/>.</param>
    /// <returns>The converted window handle.</returns>
    public static explicit operator HWND(LPARAM value) => (HWND)value.Value;

    /// <summary>
    ///  Converts an <see cref="HWND"/> handle to an <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The source window handle.</param>
    /// <returns>The converted <see cref="LPARAM"/>.</returns>
    public static unsafe explicit operator LPARAM(HWND value) => new((nint)value.Value);

    /// <summary>
    ///  Converts a managed color to the Win32 color integer carried in <see cref="LPARAM"/>.
    /// </summary>
    /// <param name="value">The source color.</param>
    /// <returns>The converted <see cref="LPARAM"/> value.</returns>
    public static explicit operator LPARAM(Color value) => (LPARAM)ColorTranslator.ToWin32(value);

    /// <summary>
    ///  Converts an <see cref="LPARAM"/> to a <see cref="Point"/> using signed low/high words.
    /// </summary>
    /// <param name="value">The source <see cref="LPARAM"/>.</param>
    /// <returns>A point composed from <see cref="SIGNEDLOWORD"/> and <see cref="SIGNEDHIWORD"/>.</returns>
    public static explicit operator Point(LPARAM value) => new(value.SIGNEDLOWORD, value.SIGNEDHIWORD);

    /// <summary>
    ///  Packs a point into an <see cref="LPARAM"/> using <see cref="MAKELPARAM(int, int)"/>.
    /// </summary>
    /// <param name="value">The point to pack.</param>
    /// <returns>The packed <see cref="LPARAM"/>.</returns>
    public static explicit operator LPARAM(Point value) => MAKELPARAM(value.X, value.Y);

    // #define HIWORD(l)           ((WORD)((((DWORD_PTR)(l)) >> 16) & 0xffff))
    /// <summary>
    ///  Gets the high 16 bits of the value as an unsigned word.
    /// </summary>
    public ushort HIWORD => (ushort)((((nuint)Value) >> 16) & 0xffff);

    /// <summary>
    ///  Gets the high 16 bits of the value as a signed word.
    /// </summary>
    public short SIGNEDHIWORD => (short)HIWORD;

    // #define LOWORD(l)           ((WORD)(((DWORD_PTR)(l)) & 0xffff))
    /// <summary>
    ///  Gets the low 16 bits of the value as an unsigned word.
    /// </summary>
    public ushort LOWORD => (ushort)(((nuint)Value) & 0xffff);

    /// <summary>
    ///  Gets the low 16 bits of the value as a signed word.
    /// </summary>
    public short SIGNEDLOWORD => (short)LOWORD;

    // #define MAKELPARAM(l, h)    ((LPARAM)(DWORD)MAKELONG(l, h))
    // #define MAKELONG(a, b)      ((LONG)(((WORD)(((DWORD_PTR)(a)) & 0xffff))
    //   | ((DWORD)((WORD)(((DWORD_PTR)(b)) & 0xffff))) << 16))
    /// <summary>
    ///  Packs two 16-bit values into a 32-bit <see cref="LPARAM"/> payload.
    /// </summary>
    /// <param name="low">The value placed in the low word.</param>
    /// <param name="high">The value placed in the high word.</param>
    /// <returns>The packed parameter value.</returns>
    public static LPARAM MAKELPARAM(int low, int high) => (LPARAM)(uint)((int)(((ushort)(((nuint)low) & 0xffff))
        | ((uint)((ushort)(((nuint)high) & 0xffff))) << 16));
}