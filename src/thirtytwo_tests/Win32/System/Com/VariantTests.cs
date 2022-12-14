// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using static Windows.Win32.System.Com.Com;

namespace Tests.Windows.Win32.System.Com;

public unsafe class VariantTests
{
    [Fact]
    public void MarshalRectangleToVariant()
    {
        // COM Interop can't handle converting arbitrary struct conversion to VARIANT. The one special case type from
        // System.Drawing is Color which does the OLE color conversion to int.
        Rectangle rectangle = new(1, 2, 3, 4);
        VARIANT variant = default;
        nint address = (nint)(void*)&variant;
        Assert.Throws<NotSupportedException>(() => Marshal.GetNativeVariantForObject(rectangle, address));

        TestVariant test = new();
        using ComScope<IUnknown> scope = new(GetComPointer<IUnknown>(test));
        ITestVariantComInterop comInterop = (ITestVariantComInterop)Marshal.GetObjectForIUnknown(scope);
        Assert.Throws<NotSupportedException>(() => comInterop.SetVariant(rectangle));
    }


    [ComImport]
    [Guid("3BE9EE32-26FB-4E7A-B8A8-25795A7EFB53")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITestVariantComInterop
    {
        [PreserveSig]
        public HRESULT SetVariant(object? variant);

        [PreserveSig]
        public HRESULT SetVariantByRef(ref object? variant);
    }

    public class TestVariant : ITestVariant.Interface, IManagedWrapper<ITestVariant>
    {
        public VARIANT Variant { get; set; }

        public HRESULT SetVariant(VARIANT variant)
        {
            Variant = variant;
            return HRESULT.S_OK;
        }

        public HRESULT SetVariantByRef(VARIANT* variant)
        {
            if (variant is null)
            {
                return HRESULT.E_POINTER;
            }

            Variant = *variant;
            return HRESULT.S_OK;
        }
    }

    public unsafe struct ITestVariant : IComIID, IVTable<ITestVariant, ITestVariant.Vtbl>
    {
        public struct Vtbl
        {
            internal delegate* unmanaged[Stdcall]<ITestVariant*, Guid*, void**, HRESULT> QueryInterface_1;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, uint> AddRef_2;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, uint> Release_3;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT, HRESULT> SetVariant_4;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT*, HRESULT> SetVariantByRef_5;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static HRESULT SetVariant(ITestVariant* @this, VARIANT variant)
            => UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.SetVariant(variant));

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static HRESULT SetVariantByRef(ITestVariant* @this, VARIANT* variant)
            => UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.SetVariantByRef(variant));

        [ComImport]
        [Guid("3BE9EE32-26FB-4E7A-B8A8-25795A7EFB53")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface Interface
        {
            public HRESULT SetVariant(VARIANT variant);
            public HRESULT SetVariantByRef(VARIANT* variant);
        }

        static ref readonly Guid IComIID.Guid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ReadOnlySpan<byte> data = new byte[]
                {
                    0x32, 0xEE, 0xE9, 0x3B, 0xFB, 0x26, 0x7A, 0x4E, 0xB8, 0xA8, 0x25, 0x79, 0x5A, 0x7E, 0xFB, 0x53
                };

                return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
            }
        }

        static void IVTable<ITestVariant, Vtbl>.PopulateVTable(Vtbl* vtable)
        {
            vtable->SetVariant_4 = &SetVariant;
            vtable->SetVariantByRef_5 = &SetVariantByRef;
        }
    }
}