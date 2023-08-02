// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Windows.Support;

public class ThirtyTwoException : Win32Exception
{
    public ThirtyTwoException()
        : base() { }

    public ThirtyTwoException(string? message, HRESULT hresult = default, Exception? innerException = null)
        : base(message, innerException) { HResult = (int)hresult; }

    public ThirtyTwoException(WIN32_ERROR error, string? message = null)
        : base(error.ToHRESULT(), message ?? error.ErrorToString()) { HResult = (int)error.ToHRESULT(); }
}