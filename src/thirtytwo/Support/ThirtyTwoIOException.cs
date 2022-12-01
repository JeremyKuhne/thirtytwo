// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Support;

public class ThirtyTwoIOException : IOException
{
    public ThirtyTwoIOException()
        : base() { }

    public ThirtyTwoIOException(HRESULT hr, string? message = null)
        : base(message ?? hr.ToStringWithDescription(), hresult: hr) { }

    public ThirtyTwoIOException(WIN32_ERROR error, string? message = null)
        : base(message ?? error.ErrorToString(), (int)error.ToHRESULT()) { }
}