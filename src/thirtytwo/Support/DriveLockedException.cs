// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

/// <summary>
///  Represents a BitLocker locked-volume failure.
/// </summary>
/// <param name="message">
///  The exception message. When <see langword="null"/>, the base type uses the mapped default message.
/// </param>
public class DriveLockedException(string? message = null) : ThirtyTwoIOException(HRESULT.FVE_E_LOCKED_VOLUME, message)
{
}