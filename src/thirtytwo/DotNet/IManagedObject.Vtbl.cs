// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.DotNet;

public unsafe partial struct IManagedObject
{
    /// <summary>
    ///  Native COM vtable layout for <see cref="IManagedObject"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Field order and signatures are ABI contract and must match the native interface exactly.
    ///  </para>
    /// </remarks>
    public struct Vtbl
    {
#pragma warning disable TOUKI0041 // Native vtable field names mirror their ABI slots.
        /// <summary>
        ///  Slot 1: <c>IUnknown::QueryInterface</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IManagedObject*, Guid*, void**, HRESULT> QueryInterface_1;

        /// <summary>
        ///  Slot 2: <c>IUnknown::AddRef</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IManagedObject*, uint> AddRef_2;

        /// <summary>
        ///  Slot 3: <c>IUnknown::Release</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IManagedObject*, uint> Release_3;

        /// <summary>
        ///  Slot 4: <c>IManagedObject::GetSerializedBuffer</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, HRESULT> GetSerializedBuffer_4;

        /// <summary>
        ///  Slot 5: <c>IManagedObject::GetObjectIdentity</c>.
        /// </summary>
        internal delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, int*, int*, HRESULT> GetObjectIdentity_5;
#pragma warning restore TOUKI0041
    }
}