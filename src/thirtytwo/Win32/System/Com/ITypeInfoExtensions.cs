// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Windows.Win32.System.Com;

public static unsafe class ITypeInfoExtensions
{
    extension(ref ITypeInfo typeInfo)
    {
        public ITypeInfoTypeAttrScope GetTypeAttr(out HRESULT hr)
        {
            hr = typeInfo.GetTypeAttr(out TYPEATTR* typeAttr);
            return new((ITypeInfo*)Unsafe.AsPointer(ref typeInfo), typeAttr);
        }
    }
}