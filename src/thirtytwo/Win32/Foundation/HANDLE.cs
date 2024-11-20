// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Windows.Support;

namespace Windows.Win32.Foundation;

public readonly partial struct HANDLE : IDisposable
{
    public unsafe void Dispose()
    {
        if ((nint)Value != 0 && (nint)Value != -1)
        {
            Interop.CloseHandle(this).ThrowLastErrorIfFalse();
        }

        Unsafe.AsRef(in this) = default;
    }
}