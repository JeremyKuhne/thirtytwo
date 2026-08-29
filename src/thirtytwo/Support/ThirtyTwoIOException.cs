// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents an <see cref="IOException"/> populated from Win32 or HRESULT error information.
/// </summary>
public class ThirtyTwoIOException : IOException
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoIOException"/> class.
    /// </summary>
    public ThirtyTwoIOException()
        : base() { }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoIOException"/> class from an HRESULT.
    /// </summary>
    /// <param name="hr">The HRESULT associated with the I/O failure.</param>
    /// <param name="message">
    ///  The exception message. When <see langword="null"/>, a formatted HRESULT message is used.
    /// </param>
    public ThirtyTwoIOException(HRESULT hr, string? message = null)
        : base(message ?? hr.ToStringWithDescription(), hresult: hr) { }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ThirtyTwoIOException"/> class from a Win32 error.
    /// </summary>
    /// <param name="error">The Win32 error code associated with the I/O failure.</param>
    /// <param name="message">
    ///  The exception message. When <see langword="null"/>, a message is generated from <paramref name="error"/>.
    /// </param>
    public ThirtyTwoIOException(WIN32_ERROR error, string? message = null)
        : base(message ?? error.ErrorToString(), (int)error.ToHRESULT()) { }
}