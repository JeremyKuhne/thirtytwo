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

    /// <summary>
    ///  The IID guid for this interface.
    /// </summary>
    public static readonly Guid IID_Guid = new(0xc3fcc19e, 0xa970, 0x11d2, 0x8b, 0x5a, 0x00, 0xa0, 0xc9, 0xb7, 0xc9, 0xc4);

    /// <inheritdoc cref="IComIID.Guid"/>
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

    /// <summary>
    ///  Populates a caller-provided COM vtable with unmanaged entrypoints for <see cref="IManagedObject"/>.
    /// </summary>
    /// <param name="vtable">
    ///  A writable pointer to the ABI-ordered vtable structure. The caller allocates and owns this memory.
    /// </param>
    public static void PopulateVTable(Vtbl* vtable)
    {
        vtable->QueryInterface_1 = &QueryInterface;
        vtable->AddRef_2 = &AddRef;
        vtable->Release_3 = &Release;
        vtable->GetSerializedBuffer_4 = &GetSerializedBuffer;
        vtable->GetObjectIdentity_5 = &GetObjectIdentity;
    }

    /// <summary>
    ///  Standard-call unmanaged thunk for COM slot 1 (<c>IUnknown::QueryInterface</c>).
    /// </summary>
    /// <param name="this">The COM interface pointer supplied by the native caller.</param>
    /// <param name="riid">The requested interface identifier.</param>
    /// <param name="ppvObject">Receives the requested interface pointer on success.</param>
    /// <returns>An <c>HRESULT</c> that indicates success or failure.</returns>
    /// <remarks>
    ///  <para>
    ///   This method does not take ownership of <paramref name="riid"/> or <paramref name="ppvObject"/>; pointer
    ///   lifetime and allocation remain owned by the native caller under COM rules.
    ///  </para>
    /// </remarks>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT QueryInterface(IManagedObject* @this, Guid* riid, void** ppvObject)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.QueryInterface(riid, ppvObject));

    /// <summary>
    ///  Standard-call unmanaged thunk for COM slot 2 (<c>IUnknown::AddRef</c>).
    /// </summary>
    /// <param name="this">The COM interface pointer supplied by the native caller.</param>
    /// <returns>The post-increment COM reference count, per COM convention.</returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(IManagedObject* @this)
        => UnwrapAndInvoke<IManagedObject, Interface, uint>(@this, o => o.AddRef());

    /// <summary>
    ///  Standard-call unmanaged thunk for COM slot 3 (<c>IUnknown::Release</c>).
    /// </summary>
    /// <param name="this">The COM interface pointer supplied by the native caller.</param>
    /// <returns>The post-decrement COM reference count, per COM convention.</returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IManagedObject* @this)
        => UnwrapAndInvoke<IManagedObject, Interface, uint>(@this, o => o.Release());

    /// <summary>
    ///  Standard-call unmanaged thunk for COM slot 4 (<c>IManagedObject::GetSerializedBuffer</c>).
    /// </summary>
    /// <param name="this">The COM interface pointer supplied by the native caller.</param>
    /// <param name="pBSTR">Receives a BSTR allocated by the COM implementation on success.</param>
    /// <returns>An <c>HRESULT</c> that indicates success or failure.</returns>
    /// <remarks>
    ///  <para>
    ///   The caller owns and releases the returned BSTR according to COM BSTR lifetime rules.
    ///  </para>
    /// </remarks>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetSerializedBuffer(IManagedObject* @this, BSTR* pBSTR)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.GetSerializedBuffer(pBSTR));

    /// <summary>
    ///  Standard-call unmanaged thunk for COM slot 5 (<c>IManagedObject::GetObjectIdentity</c>).
    /// </summary>
    /// <param name="this">The COM interface pointer supplied by the native caller.</param>
    /// <param name="pBSTRGUID">Receives a BSTR process identity GUID on success.</param>
    /// <param name="AppDomainID">Receives the AppDomain identifier for the wrapped managed object.</param>
    /// <param name="pCCW">Receives the COM callable wrapper slot identifier.</param>
    /// <returns>An <c>HRESULT</c> that indicates success or failure.</returns>
    /// <remarks>
    ///  <para>
    ///   Output pointers are caller-provided storage. Any returned BSTR follows COM ownership rules and is released
    ///   by the native caller.
    ///  </para>
    /// </remarks>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT GetObjectIdentity(IManagedObject* @this, BSTR* pBSTRGUID, int* AppDomainID, int* pCCW)
        => UnwrapAndInvoke<IManagedObject, Interface>(@this, o => o.GetObjectIdentity(pBSTRGUID, AppDomainID, pCCW));
}