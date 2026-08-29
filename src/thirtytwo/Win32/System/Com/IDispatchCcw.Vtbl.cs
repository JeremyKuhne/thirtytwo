// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatchCcw
{
    /// <summary>
    ///  Native vtable layout for the <see cref="IDispatchCcw"/> COM callable wrapper.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Field names preserve COM ABI slot ordering across inherited <c>IUnknown</c> and <c>IDispatch</c> members.
    ///  </para>
    /// </remarks>
    public struct Vtbl
    {
        /// <summary>
        ///  Vtable slot 1: <c>IUnknown::QueryInterface</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT> QueryInterface_1;

        /// <summary>
        ///  Vtable slot 2: <c>IUnknown::AddRef</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IUnknown*, uint> AddRef_2;

        /// <summary>
        ///  Vtable slot 3: <c>IUnknown::Release</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IUnknown*, uint> Release_3;

        /// <summary>
        ///  Vtable slot 4: <c>IDispatch::GetTypeInfoCount</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IDispatch*, uint*, HRESULT> GetTypeInfoCount_4;

        /// <summary>
        ///  Vtable slot 5: <c>IDispatch::GetTypeInfo</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IDispatch*, uint, uint, ITypeInfo**, HRESULT> GetTypeInfo_5;

        /// <summary>
        ///  Vtable slot 6: <c>IDispatch::GetIDsOfNames</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IDispatch*, Guid*, PWSTR*, uint, uint, int*, HRESULT> GetIDsOfNames_6;

        /// <summary>
        ///  Vtable slot 7: <c>IDispatch::Invoke</c>.
        /// </summary>
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