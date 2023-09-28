// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32.UI.Accessibility;

[Collection("Accessibility")]
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
        int[] result = dispatch->GetIdsOfNames("accDoDefaultAction");
        result.Should().HaveCount(1);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);

        // Case insensitive
        result = dispatch->GetIdsOfNames("ACCDoDefaultAction");
        result.Should().HaveCount(1);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);

        // With argument
        result = dispatch->GetIdsOfNames("accDoDefaultAction", "varChild");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(0);

        // With argument, casing
        result = dispatch->GetIdsOfNames("accDoDefaultAction", "VARChild");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(0);

        // With invalid argument
        result = dispatch->GetIdsOfNames("accDoDefaultAction", "FLOOP");
        result.Should().HaveCount(2);
        result[0].Should().Be(Interop.DISPID_ACC_DODEFAULTACTION);
        result[1].Should().Be(Interop.DISPID_UNKNOWN);

        // Args are sequentially numbered, return value is not recognized as parameter
        result = dispatch->GetIdsOfNames("accHitTest", "xLeft", "yTop", "pvarChild");
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