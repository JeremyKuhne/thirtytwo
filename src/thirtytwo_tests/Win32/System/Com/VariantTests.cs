// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace Tests.Windows.Win32.System.Com;

public unsafe class VariantTests
{
    [Fact]
    public void MarshalRectangleToVariant()
    {
        // COM Interop can't handle converting arbitrary structs to VARIANT. The one special case type from
        // System.Drawing is Color which does the OLE color conversion to int.
        Rectangle rectangle = new(1, 2, 3, 4);
        using VARIANT variant = default;
        nint address = (nint)(void*)&variant;
        Assert.Throws<NotSupportedException>(() => Marshal.GetNativeVariantForObject(rectangle, address));

        using TestVariant test = new();
        using var unknown = ComScope<IUnknown>.GetComCallableWrapper(test);
        ITestVariantComInterop comInterop = (ITestVariantComInterop)Marshal.GetObjectForIUnknown(unknown);
        Assert.Throws<NotSupportedException>(() => comInterop.SetVariant(rectangle));
    }

    [Fact]
    public void MarshalRectangleToVariant_ThroughLegacyCCW()
    {
        // Create a managed object that marshals through `object`
        LegacyVariantObject legacy = new();
        using var unknown = ComScope<IUnknown>.GetComCallableWrapper(legacy);
        using ComScope<ITestVariant> testVariant = unknown.QueryInterface<ITestVariant>();
        legacy.Variant = new Rectangle();

        // Getting the variant from the native pointer gives the NotSupported HRESULT
        VARIANT variant = default;
        testVariant.Value->GetVariant(&variant).Should().Be(HRESULT.COR_E_NOTSUPPORTED);
        variant.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void MarshalArrayToVariant()
    {
        int[] array = new[] { 1, 2, 3, 4 };
        using VARIANT variant = default;
        nint address = (nint)(void*)&variant;

        // Use Marshal to create the VARIANT directly
        Marshal.GetNativeVariantForObject(array, address);
        variant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);

        SAFEARRAY* safeArray = variant.data.parray;
        safeArray->cDims.Should().Be(1);
        safeArray->cLocks.Should().Be(0);
        safeArray->cbElements.Should().Be(4);
        safeArray->fFeatures.Should().Be(ADVANCED_FEATURE_FLAGS.FADF_HAVEVARTYPE);

        // Now create an object to expose as a CCW through ComWrappers.
        // We can't do a using on TestVariant as the same SAFEARRAY instance is generated below.
        TestVariant test = new();
        using var unkown = ComScope<IUnknown>.GetComCallableWrapper(test);

        // Use legacy COM interop to create an RCW for the pointer and set through that projection.
        ITestVariantComInterop comInterop = (ITestVariantComInterop)Marshal.GetObjectForIUnknown(unkown);
        comInterop.SetVariant(array);

        VARIANT setVariant = test.Variant;
        setVariant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);
        SAFEARRAY* newSafeArray = variant.data.parray;

        // It's surprising that the SAFEARRAY is reused, but it is.
        Assert.True(newSafeArray == safeArray);
    }


    [Fact]
    public void ArrayToVariantAfterFreeIsSometimesNewObject()
    {
        int[] array = new[] { 1, 2, 3, 4 };
        SAFEARRAY* safeArray;

        using (VARIANT variant = default)
        {
            nint address = (nint)(void*)&variant;

            // Use Marshal to create the VARIANT directly
            Marshal.GetNativeVariantForObject(array, address);
            variant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);

            safeArray = variant.data.parray;
        }

        using (VARIANT variant = default)
        {
            nint address = (nint)(void*)&variant;

            // Use Marshal to create the VARIANT directly
            Marshal.GetNativeVariantForObject(array, address);
            variant.vt.Should().Be(VARENUM.VT_ARRAY | VARENUM.VT_I4);

            SAFEARRAY* newSafeArray = variant.data.parray;

            // Usually this is the case, presumably there is no way to guarantee this as the same memory location
            // could be allocated again.
            // Assert.False(safeArray == newSafeArray);
        }
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

    public sealed class TestVariant : ITestVariant.Interface, IDisposable, IManagedWrapper<ITestVariant>
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
            => ComHelpers.UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.GetVariant(variant));

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static HRESULT SetVariant(ITestVariant* @this, VARIANT variant)
            => ComHelpers.UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.SetVariant(variant));

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static HRESULT SetVariantByRef(ITestVariant* @this, VARIANT* variant)
            => ComHelpers.UnwrapAndInvoke<ITestVariant, Interface>(@this, o => o.SetVariantByRef(variant));

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