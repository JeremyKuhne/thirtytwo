// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;
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
        using VARIANT variant = default;
        nint address = (nint)(void*)&variant;
        Assert.Throws<NotSupportedException>(() => Marshal.GetNativeVariantForObject(rectangle, address));

        using TestVariant test = new();
        using ComScope<IUnknown> scope = new(GetComPointer<IUnknown>(test));
        ITestVariantComInterop comInterop = (ITestVariantComInterop)Marshal.GetObjectForIUnknown(scope);
        Assert.Throws<NotSupportedException>(() => comInterop.SetVariant(rectangle));
    }

    [Fact]
    public void MarshalArrayToVariant()
    {
        int[] array = new[] { 1, 2, 3, 4 };
        using VARIANT variant = default;
        nint address = (nint)(void*)&variant;
        Marshal.GetNativeVariantForObject(array, address);
        variant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);

        using TestVariant test = new();
        using ComScope<IUnknown> scope = new(GetComPointer<IUnknown>(test));
        ITestVariantComInterop comInterop = (ITestVariantComInterop)Marshal.GetObjectForIUnknown(scope);
        comInterop.SetVariant(array);
        test.Variant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);
    }

    [Fact]
    public void TestLegacy()
    {
        LegacyVariantObject legacy = new();
        IUnknown* unknown = GetComPointer<IUnknown>(legacy);
        using ComScope<IUnknown> scope = new(unknown);
        using ComScope<ITestVariant> testVariant = ComScope<ITestVariant>.QueryFrom(unknown);
        legacy.Variant = new Rectangle();
        VARIANT variant = default;
        testVariant.Value->GetVariant(&variant);
    }

    public class LegacyVariantObject : ITestVariantComInterop
    {
        public object? Variant { get; set; }

        public object? GetVariant()
        {
            return Variant;
        }

        public void SetVariant(object? variant)
        {
            Variant = variant;
        }

        public void SetVariantByRef(ref object? variant)
        {
            Variant = variant;
        }
    }

    [ComImport]
    [Guid("3BE9EE32-26FB-4E7A-B8A8-25795A7EFB53")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITestVariantComInterop
    {
        public object? GetVariant();

        public void SetVariant(object? variant);

        public void SetVariantByRef(ref object? variant);
    }

    public class UiaProvider : IRawElementProviderSimple.Interface, IManagedWrapper<IRawElementProviderSimple>
    {
        public ProviderOptions ProviderOptions => throw new NotImplementedException();

        public HRESULT GetPatternProvider(UIA_PATTERN_ID patternId, IUnknown** pRetVal) => throw new NotImplementedException();
        public HRESULT GetPropertyValue(UIA_PROPERTY_ID propertyId, VARIANT* pRetVal) => throw new NotImplementedException();

        public IRawElementProviderSimple* HostRawElementProvider => throw new NotImplementedException();
    }

    public class TestVariant : ITestVariant.Interface, IDisposable, IManagedWrapper<ITestVariant>
    {
        public VARIANT Variant { get; set; }

        public void Dispose()
        {
            Variant.Clear();
            Variant = default;
        }

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

        public HRESULT GetVariant(VARIANT* variant) => throw new NotImplementedException();
    }

    public unsafe struct ITestVariant : IComIID, IVTable<ITestVariant, ITestVariant.Vtbl>
    {
        private readonly void** _vtbl;

        public HRESULT GetVariant(VARIANT* variant)
        {
            fixed (ITestVariant* pThis = &this)
                return ((delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT*, HRESULT>)_vtbl[3])(pThis, variant);
        }

        public struct Vtbl
        {
            internal delegate* unmanaged[Stdcall]<ITestVariant*, Guid*, void**, HRESULT> QueryInterface_1;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, uint> AddRef_2;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, uint> Release_3;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT*, HRESULT> GetVariant_4;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT, HRESULT> SetVariant_5;
            internal delegate* unmanaged[Stdcall]<ITestVariant*, VARIANT*, HRESULT> SetVariantByRef_6;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static HRESULT GetVariant(ITestVariant* @this, VARIANT* variant)
            => UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.GetVariant(variant));

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
            public HRESULT GetVariant(VARIANT* variant);
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
            vtable->GetVariant_4 = &GetVariant;
            vtable->SetVariant_5 = &SetVariant;
            vtable->SetVariantByRef_6 = &SetVariantByRef;
        }
    }
}