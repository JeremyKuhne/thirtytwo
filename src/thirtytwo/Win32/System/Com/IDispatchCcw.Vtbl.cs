// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatchCcw
{
    public struct Vtbl
    {
        internal delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT> QueryInterface_1;
        internal delegate* unmanaged[Stdcall]<IUnknown*, uint> AddRef_2;
        internal delegate* unmanaged[Stdcall]<IUnknown*, uint> Release_3;
        internal delegate* unmanaged[Stdcall]<IDispatch*, uint*, HRESULT> GetTypeInfoCount_4;
        internal delegate* unmanaged[Stdcall]<IDispatch*, uint, uint, ITypeInfo**, HRESULT> GetTypeInfo_5;
        internal delegate* unmanaged[Stdcall]<IDispatch*, Guid*, PWSTR*, uint, uint, int*, HRESULT> GetIDsOfNames_6;
        internal delegate* unmanaged[Stdcall]<
            IDispatch*,
            int,
            Guid*,
            uint,
            DISPATCH_FLAGS,
            DISPPARAMS*,
            VARIANT*,
            EXCEPINFO*,
            uint*,
            HRESULT> Invoke_7;
    }
}