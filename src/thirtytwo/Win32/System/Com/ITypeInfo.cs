// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

public unsafe partial struct ITypeInfo
{
    public TypeAttrScope GetTypeAttr(out HRESULT hr)
    {
        hr = GetTypeAttr(out TYPEATTR* typeAttr);
        return new TypeAttrScope((ITypeInfo*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)), typeAttr);
    }
}