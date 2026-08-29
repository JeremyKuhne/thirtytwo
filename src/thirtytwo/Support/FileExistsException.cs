// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents a create or move operation that failed because the target already exists.
/// </summary>
/// <param name="error">
///  The originating Win32 error, typically <see cref="WIN32_ERROR.ERROR_FILE_EXISTS"/> or
///  <see cref="WIN32_ERROR.ERROR_ALREADY_EXISTS"/>.
/// </param>
/// <param name="message">
///  The exception message. When <see langword="null"/>, the base type uses the mapped default message.
/// </param>
public class FileExistsException(WIN32_ERROR error, string? message = null) : ThirtyTwoIOException(error, message)
{
}