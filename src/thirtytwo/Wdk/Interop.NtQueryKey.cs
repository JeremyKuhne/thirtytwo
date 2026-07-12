// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Wdk.System.Registry;
using Windows.Win32.System.Registry;

namespace Windows.Wdk;

public static partial class Interop
{
    /// <summary>
    ///  The NtQueryKey routine provides information about the class of a registry key, and the number and sizes of its subkeys.
    /// </summary>
    /// <param name="Length">Specifies the size, in bytes, of the <paramref name="KeyInformation"/> buffer.</param>
    /// <param name="ResultLength">
    ///  Pointer to a variable that receives the size, in bytes, of the requested key information.
    /// </param>
    [DllImport("ntdll.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern unsafe NTSTATUS NtQueryKey(
        HKEY KeyHandle,
        KEY_INFORMATION_CLASS KeyInformationClass,
        void* KeyInformation,
        uint Length,
        uint* ResultLength);
}