// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;
using IServiceProvider = Windows.Win32.System.Com.IServiceProvider;

namespace Tests.Windows.Win32.UI.Accessibility;

public unsafe class CreateStdAccessibleObjectTests
{
    [StaFact]
    public void CreateStdAccessibleObject_Window()
    {
        // Default accessibility objects implement the following publicly documented interfaces:
        //
        //      IAccessible, IEnumVARIANT, IOleWindow, IServiceProvider, IAccIdentity

        using Window window = new(Window.DefaultBounds);

        // using ComScope<IAccessible> accessible = new(null);
        // HRESULT hr = Interop.AccessibleObjectFromWindow(
        //     window,
        //     (uint)OBJECT_IDENTIFIER.OBJID_WINDOW,
        //     IID.Get<IDispatch>(),
        //     accessible);

        HRESULT hr = Interop.CreateStdAccessibleObject(
            window.Handle,
            (int)OBJECT_IDENTIFIER.OBJID_WINDOW,
            IID.GetRef<IAccessible>(),
            out void* ppvObject);

        hr.Succeeded.Should().BeTrue();
        using ComScope<IAccessible> accessible = new(ppvObject);

        // For OBJID_WINDOW the child count is always 7 (OBJID_SYSMENU through OBJID_SIZEGRIP)
        accessible.Value->get_accChildCount(out int childCount).Succeeded.Should().BeTrue();
        childCount.Should().Be(7);

        using BSTR description = default;
        accessible.Value->get_accDescription((VARIANT)(int)Interop.CHILDID_SELF, &description).Should().Be(HRESULT.S_FALSE);
        accessible.Value->get_accDescription((VARIANT)(int)OBJECT_IDENTIFIER.OBJID_SYSMENU, &description).Succeeded.Should().BeTrue();
        description.ToStringAndFree().Should().Be("Contains commands to manipulate the window");

        // Navigating left from the system menu goes nowhere.
        VARIANT result = accessible.Value->accNavigate((int)Interop.NAVDIR_LEFT, (VARIANT)(int)OBJECT_IDENTIFIER.OBJID_SYSMENU);
        result.vt.Should().Be(VARENUM.VT_EMPTY);

        // We get IDispatch for the title bar going right from the system menu
        result = accessible.Value->accNavigate((int)Interop.NAVDIR_RIGHT, (VARIANT)(int)OBJECT_IDENTIFIER.OBJID_SYSMENU);
        using ComScope<IDispatch> right = new((IDispatch*)result);

        // Can't directly get IAccessibleEx / IRawElementProviderSimple
        right.QueryInterface<IAccessibleEx>().IsNull.Should().BeTrue();
        right.QueryInterface<IRawElementProviderSimple>().IsNull.Should().BeTrue();

        using (ComScope<IServiceProvider> rightService = right.QueryInterface<IServiceProvider>())
        {
            rightService.IsNull.Should().BeFalse();
            using ComScope<IRawElementProviderSimple> provider = new(null);
            rightService.Value->QueryService(IID.Get<IAccessibleEx>(), IID.Get<IRawElementProviderSimple>(), provider);
            provider.IsNull.Should().BeFalse();
            VARIANT property = provider.Value->GetPropertyValue(UIA_PROPERTY_ID.UIA_IsContentElementPropertyId);
            ((bool)property).Should().BeFalse();
            property = provider.Value->GetPropertyValue(UIA_PROPERTY_ID.UIA_AutomationIdPropertyId);
            property.IsEmpty.Should().BeTrue();
        }

        using ComScope<IEnumVARIANT> enumVariant = accessible.QueryInterface<IEnumVARIANT>(out hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IOleWindow> oleWindow = accessible.QueryInterface<IOleWindow>(out hr);
        hr.Succeeded.Should().BeTrue();
        oleWindow.Value->GetWindow(out HWND hwnd);

        hwnd.Should().Be(window.Handle);

        using ComScope<IServiceProvider> serviceProvider = accessible.QueryInterface<IServiceProvider>(out hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IAccIdentity> identity = accessible.QueryInterface<IAccIdentity>(out hr);
        hr.Succeeded.Should().BeTrue();

        byte* id = default;
        uint length;
        identity.Value->GetIdentityString(Interop.CHILDID_SELF, &id, &length);
        Span<byte> idSpan = new(id, (int)length);
        idSpan.IsEmpty.Should().BeFalse();
        Interop.CoTaskMemFree(id);
    }
}

public unsafe class CreateStdAccessibleProxyTests
{
    [StaFact]
    public void CreateStdAccessibleProxy_Button()
    {
        using Window window = new(Window.DefaultBounds);
        HRESULT hr = Interop.CreateStdAccessibleProxy(
            window.Handle,
            "BUTTON",
            (int)OBJECT_IDENTIFIER.OBJID_CLIENT,
            IID.GetRef<IAccessible>(),
            out void* ppvObject);

        hr.Succeeded.Should().BeTrue();
        using ComScope<IAccessible> accessible = new(ppvObject);
    }

    [StaFact]
    public void CreateStdAccessibleProxy_GetIdsOfNames()
    {
        using Window window = new(Window.DefaultBounds);
        HRESULT hr = Interop.CreateStdAccessibleProxy(
            window.Handle,
            "BUTTON",
            (int)OBJECT_IDENTIFIER.OBJID_CLIENT,
            IID.GetRef<IAccessible>(),
            out void* ppvObject);

        hr.Succeeded.Should().BeTrue();
        using ComScope<IAccessible> accessible = new(ppvObject);
        IDispatch* dispatch = (IDispatch*)accessible;

        // Get a member name, capitalized correctly
        int[] result = dispatch->GetIDsOfNames("accDoDefaultAction");
        result.Should().HaveCount(1);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);

        // Case insensitive
        result = dispatch->GetIDsOfNames("ACCDoDefaultAction");
        result.Should().HaveCount(1);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);

        // With argument
        result = dispatch->GetIDsOfNames("accDoDefaultAction", "varChild");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(0);

        // With argument, casing
        result = dispatch->GetIDsOfNames("accDoDefaultAction", "VARChild");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(0);

        // With invalid argument
        result = dispatch->GetIDsOfNames("accDoDefaultAction", "FLOOP");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(Interop.DISPID_UNKNOWN);

        // Args are sequentially numbered, return value is not recognized as parameter
        result = dispatch->GetIDsOfNames("accHitTest", "xLeft", "yTop", "pvarChild");
        result.Should().HaveCount(4);
        result[0].Should().Be(Interop.DISPID_ACC_HITTEST);
        result[1].Should().Be(0);
        result[2].Should().Be(1);
        result[3].Should().Be(Interop.DISPID_UNKNOWN);
    }

