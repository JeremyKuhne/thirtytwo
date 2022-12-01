// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public class DriveNotReadyException : ThirtyTwoIOException
{
    public DriveNotReadyException(string? message = null)
        : base(WIN32_ERROR.ERROR_NOT_READY, message) { }
}