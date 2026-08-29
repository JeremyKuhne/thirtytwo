// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Wdk.System.Registry;
using Windows.Win32.System.Registry;

namespace Windows.Wdk;

/// <summary>
///  Provides WDK and NT native interop entry points used by this library.
/// </summary>
public static partial class Interop
{
    /// <summary>
    ///  The NtQueryKey routine provides information about the class of a registry key, and the number and sizes of its subkeys.
    /// </summary>
    /// <param name="KeyHandle">The open registry key handle to query.</param>
    /// <param name="KeyInformationClass">The kind of key information to return.</param>
    /// <param name="KeyInformation">
    ///  A caller-allocated output buffer that receives the requested key information structure.
    /// </param>
    /// <param name="Length">Specifies the size, in bytes, of the <paramref name="KeyInformation"/> buffer.</param>
    /// <param name="ResultLength">
    ///  Pointer to a variable that receives the size, in bytes, of the requested key information.
    /// </param>
    /// <returns>
    ///  An <see cref="NTSTATUS"/> code indicating success or the failure reason, such as an insufficient buffer.
    /// </returns>
    [DllImport("ntdll.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern unsafe NTSTATUS NtQueryKey(
        HKEY KeyHandle,
        KEY_INFORMATION_CLASS KeyInformationClass,
        void* KeyInformation,
        uint Length,
        uint* ResultLength);
}