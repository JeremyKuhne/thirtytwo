// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Support;

/// <summary>
///  Provides helpers for converting Win32 call results into mapped managed exceptions.
/// </summary>
public static class Win32ErrorExtensions
{
    /// <summary>
    ///  Provides helpers for interpreting and throwing from a Win32 error value.
    /// </summary>
    /// <param name="error">The Win32 error value that extension members operate on.</param>
    extension(WIN32_ERROR error)
    {
        /// <summary>
        ///  Throws an exception mapped from this Win32 error.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <exception cref="Exception">Always thrown. The concrete type depends on the Win32 error value.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public void ThrowThirtyTwoException(string? path = null)
            => throw error.GetThirtyTwoException(path);

        /// <summary>
        ///  Throws an exception mapped from this Win32 error when it is not <see cref="WIN32_ERROR.ERROR_SUCCESS"/>.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <exception cref="Exception">
        ///  Thrown when this error value is not success. The concrete type depends on the Win32 error value.
        /// </exception>
        public void ThrowIfThirtyTwoFailed(string? path = null)
        {
            if (error != WIN32_ERROR.ERROR_SUCCESS)
            {
                error.ThrowThirtyTwoException(path);
            }
        }

        /// <summary>
        ///  Creates the exception that corresponds to this Win32 error using thirtytwo's mapping rules.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <returns>The mapped exception instance.</returns>
        public Exception GetThirtyTwoException(string? path = null)
        {
            string message = path is null
                ? error.ErrorToString()
                : $"{error.ErrorToString()} '{path}'";

            return WindowsErrorToException(error, message, path);
        }

        /// <summary>
        ///  Throws the current thread's last Win32 error when it does not match this expected error.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <exception cref="Exception">
        ///  Thrown when the thread's last Win32 error differs from this expected error.
        /// </exception>
        public void ThrowIfLastErrorNot(string? path = null)
        {
            WIN32_ERROR lastError = Error.GetLastError();
            if (lastError != error)
            {
                lastError.ThrowThirtyTwoException(path);
            }
        }
    }

    /// <summary>
    ///  Provides helpers that validate Win32 Boolean call results.
    /// </summary>
    /// <param name="result">The Boolean result returned from a Win32 API call.</param>
    extension(bool result)
    {
        /// <summary>
        ///  Throws the thread's last Win32 error using thirtytwo's mapping when the result is false.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <exception cref="Exception">Thrown when <c>result</c> is <see langword="false"/>.</exception>
        internal void ThrowLastErrorIfFalse(string? path = null)
        {
            if (!result)
            {
                Error.GetLastError().ThrowThirtyTwoException(path);
            }
        }
    }

    /// <summary>
    ///  Provides helpers that validate Win32 BOOL call results.
    /// </summary>
    /// <param name="result">The BOOL result returned from a Win32 API call.</param>
    extension(BOOL result)
    {
        /// <summary>
        ///  Throws the thread's last Win32 error using thirtytwo's mapping when the result is false.
        /// </summary>
        /// <param name="path">Optional path text appended to the generated message.</param>
        /// <exception cref="Exception">Thrown when <c>result</c> is <see langword="false"/>.</exception>
        internal void ThrowLastErrorIfFalse(string? path = null)
        {
            if (!result)
            {
                Error.GetLastError().ThrowThirtyTwoException(path);
            }
        }
    }

    /// <summary>
    ///  Converts a Win32 error to the corresponding managed exception type used by thirtytwo.
    /// </summary>
    /// <param name="error">The Win32 error code to map.</param>
    /// <param name="message">The message to use for the created exception.</param>
    /// <param name="path">An optional path associated with the failure.</param>
    /// <returns>The mapped managed exception instance.</returns>
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