    [StaFact]
    public void CreateStdAccessibleProxy_GetTypeInfo()
    {
        using Window window = new(Window.DefaultBounds);
        HRESULT hr = Interop.CreateStdAccessibleProxy(
            window.Handle,
            "BUTTON",
            (int)OBJECT_IDENTIFIER.OBJID_CLIENT,
            IID.GetRef<IAccessible>(),
            out void* ppvObject);

        hr.Succeeded.Should().BeTrue();
        using ComScope<IAccessible> accessible = new(ppvObject);
        IDispatch* dispatch = (IDispatch*)accessible;

        dispatch->GetTypeInfoCount(out uint count);
        count.Should().Be(1);

        using ComScope<ITypeInfo> typeInfo = new(null);
        dispatch->GetTypeInfo(0, 0, typeInfo);
        typeInfo.IsNull.Should().BeFalse();

        typeInfo.Value->GetTypeAttr(out TYPEATTR* pTypeAttr);
        try
        {
            pTypeAttr->cFuncs.Should().Be(28);
            pTypeAttr->cImplTypes.Should().Be(1);
            pTypeAttr->guid.Should().Be(IAccessible.IID_Guid);
            pTypeAttr->wTypeFlags.Should().Be((ushort)(TYPEFLAGS.TYPEFLAG_FDISPATCHABLE
                | TYPEFLAGS.TYPEFLAG_FDUAL | TYPEFLAGS.TYPEFLAG_FHIDDEN));
            pTypeAttr->typekind.Should().Be(TYPEKIND.TKIND_DISPATCH);
        }
        finally
        {
            typeInfo.Value->ReleaseTypeAttr(pTypeAttr);
        }

        using ComScope<ITypeLib> typelib = new(null);
        typeInfo.Value->GetContainingTypeLib(typelib, out uint index);
        typelib.Value->GetLibAttr(out TLIBATTR* pLibAttr);
        try
        {
            // This is the Accessibility type library
            pLibAttr->guid.Should().Be(new Guid("1ea4dbf0-3c3b-11cf-810c-00aa00389b71"));
            pLibAttr->syskind.Should().Be(Environment.Is64BitProcess ? SYSKIND.SYS_WIN64 : SYSKIND.SYS_WIN32);
            pLibAttr->wLibFlags.Should().Be((ushort)(LIBFLAGS.LIBFLAG_FHIDDEN | LIBFLAGS.LIBFLAG_FHASDISKIMAGE));
            pLibAttr->wMajorVerNum.Should().Be(1);
            pLibAttr->wMinorVerNum.Should().Be(1);
        }
        finally
        {
            typelib.Value->ReleaseTLibAttr(pLibAttr);
        }
    }
}