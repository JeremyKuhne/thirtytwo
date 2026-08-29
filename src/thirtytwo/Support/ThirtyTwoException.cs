// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.Support;

/// <summary>
///  Represents an exception created from Win32 or HRESULT error information.
/// </summary>
public class ThirtyTwoException : Win32Exception
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoException"/> class.
    /// </summary>
    public ThirtyTwoException()
        : base() { }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoException"/> class from a message and optional HRESULT.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="hresult">The HRESULT to store in <see cref="Exception.HResult"/>.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public ThirtyTwoException(string? message, HRESULT hresult = default, Exception? innerException = null)
        : base(message, innerException) { HResult = (int)hresult; }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoException"/> class from a Win32 error code.
    /// </summary>
    /// <param name="error">The Win32 error code to convert to HRESULT and default message text.</param>
    /// <param name="message">
    ///  The exception message. When <see langword="null"/>, a message is generated from <paramref name="error"/>.
    /// </param>
    public ThirtyTwoException(WIN32_ERROR error, string? message = null)
        : base(error.ToHRESULT(), message ?? error.ErrorToString()) { HResult = (int)error.ToHRESULT(); }
}