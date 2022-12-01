// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public class FileExistsException : ThirtyTwoIOException
{
    public FileExistsException(WIN32_ERROR error, string? message = null)
        : base(error, message) { }
}