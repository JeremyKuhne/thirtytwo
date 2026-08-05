// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

public unsafe partial struct IComCallableWrapper
{
    public struct Vtbl
    {
        internal delegate* unmanaged[Stdcall]<IEnumUnknown*, Guid*, void**, HRESULT> QueryInterface_1;
        internal delegate* unmanaged[Stdcall]<IEnumUnknown*, uint> AddRef_2;
        internal delegate* unmanaged[Stdcall]<IEnumUnknown*, uint> Release_3;
    }
}