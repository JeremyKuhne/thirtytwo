// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Windows.Win32.ComHelpers;

namespace Windows.Win32.System.Com;

public unsafe partial struct IUnknown : IVTable<IUnknown, IUnknown.Vtbl>
{
    public TInterface* TryQueryInterface<TInterface>() where TInterface : unmanaged, IComIID
    {
        TInterface* @interface = default;
        QueryInterface(IID.Get<TInterface>(), (void**)&@interface);
        return @interface;
    }

    public TInterface* QueryInterface<TInterface>() where TInterface : unmanaged, IComIID
    {
        TInterface* @interface = default;
        QueryInterface(IID.Get<TInterface>(), (void**)&@interface).ThrowOnFailure();
        return @interface;
    }

    public AgileComPointer<TInterface>? TryQueryAgileInterface<TInterface>()
        where TInterface : unmanaged, IComIID
    {
        TInterface* @interface = TryQueryInterface<TInterface>();
        return @interface is null ? null : new(@interface, takeOwnership: true);
    }

    public static void PopulateVTable(Vtbl* vtable)
    {
        vtable->QueryInterface_1 = &QueryInterface;
        vtable->AddRef_2 = &AddRef;
        vtable->Release_3 = &Release;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static HRESULT QueryInterface(IUnknown* @this, Guid* riid, void** ppvObject)
        => UnwrapAndInvoke<IUnknown, Interface>(@this, o => o.QueryInterface(riid, ppvObject));

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(IUnknown* @this)
        => UnwrapAndInvoke<IUnknown, Interface, uint>(@this, o => o.AddRef());

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IUnknown* @this)
        => UnwrapAndInvoke<IUnknown, Interface, uint>(@this, o => o.Release());

    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public unsafe interface Interface
    {
        [PreserveSig]
        HRESULT QueryInterface(
            Guid* riid,
            void** ppvObject);

        [PreserveSig]
        uint AddRef();

        [PreserveSig]
        uint Release();
    }
}