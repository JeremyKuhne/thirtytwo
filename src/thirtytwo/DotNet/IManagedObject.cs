// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.System.Com;
using static Windows.Win32.ComExtensions;

namespace Windows.DotNet;

/// <inheritdoc cref="Interface"/>
public unsafe partial struct IManagedObject : IComIID, IVTable<IManagedObject, IManagedObject.Vtbl>
{
    private readonly void** _vtable;

    /// <inheritdoc cref="IUnknown.QueryInterface(Guid*, void**)"/>
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        fixed (IManagedObject* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IManagedObject*, Guid*, void**, HRESULT>)_vtable[0])(pThis, riid, ppvObject);
    }

    /// <inheritdoc cref="IUnknown.AddRef()"/>
    public uint AddRef()
    {
        fixed (IManagedObject* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IManagedObject*, uint>)_vtable[1])(pThis);
    }

    /// <inheritdoc cref="IUnknown.Release()"/>
    public uint Release()
    {
        fixed (IManagedObject* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IManagedObject*, uint>)_vtable[2])(pThis);
    }

    /// <inheritdoc cref="Interface.GetSerializedBuffer(BSTR*)"/>
    public HRESULT GetSerializedBuffer(BSTR* pBSTR)
    {
        fixed (IManagedObject* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, HRESULT>)_vtable[3])(pThis, pBSTR);
    }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    /// <inheritdoc cref="Interface.GetObjectIdentity(BSTR*, int*, int*)"/>
    public HRESULT GetObjectIdentity(BSTR* pBSTRGUID, int* AppDomainID, int* pCCW)
    {
        fixed (IManagedObject* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IManagedObject*, BSTR*, int*, int*, HRESULT>)_vtable[4])(pThis, pBSTRGUID, AppDomainID, pCCW);
    }
#pragma warning restore SA1313

    /// <summary>The IID guid for this interface.</summary>
    public static readonly Guid IID_Guid = new(0xc3fcc19e, 0xa970, 0x11d2, 0x8b, 0x5a, 0x00, 0xa0, 0xc9, 0xb7, 0xc9, 0xc4);

    static ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ReadOnlySpan<byte> data = new byte[]
            {
                0x9e, 0xc1, 0xfc, 0xc3,
                0x70, 0xa9,
                0xd2, 0x11,
                0x8b, 0x5a, 0x00, 0xa0, 0xc9, 0xb7, 0xc9, 0xc4
            };

            return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
        }
    }

    public static void PopulateVTable(Vtbl* vtable)
    {
        vtable->QueryInterface_1 = &QueryInterface;
        vtable->AddRef_2 = &AddRef;
        vtable->Release_3 = &Release;
        vtable->GetSerializedBuffer_4 = &GetSerializedBuffer;
        vtable->GetObjectIdentity_5 = &GetObjectIdentity;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT QueryInterface(IManagedObject* @this, Guid* riid, void** ppvObject)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.QueryInterface(riid, ppvObject));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(IManagedObject* @this)
        => UnwrapAndInvoke<IManagedObject, Interface, uint>(@this, o => o.AddRef());

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IManagedObject* @this)
        => UnwrapAndInvoke<IManagedObject, Interface, uint>(@this, o => o.Release());

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetSerializedBuffer(IManagedObject* @this, BSTR* pBSTR)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.GetSerializedBuffer(pBSTR));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetObjectIdentity(IManagedObject* @this, BSTR* pBSTRGUID, int* AppDomainID, int* pCCW)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.GetObjectIdentity(pBSTRGUID, AppDomainID, pCCW));
}