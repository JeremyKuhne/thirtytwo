// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public class DriveNotReadyException(string? message = null) : ThirtyTwoIOException(WIN32_ERROR.ERROR_NOT_READY, message)
{
}