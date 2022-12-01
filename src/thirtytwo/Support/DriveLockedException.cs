// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public class DriveLockedException : ThirtyTwoIOException
{
    public DriveLockedException(string? message = null)
        : base(HRESULT.FVE_E_LOCKED_VOLUME, message) { }
}