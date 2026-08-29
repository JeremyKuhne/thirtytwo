// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents an attempt to access a drive that is present but not ready.
/// </summary>
/// <param name="message">
///  The exception message. When <see langword="null"/>, the base type uses the mapped default message.
/// </param>
public class DriveNotReadyException(string? message = null) : ThirtyTwoIOException(WIN32_ERROR.ERROR_NOT_READY, message)
{
}