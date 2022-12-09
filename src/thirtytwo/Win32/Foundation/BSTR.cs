// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Windows.Win32.Foundation;

public readonly unsafe partial struct BSTR : IDisposable
{
    public BSTR(string value) : this((char*)Marshal.StringToBSTR(value))
    {
    }

    public void Dispose()
    {
        Marshal.FreeBSTR((nint)Value);
        fixed (char** c = &Value)
        {
            *c = null;
        }
    }

    public string ToStringAndFree()
    {
        string result = ToString();
        Dispose();
        return result;
    }

    public bool IsNull => Value is null;
}