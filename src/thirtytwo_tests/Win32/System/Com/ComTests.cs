// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Dialogs;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com.Marshal;
using Windows.Win32.UI.Shell;
using InteropMarshal = global::System.Runtime.InteropServices.Marshal;

namespace Windows.Win32.System.Com;

[TestClass]
public unsafe class ComTests
{
    [TestMethod]
    public void Com_GetComPointer_SameUnknownInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        using ComScope<IUnknown> unknown1 = events.GetComCallableWrapper();
        using ComScope<IUnknown> unknown2 = events.GetComCallableWrapper();

        Assert.IsTrue(unknown1.Pointer == unknown2.Pointer);
    }

    [TestMethod]
    public void Com_GetComPointer_SameInterfaceInstance()
    {
        FileDialog.FileDialogEvents events = new(null!);
        using ComScope<IFileDialogEvents> iEvents1 = events.GetComCallableWrapper<IFileDialogEvents>();
        using ComScope<IFileDialogEvents> iEvents2 = events.GetComCallableWrapper<IFileDialogEvents>();

        Assert.IsTrue(iEvents1.Pointer == iEvents2.Pointer);
    }

    [TestMethod]
    public void Com_BuiltInCom_RCW_Behavior()
    {
        UnknownTest unknown = new();
        using ComScope<IUnknown> iUnknown = new(UnknownCCW.CreateInstance(unknown));

        object rcw = InteropMarshal.GetObjectForIUnknown((IntPtr)iUnknown.Pointer);

        unknown.AddRefCount.Should().Be(1);
        unknown.ReleaseCount.Should().Be(1);
        unknown.LastRefCount.Should().Be(2);
        unknown.QueryInterfaceGuids.Should().BeEquivalentTo([
            IUnknown.IID_Guid,
            INoMarshal.IID_Guid,
            IAgileObject.IID_Guid,
            IMarshal.IID_Guid]);

        // Release and FinalRelease look the same from our IUnknown's perspective
        InteropMarshal.FinalReleaseComObject(rcw);

        unknown.AddRefCount.Should().Be(1);
        unknown.ReleaseCount.Should().Be(2);
        unknown.LastRefCount.Should().Be(1);
        unknown.QueryInterfaceGuids.Should().BeEquivalentTo([
            IUnknown.IID_Guid,
            INoMarshal.IID_Guid,
            IAgileObject.IID_Guid,
            IMarshal.IID_Guid]);
    }

    public interface IUnkownTest
    {
        public void QueryInterface(Guid riid);
        public void AddRef(uint current);
        public void Release(uint current);
    }

    public class UnknownTest : IUnkownTest
    {
        public int AddRefCount { get; private set; }
        public int ReleaseCount { get; private set; }
        public List<Guid> QueryInterfaceGuids { get; } = [];
        public int LastRefCount { get; private set; }

        void IUnkownTest.AddRef(uint current)
        {
            AddRefCount++;
            LastRefCount = (int)current;
        }

        void IUnkownTest.QueryInterface(Guid riid)
        {
            QueryInterfaceGuids.Add(riid);
        }

        void IUnkownTest.Release(uint current)
        {
            ReleaseCount++;
            LastRefCount = (int)current;
        }
    }

    public static class UnknownCCW
    {
        public static IUnknown* CreateInstance(IUnkownTest @object)
            => (IUnknown*)Lifetime<UnknownVtable, IUnkownTest>.Allocate(@object, CCWVTable);

        private static readonly UnknownVtable* CCWVTable = AllocateVTable();

        private static UnknownVtable* AllocateVTable()
        {
            // Allocate and create a static VTable for this type projection.
            var vtable = (UnknownVtable*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(UnknownCCW),
                sizeof(UnknownVtable));

            // IUnknown
            vtable->QueryInterface_1 = &QueryInterface;
            vtable->AddRef_2 = &AddRef;
            vtable->Release_3 = &Release;
            return vtable;
        }

        private struct UnknownVtable
        {
            internal delegate* unmanaged[Stdcall]<IUnknown*, Guid*, void**, HRESULT> QueryInterface_1;
            internal delegate* unmanaged[Stdcall]<IUnknown*, uint> AddRef_2;
            internal delegate* unmanaged[Stdcall]<IUnknown*, uint> Release_3;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static HRESULT QueryInterface(IUnknown* @this, Guid* riid, void** ppvObject)
        {
            if (ppvObject is null)
            {
                return HRESULT.E_POINTER;
            }

            var unknown = Lifetime<UnknownVtable, IUnkownTest>.GetObject(@this);
            if (unknown is null)
            {
                return HRESULT.COR_E_OBJECTDISPOSED;
            }

            unknown.QueryInterface(*riid);

            if (*riid == typeof(IUnknown).GUID)
            {
                *ppvObject = @this;
            }
            else
            {
                *ppvObject = null;
                return PInvoke.E_NOINTERFACE;
            }

            Lifetime<UnknownVtable, IUnkownTest>.AddRef(@this);
            return HRESULT.S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static uint AddRef(IUnknown* @this)
        {
            var unknown = Lifetime<UnknownVtable, IUnkownTest>.GetObject(@this);
            if (unknown is null)
            {
                return HRESULT.COR_E_OBJECTDISPOSED;
            }

            uint current = Lifetime<UnknownVtable, IUnkownTest>.AddRef(@this);
            unknown.AddRef(current);
            return current;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static uint Release(IUnknown* @this)
        {
            var unknown = Lifetime<UnknownVtable, IUnkownTest>.GetObject(@this);
            if (unknown is null)
            {
                return HRESULT.COR_E_OBJECTDISPOSED;
            }

            uint current = Lifetime<UnknownVtable, IUnkownTest>.Release(@this);
            unknown.Release(current);
            return current;
        }
    }
}
