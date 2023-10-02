// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

public unsafe class ComScopeTests
{
    [StaFact]
    public void ComScope_NullAfterDispose()
    {
        ComScope<ITestObject> scope = new(ComHelpers.GetComPointer<ITestObject>(new TestObject()));
        scope.IsNull.Should().BeFalse();
        scope.Dispose();
        scope.IsNull.Should().BeTrue();
    }

    public class TestObject : ITestObject.Interface, IManagedWrapper<ITestObject>
    {
    }

    public readonly unsafe struct ITestObject : IComIID, IVTable<ITestObject, ITestObject.Vtbl>
    {
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0169 // The field 'ComScopeTests.ITestObject._vtbl' is never used
        private readonly void** _vtbl;
#pragma warning restore CS0169
#pragma warning restore IDE0051

        public struct Vtbl
        {
            internal delegate* unmanaged[Stdcall]<ITestObject*, Guid*, void**, HRESULT> QueryInterface_1;
            internal delegate* unmanaged[Stdcall]<ITestObject*, uint> AddRef_2;
            internal delegate* unmanaged[Stdcall]<ITestObject*, uint> Release_3;
        }

        [ComImport]
        [Guid("630A7370-733D-43E9-8141-685DAE2BDB44")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface Interface
        {
        }

        static ref readonly Guid IComIID.Guid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ReadOnlySpan<byte> data = new byte[]
                {
                    // "3BE9EE32-26FB-4E7A-B8A8-25795A7EFB53"
                    0x70, 0x73, 0x0A, 0x63, 0x3D, 0x73, 0xE9, 0x43, 0x81, 0x41, 0x68, 0x5D, 0xAE, 0x2B, 0xDB, 0x44
                };

                return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
            }
        }

        static void IVTable<ITestObject, Vtbl>.PopulateVTable(Vtbl* vtable)
        {
        }
    }
}
