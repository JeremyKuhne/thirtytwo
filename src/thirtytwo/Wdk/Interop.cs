// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Windows.Wdk.System.SystemServices;
using Windows.Win32.System.Registry;

namespace Windows.Wdk;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

public static partial class Interop
{
    /// <summary>
    ///  The NtQueryKey routine provides information about the class of a registry key, and the number and sizes of its subkeys.
    /// </summary>
    /// <param name="Length">Specifies the size, in bytes, of the <paramref name="KeyInformation"/> buffer.</param>
    /// <param name="ResultLength">
    ///  Pointer to a variable that receives the size, in bytes, of the requested key information. If NtQueryKey returns
    ///  <see cref="NTSTATUS.STATUS_SUCCESS"/>, the variable contains the amount of data returned. If NwQueryKey returns
    ///  <see cref="NTSTATUS.STATUS_BUFFER_OVERFLOW"/> or <see cref="NTSTATUS.STATUS_BUFFER_TOO_SMALL"/>, you can use the
    ///  value of the variable to determine the required buffer size.
    /// </param>
    /// <remarks>
    ///  <para>
    ///   <see href="https://learn.microsoft.com/windows-hardware/drivers/ddi/wdm/nf-wdm-zwquerykey">Read more on learn.microsoft.com</see>.
    ///  </para>
    /// </remarks>
    [DllImport("ntdll.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern unsafe NTSTATUS NtQueryKey(
        HKEY KeyHandle,
        KEY_INFORMATION_CLASS KeyInformationClass,
        void* KeyInformation,
        uint Length,
        uint* ResultLength);
}

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter