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

[Collection("Accessibility")]
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
        right.TryQueryInterface<IAccessibleEx>().IsNull.Should().BeTrue();
        right.TryQueryInterface<IRawElementProviderSimple>().IsNull.Should().BeTrue();

        using (ComScope<IServiceProvider> rightService = right.QueryInterface<IServiceProvider>())
        {
            rightService.IsNull.Should().BeFalse();
            using ComScope<IRawElementProviderSimple> provider = new(null);
            rightService.Value->QueryService(IID.Get<IAccessibleEx>(), IID.Get<IRawElementProviderSimple>(), provider);
            provider.IsNull.Should().BeFalse();
            provider.Value->GetPropertyValue(UIA_PROPERTY_ID.UIA_IsContentElementPropertyId, out VARIANT property).Succeeded.Should().BeTrue();
            ((bool)property).Should().BeFalse();
            provider.Value->GetPropertyValue(UIA_PROPERTY_ID.UIA_AutomationIdPropertyId, out property).Succeeded.Should().BeTrue();
            property.IsEmpty.Should().BeTrue();
        }

        using ComScope<IEnumVARIANT> enumVariant = accessible.TryQueryInterface<IEnumVARIANT>(out hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IOleWindow> oleWindow = accessible.TryQueryInterface<IOleWindow>(out hr);
        hr.Succeeded.Should().BeTrue();
        oleWindow.Value->GetWindow(out HWND hwnd);

        hwnd.Should().Be(window.Handle);

        using ComScope<IServiceProvider> serviceProvider = accessible.TryQueryInterface<IServiceProvider>(out hr);
        hr.Succeeded.Should().BeTrue();

        using ComScope<IAccIdentity> identity = accessible.TryQueryInterface<IAccIdentity>(out hr);
        hr.Succeeded.Should().BeTrue();

        byte* id = default;
        uint length;
        identity.Value->GetIdentityString(Interop.CHILDID_SELF, &id, &length);
        Span<byte> idSpan = new(id, (int)length);
        idSpan.IsEmpty.Should().BeFalse();
        Interop.CoTaskMemFree(id);
    }
}