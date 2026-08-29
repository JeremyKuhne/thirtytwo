// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents an unsigned pointer-sized Win32 message parameter (<c>WPARAM</c>).
/// </summary>
public unsafe readonly partial struct WPARAM
{
    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to an unmanaged pointer.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted pointer value.</returns>
    public static implicit operator void*(WPARAM value) => (void*)value.Value;

    /// <summary>
    ///  Converts an unmanaged pointer to a <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The pointer to convert.</param>
    /// <returns>The converted message parameter.</returns>
    public static implicit operator WPARAM(void* value) => new((nuint)value);

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to an <see cref="HWND"/> handle.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted window handle.</returns>
    public static explicit operator HWND(WPARAM value) => (HWND)(nint)value.Value;

    /// <summary>
    ///  Converts an <see cref="HWND"/> handle to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source window handle.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(HWND value) => new((nuint)value.Value);

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to an <see cref="HDC"/> handle.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted device-context handle.</returns>
    public static explicit operator HDC(WPARAM value) => (HDC)(nint)value.Value;

    /// <summary>
    ///  Converts an <see cref="HDC"/> handle to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source device-context handle.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(HDC value) => new((nuint)value.Value);

    /// <summary>
    ///  Converts an <see cref="HFONT"/> handle to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source font handle.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(HFONT value) => new((nuint)value.Value);

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to a Win32 boolean.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted boolean value.</returns>
    public static explicit operator BOOL(WPARAM value) => (BOOL)(nint)value.Value;

    /// <summary>
    ///  Converts a Win32 boolean to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source boolean value.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(BOOL value) => new((nuint)(nint)value);

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to a signed 32-bit integer.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted signed integer.</returns>
    public static explicit operator int(WPARAM value) => (int)(nint)value.Value;

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted unsigned integer.</returns>
    public static explicit operator uint(WPARAM value) => (uint)value.Value;

    /// <summary>
    ///  Converts a <see cref="WPARAM"/> to a signed pointer-sized integer.
    /// </summary>
    /// <param name="value">The source message parameter.</param>
    /// <returns>The converted signed pointer-sized integer.</returns>
    public static explicit operator nint(WPARAM value) => (nint)value.Value;

    /// <summary>
    ///  Converts a signed 32-bit integer to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source signed integer.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(int value) => new((nuint)(nint)value);

    /// <summary>
    ///  Converts a UTF-16 code unit to <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The character to convert.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(char value) => new((ushort)value);

    /// <summary>
    ///  Converts a managed color to the Win32 color integer carried in <see cref="WPARAM"/>.
    /// </summary>
    /// <param name="value">The source color.</param>
    /// <returns>The converted message parameter.</returns>
    public static explicit operator WPARAM(Color value) => new((nuint)ColorTranslator.ToWin32(value));

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

    // #define MAKEWPARAM(l, h)    ((WPARAM)(DWORD)MAKELONG(l, h))
    // #define MAKELONG(a, b)      ((LONG)(((WORD)(((DWORD_PTR)(a)) & 0xffff))
    //   | ((DWORD)((WORD)(((DWORD_PTR)(b)) & 0xffff))) << 16))
    /// <summary>
    ///  Packs two 16-bit values into a 32-bit <see cref="WPARAM"/> payload.
    /// </summary>
    /// <param name="low">The value placed in the low word.</param>
    /// <param name="high">The value placed in the high word.</param>
    /// <returns>The packed message parameter.</returns>
    public static WPARAM MAKEWPARAM(int low, int high) => (WPARAM)(uint)((int)(((ushort)(((nuint)low) & 0xffff))
| ((uint)((ushort)(((nuint)high) & 0xffff))) << 16));
}