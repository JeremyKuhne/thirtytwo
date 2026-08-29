// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents an integer value encoded using Win32 <c>MAKEINTRESOURCE</c> pointer conventions.
/// </summary>
/// <param name="integer">The integer value to encode as a resource pointer.</param>
public readonly unsafe struct INTRESOURCE(ushort integer)
{
    // How can I tell that somebody used the MAKEINTRESOURCE macro to smuggle an integer inside a pointer?
    // https://devblogs.microsoft.com/oldnewthing/20130925-00/?p=3123

    /// <summary>
    ///  Gets the encoded pointer-sized value.
    /// </summary>
    public nint Value { get; } = integer;

    /// <summary>
    ///  Determines whether a pointer-sized integer is encoded as an integer resource.
    /// </summary>
    /// <param name="value">The pointer-sized value to inspect.</param>
    /// <returns><see langword="true"/> when the high 16 bits are zero; otherwise, <see langword="false"/>.</returns>
    public static bool IsIntResource(nint value) => ((ulong)value) >> 16 == 0;

    /// <summary>
    ///  Determines whether an unmanaged pointer is encoded as an integer resource.
    /// </summary>
    /// <param name="value">The pointer value to inspect.</param>
    /// <returns><see langword="true"/> when the high 16 bits are zero; otherwise, <see langword="false"/>.</returns>
    public static bool IsIntResource(void* value) => ((ulong)value) >> 16 == 0;
}