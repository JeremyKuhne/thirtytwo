// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public unsafe partial struct ITypeInfo
{
    public readonly unsafe ref struct TypeAttrScope
    {
        private readonly ITypeInfo* _typeInfo;
        private readonly TYPEATTR* _typeAttr;

        public TypeAttrScope(ITypeInfo* typeInfo, TYPEATTR* typeAttr)
        {
            _typeInfo = typeInfo;
            _typeAttr = typeAttr;
        }

        public TYPEATTR* Value => _typeAttr;

        public readonly void Dispose()
        {
            if (_typeAttr is not null)
            {
                _typeInfo->ReleaseTypeAttr(_typeAttr);
            }
        }
    }
}