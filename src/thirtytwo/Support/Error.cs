// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Support;

public static class Win32ErrorExtensions
{
    extension(WIN32_ERROR error)
    {
        /// <summary>
        ///  Throws the error using thirtytwo's exception mapping.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public void ThrowThirtyTwoException(string? path = null)
            => throw error.GetThirtyTwoException(path);

        /// <summary>
        ///  Throws the error using thirtytwo's exception mapping when it is not successful.
        /// </summary>
        public void ThrowIfThirtyTwoFailed(string? path = null)
        {
            if (error != WIN32_ERROR.ERROR_SUCCESS)
            {
                error.ThrowThirtyTwoException(path);
            }
        }

        /// <summary>
        ///  Creates the exception that corresponds to the error using thirtytwo's exception mapping.
        /// </summary>
        public Exception GetThirtyTwoException(string? path = null)
        {
            string message = path is null
                ? error.ErrorToString()
                : $"{error.ErrorToString()} '{path}'";

            return WindowsErrorToException(error, message, path);
        }

        /// <summary>
        ///  Throws the last Windows error when it is not this expected value.
        /// </summary>
        public void ThrowIfLastErrorNot(string? path = null)
        {
            WIN32_ERROR lastError = Error.GetLastError();
            if (lastError != error)
            {
                lastError.ThrowThirtyTwoException(path);
            }
        }
    }

    extension(bool result)
    {
        /// <summary>
        ///  Throws the last Windows error using thirtytwo's exception mapping when the result is false.
        /// </summary>
        internal void ThrowLastErrorIfFalse(string? path = null)
        {
            if (!result)
            {
                Error.GetLastError().ThrowThirtyTwoException(path);
            }
        }
    }

    extension(BOOL result)
    {
        /// <summary>
        ///  Throws the last Windows error using thirtytwo's exception mapping when the result is false.
        /// </summary>
        internal void ThrowLastErrorIfFalse(string? path = null)
        {
            if (!result)
            {
                Error.GetLastError().ThrowThirtyTwoException(path);
            }
        }
    }

    private static Exception WindowsErrorToException(WIN32_ERROR error, string? message, string? path)
    {
        switch (error)
        {
            case WIN32_ERROR.ERROR_FILE_NOT_FOUND:
                return new FileNotFoundException(message, path);
            case WIN32_ERROR.ERROR_PATH_NOT_FOUND:
                return new DirectoryNotFoundException(message);
            case WIN32_ERROR.ERROR_ACCESS_DENIED:
            // Network access doesn't throw UnauthorizedAccess in .NET
            case WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED:
                return new UnauthorizedAccessException(message);
            case WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE:
                return new PathTooLongException(message);
            case WIN32_ERROR.ERROR_INVALID_DRIVE:
                // Not available in Portable libraries
                return new DriveNotFoundException(message);
            case WIN32_ERROR.ERROR_OPERATION_ABORTED:
            case WIN32_ERROR.ERROR_CANCELLED:
                return new OperationCanceledException(message);
            case WIN32_ERROR.ERROR_NOT_READY:
                return new DriveNotReadyException(message);
            case WIN32_ERROR.ERROR_FILE_EXISTS:
            case WIN32_ERROR.ERROR_ALREADY_EXISTS:
                return new FileExistsException(error, message);
            case WIN32_ERROR.ERROR_INVALID_PARAMETER:
                return new ArgumentException(message);
            case WIN32_ERROR.ERROR_NOT_SUPPORTED:
            case WIN32_ERROR.ERROR_NOT_SUPPORTED_IN_APPCONTAINER:
                return new NotSupportedException(message);
            case WIN32_ERROR.ERROR_SHARING_VIOLATION:
            default:
                if (error == (WIN32_ERROR)(int)HRESULT.FVE_E_LOCKED_VOLUME)
                {
                    return new DriveLockedException(message);
                }

                return new ThirtyTwoException(error, message);
        }
    }
}