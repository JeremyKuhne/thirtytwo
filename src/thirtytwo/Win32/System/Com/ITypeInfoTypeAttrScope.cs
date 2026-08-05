// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public readonly unsafe ref struct ITypeInfoTypeAttrScope
{
    private readonly ITypeInfo* _typeInfo;
    private readonly TYPEATTR* _typeAttr;

    public ITypeInfoTypeAttrScope(ITypeInfo* typeInfo, TYPEATTR* typeAttr)
    {
        _typeInfo = typeInfo;
        _typeAttr = typeAttr;
    }

    public TYPEATTR* Value => _typeAttr;

    public void Dispose()
    {
        if (_typeAttr is not null)
        {
            _typeInfo->ReleaseTypeAttr(_typeAttr);
        }
    }
}