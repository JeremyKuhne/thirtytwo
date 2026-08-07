// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.DotNet;

public unsafe partial struct IManagedObject
{
    public struct Vtbl
    {
#pragma warning disable TOUKI0041 // Native vtable field names mirror their ABI slots.
        internal delegate* unmanaged[Stdcall]<IManagedObject*, Guid*, void**, HRESULT> QueryInterface_1;
        internal delegate* unmanaged[Stdcall]<IManagedObject*, uint> AddRef_2;
        internal delegate* unmanaged[Stdcall]<IManagedObject*, uint> Release_3;
        internal delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, HRESULT> GetSerializedBuffer_4;
        internal delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, int*, int*, HRESULT> GetObjectIdentity_5;
#pragma warning restore TOUKI0041
    }
}