// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Foundation;

public partial struct NTSTATUS
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIfFailed()
    {
        if (SeverityCode is Severity.Error or Severity.Warning)
        {
            throw this;
        }
    }

    /// <summary>
    ///  Turns Windows errors into the appropriate exception (that maps with existing .NET behavior as much as possible).
    /// </summary>
    public Exception GetException(string? path = null)
    {
        WIN32_ERROR error = (WIN32_ERROR)this;

        string message = path is null
            ? $"{Error.ErrorToString(error)} {{NTSTATUS: {Value:x8}}}"
            : $"{Error.ErrorToString(error)} {{NTSTATUS: {Value:x8}}} '{path}'";

        return Error.WindowsErrorToException(error, message, path);
    }

    public static implicit operator Exception(NTSTATUS result) => result.GetException();

    public static explicit operator WIN32_ERROR(NTSTATUS status)
    {
        // LsaNtStatusToWinError is another entry point to RtlNtStatusToDosError
        return (WIN32_ERROR)Interop.LsaNtStatusToWinError(status);
    }
}