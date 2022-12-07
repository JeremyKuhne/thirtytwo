// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ManagedWrapperTests
{
    [Fact]
    public void ManagedWrapper_SameInterfaceDifferentClasses()
    {
        OleContainerOne containerOne = new();
        OleContainerTwo containerTwo = new();

        ComInterfaceTable tableOne = ((IManagedWrapper)containerOne).GetInterfaceTable();
        ComInterfaceTable tableTwo = ((IManagedWrapper)containerTwo).GetInterfaceTable();

        // We should get the same table instance
        Assert.False(tableOne.Entries is null);
        Assert.True(tableOne.Entries == tableTwo.Entries);
    }

    [Fact]
    public void ManagedWrapper_DifferentInterfaceDifferentClasses()
    {
        OleContainerOne container = new();
        OleClientSite site = new();

        ComInterfaceTable containerTable = ((IManagedWrapper)container).GetInterfaceTable();
        ComInterfaceTable siteTable = ((IManagedWrapper)site).GetInterfaceTable();

        // We should not get the same table instance
        Assert.False(containerTable.Entries is null);
        Assert.False(siteTable.Entries is null);
        Assert.False(containerTable.Entries == siteTable.Entries);
    }

    [Fact]
    public void ManagedWrapper_TwoInterfaces()
    {
        OleThing thing = new();
        ComInterfaceTable table = ((IManagedWrapper)thing).GetInterfaceTable();

        Assert.False(table.Entries is null);
        table.Count.Should().Be(3);
        table.Entries[0].Vtable.Should().NotBe(0);
        table.Entries[0].IID.Should().Be(*IID.Get<IComCallableWrapper>());
        table.Entries[1].Vtable.Should().NotBe(0);
        table.Entries[1].IID.Should().Be(*IID.Get<IOleClientSite>());
        table.Entries[2].Vtable.Should().NotBe(0);
        table.Entries[2].IID.Should().Be(*IID.Get<IOleContainer>());
    }

    [Fact]
    public void ManagedWrapper_OneInterface()
    {
        OleContainerOne thing = new();
        ComInterfaceTable table = ((IManagedWrapper)thing).GetInterfaceTable();

        Assert.False(table.Entries is null);
        table.Count.Should().Be(2);
        table.Entries[0].Vtable.Should().NotBe(0);
        table.Entries[0].IID.Should().Be(*IID.Get<IComCallableWrapper>());
        table.Entries[1].Vtable.Should().NotBe(0);
        table.Entries[1].IID.Should().Be(*IID.Get<IOleContainer>());
    }

    private class OleContainerOne : IOleContainer.Interface, IManagedWrapper<IOleContainer>
    {
        unsafe HRESULT IOleContainer.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
        unsafe HRESULT IOleContainer.Interface.EnumObjects(OLECONTF grfFlags, IEnumUnknown** ppenum) => throw new NotImplementedException();
        HRESULT IOleContainer.Interface.LockContainer(BOOL fLock) => throw new NotImplementedException();
        unsafe HRESULT IParseDisplayName.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
    }

    private class OleContainerTwo : IOleContainer.Interface, IManagedWrapper<IOleContainer>
    {
        unsafe HRESULT IOleContainer.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
        unsafe HRESULT IOleContainer.Interface.EnumObjects(OLECONTF grfFlags, IEnumUnknown** ppenum) => throw new NotImplementedException();
        HRESULT IOleContainer.Interface.LockContainer(BOOL fLock) => throw new NotImplementedException();
        unsafe HRESULT IParseDisplayName.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
    }

    private class OleClientSite : IOleClientSite.Interface, IManagedWrapper<IOleClientSite>
    {
        HRESULT IOleClientSite.Interface.SaveObject() => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, IMoniker** ppmk) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.GetContainer(IOleContainer** ppContainer) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.ShowObject() => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.OnShowWindow(BOOL fShow) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.RequestNewObjectLayout() => throw new NotImplementedException();
    }

    private class OleThing : IOleClientSite.Interface, IOleContainer.Interface, IManagedWrapper<IOleClientSite, IOleContainer>
    {
        HRESULT IOleClientSite.Interface.SaveObject() => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.GetMoniker(OLEGETMONIKER dwAssign, OLEWHICHMK dwWhichMoniker, IMoniker** ppmk) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.GetContainer(IOleContainer** ppContainer) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.ShowObject() => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.OnShowWindow(BOOL fShow) => throw new NotImplementedException();
        HRESULT IOleClientSite.Interface.RequestNewObjectLayout() => throw new NotImplementedException();
        HRESULT IOleContainer.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
        HRESULT IOleContainer.Interface.EnumObjects(OLECONTF grfFlags, IEnumUnknown** ppenum) => throw new NotImplementedException();
        HRESULT IOleContainer.Interface.LockContainer(BOOL fLock) => throw new NotImplementedException();
        HRESULT IParseDisplayName.Interface.ParseDisplayName(IBindCtx* pbc, PWSTR pszDisplayName, uint* pchEaten, IMoniker** ppmkOut) => throw new NotImplementedException();
    }
}