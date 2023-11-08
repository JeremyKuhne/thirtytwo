// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Win32.Foundation;
using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe class StandardDispatchTests
{
    [Fact]
    public void StandardDispatch_ImplDoesNotProvideEx()
    {
        // Load the registered type library and get the relevant ITypeInfo for the specified interface.
        using ComScope<ITypeLib> typelib = new(null);
        HRESULT hr = Interop.LoadRegTypeLib(
            new Guid("00020430-0000-0000-C000-000000000046"),
            2,
            0,
            0,
            typelib);

        hr.ThrowOnFailure();

        using ComScope<ITypeInfo> typeinfo = new(null);
        typelib.Value->GetTypeInfoOfGuid(IUnknown.IID_Guid, typeinfo);

        IUnknown* unknown = ComHelpers.GetComPointer<IUnknown>(new OleWindow());

        // The unknown we get is a wrapper unknown.
        unknown->QueryInterface(IUnknown.IID_Guid, out void* instance).ThrowOnFailure();
        IUnknown* standard = default;
        Interop.CreateStdDispatch(
            unknown,
            instance,
            typeinfo.Value,
            &standard).ThrowOnFailure();

        // StdDispatch does not provide an implementation of IDispatchEx.
        IDispatchEx* dispatchEx = standard->QueryInterface<IDispatchEx>();
        Assert.True(dispatchEx is null);

        standard->Release();
        unknown->Release();
    }

    public class OleWindow : IOleWindow.Interface, IManagedWrapper<IOleWindow>
    {
        HRESULT IOleWindow.Interface.GetWindow(HWND* phwnd) => HRESULT.E_NOTIMPL;
        HRESULT IOleWindow.Interface.ContextSensitiveHelp(BOOL fEnterMode) => HRESULT.E_NOTIMPL;
    }

    [Fact]
    public void StandardDispatch_IUnknown()
    {
        SimpleDispatch unknownDispatch = new();
        using ComScope<IDispatch> dispatch = new(ComHelpers.GetComPointer<IDispatch>(unknownDispatch));
        using ComScope<IDispatchEx> dispatchEx = dispatch.TryQueryInterface<IDispatchEx>(out HRESULT hr);

        using ComScope<ITypeInfo> typeInfo = new(null);
        hr = dispatch.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
        hr.Should().Be(HRESULT.S_OK);

        // This all matches what we get off of an IReflect dispatch through COM interop

        TYPEATTR* attr;
        hr = typeInfo.Value->GetTypeAttr(&attr);
        hr.Succeeded.Should().Be(true);
        TYPEATTR attrCopy = *attr;
        typeInfo.Value->ReleaseTypeAttr(attr);

        attrCopy.cFuncs.Should().Be(3);
        attrCopy.cImplTypes.Should().Be(0);
        attrCopy.cVars.Should().Be(0);
        attrCopy.cbAlignment.Should().Be((ushort)sizeof(nint));
        attrCopy.guid.Should().Be(IUnknown.IID_Guid);
        attrCopy.lcid.Should().Be(0);
        Assert.True(attrCopy.lpstrSchema.Value is null);
        attrCopy.memidConstructor.Should().Be(-1);
        attrCopy.memidDestructor.Should().Be(-1);
        attrCopy.typekind.Should().Be(TYPEKIND.TKIND_INTERFACE);
        attrCopy.wMajorVerNum.Should().Be(0);
        attrCopy.wMinorVerNum.Should().Be(0);
        attrCopy.wTypeFlags.Should().Be((ushort)TYPEFLAGS.TYPEFLAG_FHIDDEN);

        using ComScope<ITypeComp> typeComp = new(null);
        hr = typeInfo.Value->GetTypeComp(typeComp);
        hr.Succeeded.Should().BeTrue();

        VARDESC* vardesc;
        hr = typeInfo.Value->GetVarDesc(1, &vardesc);
        hr.Should().Be(HRESULT.TYPE_E_ELEMENTNOTFOUND);

        FUNCDESC* funcdesc;
        hr = typeInfo.Value->GetFuncDesc(0, &funcdesc);
        hr.Succeeded.Should().BeTrue();
        funcdesc->cParams.Should().Be(2);
        funcdesc->memid.Should().Be(0x60000000);

        // Return value
        funcdesc->elemdescFunc.tdesc.vt.Should().Be(VARENUM.VT_HRESULT);
        funcdesc->wFuncFlags.Should().Be(FUNCFLAGS.FUNCFLAG_FRESTRICTED);

        // First parameter
        funcdesc->lprgelemdescParam[0].Anonymous.paramdesc.wParamFlags.Should().Be(PARAMFLAGS.PARAMFLAG_FIN);
        funcdesc->lprgelemdescParam[0].tdesc.vt.Should().Be(VARENUM.VT_PTR);
        funcdesc->lprgelemdescParam[0].tdesc.Anonymous.lptdesc->vt.Should().Be(VARENUM.VT_USERDEFINED);
        funcdesc->lprgelemdescParam[0].tdesc.Anonymous.lptdesc->Anonymous.hreftype.Should().Be(0);

        // Second parameter
        funcdesc->lprgelemdescParam[1].Anonymous.paramdesc.wParamFlags.Should().Be(PARAMFLAGS.PARAMFLAG_FOUT);
        funcdesc->lprgelemdescParam[1].tdesc.vt.Should().Be(VARENUM.VT_PTR);
        funcdesc->lprgelemdescParam[1].tdesc.Anonymous.lptdesc->vt.Should().Be(VARENUM.VT_PTR);
        funcdesc->lprgelemdescParam[1].tdesc.Anonymous.lptdesc->Anonymous.lptdesc->vt.Should().Be(VARENUM.VT_VOID);

        typeInfo.Value->ReleaseFuncDesc(funcdesc);

        using BSTR name = default;
        using BSTR doc = default;
        using BSTR helpFile = default;
        uint helpContext;

        hr = typeInfo.Value->GetDocumentation(0x60000000, &name, &doc, &helpContext, &helpFile);
        name.ToStringAndFree().Should().Be("QueryInterface");
    }

    private class SimpleDispatch : UnknownDispatch, IManagedWrapper<IDispatch>
    {
    }
}
