// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.DotNet;
using Windows.Win32.Foundation;
using Windows.Win32.System.Ole;
using Windows.Win32.System.Variant;
using static Windows.Win32.System.Ole.FDEX_PROP_FLAGS;
using IMarshal = Windows.Win32.System.Com.Marshal.IMarshal;
using InteropMarshal = System.Runtime.InteropServices.Marshal;

namespace Windows.Win32.System.Com;

public unsafe class IReflectTests
{
    // Interestingly IReflect only generates a useful DispatchEx with TypeInfo if the type is *not* public.

    [Fact]
    public void SimpleClass_Public_CanGetDispatch()
    {
        PublicSimpleClass publicSimple = new();

        // Can get IUnknown for any class
        nint unknown = InteropMarshal.GetIUnknownForObject(publicSimple);
        unknown.Should().NotBe(0);

        // Can query it for IDispatch when it is public
        IDispatch* dispatch;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatch>(), (void**)&dispatch);
        hr.Should().Be(HRESULT.S_OK);
        dispatch->Release();

        // Can not query it for IDispatchEx when it is public
        IDispatchEx* dispatchEx;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatchEx>(), (void**)&dispatchEx);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        InteropMarshal.Release(unknown);
    }

    [Fact]
    public void SimpleClass_Public_DispatchBehavior()
    {
        PublicSimpleClass publicSimple = new();

        using ComScope<IDispatch> dispatch = new((IDispatch*)InteropMarshal.GetIDispatchForObject(publicSimple));
        using ComScope<ITypeInfo> typeInfo = new(null);
        HRESULT hr = dispatch.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
        hr.Should().Be(HRESULT.TLBX_E_LIBNOTREGISTERED);
    }

    [Fact]
    public void SimpleClass_Private_CannotGetDispatch()
    {
        PrivateSimpleClass privateSimple = new();

        // Can get IUnknown for any class
        nint unknown = InteropMarshal.GetIUnknownForObject(privateSimple);
        unknown.Should().NotBe(0);

        // Can not query it for IDispatch when it is private
        IDispatch* dispatch;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatch>(), (void**)&dispatch);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        // Can not query it for IDispatchEx when it is private
        IDispatchEx* dispatchEx;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatchEx>(), (void**)&dispatchEx);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        InteropMarshal.Release(unknown);
    }

    [Fact]
    public void IReflect_GetDispatch()
    {
        ReflectClass reflect = new();
        nint unknown = InteropMarshal.GetIUnknownForObject(reflect);
        unknown.Should().NotBe(0);

        // Can query IReflect for IDispatch
        IDispatch* dispatch;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatch>(), (void**)&dispatch);
        hr.Should().Be(HRESULT.S_OK);
        dispatch->Release();

        // Can query IReflect for IDispatchEx
        IDispatchEx* dispatchEx;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatchEx>(), (void**)&dispatchEx);
        hr.Should().Be(HRESULT.S_OK);
        dispatchEx->Release();

        InteropMarshal.Release(unknown);

        // IDispatchEx is generated for Type, EnumBuilder, TypeBuilder, and any class that derives from IReflect.
    }

    [Fact]
    public void IReflect_ISupportErrorInfo()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        ISupportErrorInfo* supportErrorInfo;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<ISupportErrorInfo>(), (void**)&supportErrorInfo);
        hr.Should().Be(HRESULT.S_OK);

        // Always returns true for any value.
        hr = supportErrorInfo->InterfaceSupportsErrorInfo(null);
        hr.Should().Be(HRESULT.S_OK);
        supportErrorInfo->Release();

        // Exceptions support IErrorInfo itself
        IErrorInfo* errorInfo;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IErrorInfo>(), (void**)&errorInfo);
        hr.Should().Be(HRESULT.E_NOINTERFACE);
    }

    [Fact]
    public void IReflect_IConnectionPointContainer_NoAttribute()
    {
        ReflectEvent reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        IConnectionPointContainer* container;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IConnectionPointContainer>(), (void**)&container);
        hr.Should().Be(HRESULT.S_OK);

        // What gets exposed are [ComSourceInterfaces]
        // https://web.archive.org/web/20110704133307/http://msdn.microsoft.com:80/en-us/library/dd8bf0x3(v=VS.100).aspx

        using (ComScope<IEnumConnectionPoints> enumPoints = new(null))
        {
            hr = container->EnumConnectionPoints(enumPoints);
            hr.Should().Be(HRESULT.S_OK);
            IConnectionPoint* connection;
            while (enumPoints.Value->Next(1, &connection, out uint fetched) == HRESULT.S_OK)
            {
                connection->Release();
                Assert.Fail("Shouldn't get any connection points.");
            }
        }

        container->Release();
    }


    [Fact]
    public void IReflect_IConnectionPointContainer_Attributed()
    {
        ReflectSourcedEvent reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        IConnectionPointContainer* container;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IConnectionPointContainer>(), (void**)&container);
        hr.Should().Be(HRESULT.S_OK);

        // What gets exposed are [ComSourceInterfaces]
        // https://web.archive.org/web/20110704133307/http://msdn.microsoft.com:80/en-us/library/dd8bf0x3(v=VS.100).aspx

        using (ComScope<IEnumConnectionPoints> enumPoints = new(null))
        {
            container->EnumConnectionPoints(enumPoints).Succeeded.Should().BeTrue();
            IConnectionPoint* connection;
            while (enumPoints.Value->Next(1, &connection, out uint fetched) == HRESULT.S_OK)
            {
                connection->GetConnectionInterface(out Guid riid).Succeeded.Should().BeTrue();
                IConnectionPoint* foundConnection;
                hr = container->FindConnectionPoint(&riid, &foundConnection);
                Assert.True(connection == foundConnection);
                foundConnection->Release();
                connection->Release();
            }
        }

        container->Release();
    }

    [Fact]
    public void IReflect_IMarshal()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        // In Core, the implementation of IMarshal is simply delegated to CoCreateFreeThreadedMarshaler
        IMarshal* marshal;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IMarshal>(), (void**)&marshal);
        hr.Should().Be(HRESULT.S_OK);
        marshal->Release();
    }

    [Fact]
    public void IReflect_IAgileObject()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        // IAgileObject marks as the object as free-threaded, which will make the Global Interface Table skip
        // marshalling across apartments.
        IAgileObject* agileObject;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IAgileObject>(), (void**)&agileObject);
        hr.Should().Be(HRESULT.S_OK);
        agileObject->Release();
    }

    [Fact]
    public void IReflect_IClassInfo()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        using ComScope<IProvideClassInfo> provideClassInfo = new(null);
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IProvideClassInfo>(), provideClassInfo);
        hr.Should().Be(HRESULT.S_OK);

        IProvideClassInfo2* provideClassInfo2;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IProvideClassInfo2>(), (void**)&provideClassInfo2);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        // The Assembly needs to have a registered TypeLib to get anything.
        ITypeInfo* typeInfo;
        hr = provideClassInfo.Value->GetClassInfo(&typeInfo);
        hr.Should().Be(HRESULT.TLBX_E_LIBNOTREGISTERED);
    }

    [Fact]
    public void IReflect_IManagedObject()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));

        // IManagedObject is a .NET Framework only thing, it isn't used/implemented in .NET Core. Other interfaces that
        // don't exist in .NET Core: IObjectSafety, IWeakReferenceSource, ICustomPropertyProvider, ICCW, and IStringTable.
        IManagedObject* managedObject;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IManagedObject>(), (void**)&managedObject);
        hr.Should().Be(HRESULT.E_NOINTERFACE);
    }

    [Fact]
    public void IReflect_DoesNotImpactGetType()
    {
        ReflectClass reflect = new();
        Type type = reflect.GetType();
        var memberInfo = type.GetMembers();

        SubMemberClass subReflect = new();
        Type subType = subReflect.GetType();
        var subMemberInfo = subType.GetMembers();

        memberInfo.Length.Should().Be(subMemberInfo.Length);
    }

    [Fact]
    public void IReflect_InvokeMember()
    {
        InvokeMemberClass invoke = new();
        using ComScope<IDispatch> dispatch = new((IDispatch*)InteropMarshal.GetIDispatchForObject(invoke));
        using ComScope<IDispatchEx> dispatchEx = dispatch.TryQueryInterface<IDispatchEx>(out HRESULT hr);

        using ComScope<ITypeInfo> typeInfo = new(null);
        hr = dispatchEx.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
        hr.Succeeded.Should().Be(true);

        using BSTR findName = new("Foo");
        hr = dispatchEx.Value->GetDispID(findName, default, out int pid);

        TYPEATTR* attr;
        hr = typeInfo.Value->GetTypeAttr(&attr);
        hr.Succeeded.Should().Be(true);
        TYPEATTR attrCopy = *attr;
        typeInfo.Value->ReleaseTypeAttr(attr);

        VARIANT result;
        DISPPARAMS dispparams = default;
        hr = dispatchEx.Value->Invoke(
            1776,
            IID.Empty(),
            0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispparams,
            &result,
            null,
            null);

        hr = dispatchEx.Value->InvokeEx(
            1776,
            0,
            (ushort)DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispparams,
            &result,
            null,
            null);
    }

    [Fact]
    public void IReflect_Enumerate_ReflectSelf()
    {
        ReflectSelf reflect = new();

        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatchEx = unknown.QueryInterface<IDispatchEx>();

        // When Invoking via enumerated ids "ToString" is passed to IReflect.InvokeMember (as opposed to "[DISPID=]").

        var dispatchIds = dispatchEx.Value->GetAllDispatchIds();
        dispatchIds.Keys.Should().BeEquivalentTo("GetType", "ToString", "Equals", "GetHashCode");

        using VARIANT result = default;
        DISPPARAMS dispparams = default;
        HRESULT hr = dispatchEx.Value->InvokeEx(
            dispatchIds["ToString"],
            0,
            (ushort)DISPATCH_FLAGS.DISPATCH_METHOD,
            &dispparams,
            &result,
            null,
            null);

        hr.Succeeded.Should().BeTrue();

        result.vt.Should().Be(VARENUM.VT_BSTR);
        result.data.bstrVal.ToString().Should().Be("Windows.Win32.System.Com.IReflectTests+ReflectSelf");

        result.Dispose();

        // Invoke works as well.
        hr = dispatchEx.Value->Invoke(
            dispatchIds["ToString"],
            IID.Empty(),
            0,
            DISPATCH_FLAGS.DISPATCH_METHOD,
            &dispparams,
            &result,
            null,
            null);

        hr.Succeeded.Should().BeTrue();

        result.vt.Should().Be(VARENUM.VT_BSTR);
        result.data.bstrVal.ToString().Should().Be("Windows.Win32.System.Com.IReflectTests+ReflectSelf");

        result.Dispose();

        // Try again with QI'ed IDispatch
        using ComScope<IDispatch> dispatch = unknown.QueryInterface<IDispatch>();

        hr = dispatch.Value->Invoke(
            dispatchIds["ToString"],
            IID.Empty(),
            0,
            DISPATCH_FLAGS.DISPATCH_METHOD,
            &dispparams,
            &result,
            null,
            null);

        hr.Succeeded.Should().BeTrue();

        result.vt.Should().Be(VARENUM.VT_BSTR);
        result.data.bstrVal.ToString().Should().Be("Windows.Win32.System.Com.IReflectTests+ReflectSelf");

        // Can we get any more info off of IDispatch? Everything but GetDocumentation takes an index, not an id.
        using ComScope<ITypeInfo> typeInfo = new(null);
        dispatchEx.Value->GetTypeInfo(0, 0, typeInfo);

        using BSTR name = default;
        using BSTR doc = default;
        using BSTR helpFile = default;
        uint helpContext;
        hr = typeInfo.Value->GetDocumentation(dispatchIds["ToString"], &name, &doc, &helpContext, &helpFile);
        hr.Should().Be(HRESULT.TYPE_E_ELEMENTNOTFOUND);
    }

    public static TheoryData<object?, VARENUM> ObjectBehaviorTestData => new()
    {
        { null, VARENUM.VT_EMPTY },
        { new object(), VARENUM.VT_DISPATCH },
        { 42, VARENUM.VT_I4 },
        // Structs aren't normally handled - returns COR_E_NOTSUPPORTED
        { new Point(1, 2), VARENUM.VT_ILLEGAL },
        // System.Drawing.Color has special handling
        { Color.Blue, VARENUM.VT_UI4 },
        { new int[] { 1, 2 }, VARENUM.VT_ARRAY | VARENUM.VT_I4 },
    };

    [Theory, MemberData(nameof(ObjectBehaviorTestData))]
    public void IReflect_ObjectBehavior(object? obj, VARENUM expected)
    {
        ReflectObjectTypes reflect = new();

        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatch = unknown.QueryInterface<IDispatchEx>();

        var dispatchIds = dispatch.Value->GetAllDispatchIds();

        dispatch.Value->GetMemberProperties(dispatchIds["Object"], uint.MaxValue, out var flags);
        flags.Should().Be(fdexPropCanGet | fdexPropCanPut | fdexPropCannotPutRef
            | fdexPropCannotCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents);

        // Somewhat surprisingly fields do not come back as a System.Reflection.BindingFlags.GetField. They come
        // back as GetProperty, which would require IReflect.Invoke to understand and handle appropriately.

        reflect.Object = obj;
        reflect.ObjectAsInterface = obj;

        using VARIANT result = dispatch.Value->TryGetPropertyValue(dispatchIds["Object"], out HRESULT hr);
        if (expected == VARENUM.VT_ILLEGAL)
        {
            hr.Succeeded.Should().BeFalse();
        }
        else
        {
            hr.Succeeded.Should().BeTrue();
            result.vt.Should().Be(expected);
        }

        using VARIANT result2 = dispatch.Value->TryGetPropertyValue(dispatchIds["ObjectAsInterface"], out hr);
        if (expected == VARENUM.VT_ILLEGAL)
        {
            hr.Succeeded.Should().BeFalse();
        }
        else
        {
            hr.Succeeded.Should().BeTrue();
            result2.vt.Should().Be(expected);
        }
    }

    [Fact]
    public void IReflect_NonObjectBehavior()
    {
        ReflectNonObjectTypes reflect = new()
        {
            Color = Color.Blue
        };

        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatch = unknown.QueryInterface<IDispatchEx>();

        var dispatchIds = dispatch.Value->GetAllDispatchIds();

        dispatchIds.Keys.Should().Contain("Location", "get_Location", "Color", "get_Color", "set_Color");

        VARIANT result = dispatch.Value->TryGetPropertyValue(dispatchIds["Location"], out HRESULT hr);
        hr.Should().Be(HRESULT.COR_E_NOTSUPPORTED);

        VARIANT value = (VARIANT)42;
        hr = dispatch.Value->TrySetPropertyValue(dispatchIds["Count"], value);
        hr.Succeeded.Should().BeTrue();
        reflect.Count.Should().Be(42);

        result = dispatch.Value->TryGetPropertyValue(dispatchIds["Color"], out hr);
        result.vt.Should().Be(VARENUM.VT_UI4);
        ((uint)result).Should().Be(0x00FF0000);  // AABBGGRR OLECOLOR

        // While we can *get* the color, we can't set it as there is no way (afaik) to match the passed in uint parameter.
        value = (VARIANT)(uint)0x000000FF;
        hr = dispatch.Value->TrySetPropertyValue(dispatchIds["Color"], value);
        hr.Should().Be(HRESULT.DISP_E_MEMBERNOTFOUND);

        dispatch.Value->GetMemberProperties(dispatchIds["Color"], uint.MaxValue, out var flags);
        flags.Should().Be(fdexPropCanGet | fdexPropCanPut | fdexPropCannotPutRef
            | fdexPropCannotCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents);

        dispatch.Value->GetMemberProperties(dispatchIds["get_Color"], uint.MaxValue, out flags);
        flags.Should().Be(fdexPropCannotGet | fdexPropCannotPut | fdexPropCannotPutRef
            | fdexPropCanCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents);

        dispatch.Value->GetMemberProperties(dispatchIds["set_Color"], uint.MaxValue, out flags);
        flags.Should().Be(fdexPropCannotGet | fdexPropCannotPut | fdexPropCannotPutRef
            | fdexPropCanCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents);
    }

    public static TheoryData<object, IEnumerable<string>> EnumerateBehaviorTestData => new()
    {
        { new ReflectClass(), Array.Empty<string>() },
        { new WithInterfaceClass(), Array.Empty<string>() },
        { new SubMemberClass(), Array.Empty<string>() },
        { new SubMethodClass(), new string[] { "ToString" } },
        { new ReflectSelf(), new string[] { "GetType", "ToString", "Equals", "GetHashCode" } }
    };

    [Theory, MemberData(nameof(EnumerateBehaviorTestData))]
    public void IReflect_IDispatchEx_Enumerate(object reflect, IEnumerable<string> names)
    {
        using ComScope<IUnknown> unknown = new((IUnknown*)InteropMarshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatch = unknown.QueryInterface<IDispatchEx>();

        // Only explicitly provided member info in IReflect is exposed.

        var dispatchIds = dispatch.Value->GetAllDispatchIds();
        dispatchIds.Keys.Should().BeEquivalentTo(names);
    }

    public static TheoryData<object> ReflectClasses => new()
    {
        new ReflectClass(),
        new WithInterfaceClass(),
        new SubMemberClass(),
        new SubMethodClass(),
        new ReflectSelf()
    };

    [Theory, MemberData(nameof(ReflectClasses))]
    public void IReflect_IDispatchDefaultBehavior(object reflect)
    {
        // All we ever see via IDispatch are the IUnknown methods.

        // Getting IDispatch calls IReflect.GetProperties, IReflect.GetFields, then IReflect.GetMethods
        using ComScope<IDispatch> dispatch = new((IDispatch*)InteropMarshal.GetIDispatchForObject(reflect));
        HRESULT hr = dispatch.Value->GetTypeInfoCount(out uint count);
        hr.Should().Be(HRESULT.S_OK);
        count.Should().Be(1);

        using ComScope<ITypeInfo> typeInfo = new(null);
        hr = dispatch.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
        hr.Should().Be(HRESULT.S_OK);

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

        // This will call IReflect.InvokeMember with a name of "[DISPID=0]" (the member dispid)
        VARIANT result;
        DISPPARAMS dispparams = default;
        hr = dispatch.Value->Invoke(0, IID.Empty(), 0, DISPATCH_FLAGS.DISPATCH_METHOD, &dispparams, &result, null, null);
    }

    public class PublicSimpleClass
    {
    }

    private class PrivateSimpleClass
    {
    }

    private class SubMethodClass : ReflectClass
    {
        protected override MethodInfo[] GetMethods(BindingFlags bindingAttr) => new MethodInfo[]
        {
            typeof(object).GetMethod("ToString")!
        };
    }

    private class SubMemberClass : ReflectClass
    {
        protected override MemberInfo[] GetMembers(BindingFlags bindingAttr) => new MemberInfo[]
        {
            new MyMemberInfo()
        };

        private class MyMemberInfo : MemberInfo
        {
            public override Type? DeclaringType => typeof(SubMemberClass);

            public override MemberTypes MemberType => MemberTypes.Property;

            public override string Name => "Test This";

            public override Type? ReflectedType => typeof(SubMemberClass);

            public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();
            public override bool IsDefined(Type attributeType, bool inherit) => false;
        }
    }

    private class WithInterfaceClass : ReflectClass, IComCallableWrapper.Interface
    {
    }

    private class InvokeMemberClass : ReflectClass, IMyDispatch
    {
        public string? LastName { get; set; }

        [ComVisible(true)]
        public bool Snipe = true;

        [ComVisible(true)]
        [DispId(1776)]
        public bool Represent { get; } = true;

#pragma warning disable CA1822 // Mark members as static
        [ComVisible(true)]
        public bool Foo() => true;

        [DispId(1066)]
        public bool Bar() => true;
#pragma warning restore CA1822

        protected override object? InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder? binder,
            object? target,
            object?[]? args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? namedParameters)
        {
            LastName = name;
            return base.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        public bool DoThatThing() => throw new NotImplementedException();
    }

    [ComImport]
    [ComVisible(true)]
    [Guid("8E6E2EF8-CE8D-4F2A-8481-A47474FFB808")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMyDispatch
    {
        [DispId(2020)]
        bool DoThatThing();
    }

    public delegate void ClickDelegate();

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IClickEvents
    {
        void Click();
    }

    private class ReflectNonObjectTypes : ReflectSelf
    {
        public Point Location { get; internal set; }
        public Color Color { get; set; }
        public int Count { get; set; }
    }

    private class ReflectObjectTypes : ReflectSelf
    {
        public object? ObjectAsInterface
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
            internal set;
        }

        public object? Object { get; internal set ; }
    }

    [ComVisible(true)]
    [ComSourceInterfaces(typeof(IClickEvents))]
    private class ReflectSourcedEvent : ReflectSelf
    {
#pragma warning disable CS0067
        public event ClickDelegate? Click;
#pragma warning restore CS0067
    }

    [ComVisible(true)]
    private class ReflectEvent : ReflectSelf
    {
#pragma warning disable CS0067
        public event ClickDelegate? Click;
#pragma warning restore CS0067
    }

    private class ReflectSelf : ReflectClass
    {
        protected override Type UnderlyingSystemType => GetType();

        protected override FieldInfo? GetField(string name, BindingFlags bindingAttr)
        {
            return GetType().GetField(name, bindingAttr);
        }

        protected override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            FieldInfo[] fields = GetType().GetFields(bindingAttr);
            return fields;
        }

        protected override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return GetType().GetMember(name, bindingAttr);
        }

        protected override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return GetType().GetMembers(bindingAttr);
        }

        protected override MethodInfo? GetMethod(string name, BindingFlags bindingAttr)
        {
            return GetType().GetMethod(name, bindingAttr);
        }

        protected override MethodInfo? GetMethod(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type[] types,
            ParameterModifier[]? modifiers)
        {
            return GetType().GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        protected override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            MethodInfo[] methods = GetType().GetMethods(bindingAttr);
            return methods;
        }

        protected override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            PropertyInfo[] properties = GetType().GetProperties(bindingAttr);
            return properties;
        }

        protected override PropertyInfo? GetProperty(string name, BindingFlags bindingAttr)
        {
            return GetType().GetProperty(name, bindingAttr);
        }

        protected override PropertyInfo? GetProperty(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type? returnType,
            Type[] types,
            ParameterModifier[]? modifiers)
        {
            return GetType().GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected override object? InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder? binder,
            object? target,
            object?[]? args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? namedParameters)
        {
            try
            {
                object? result = GetType().InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
                return result;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }

    private class ReflectClass : IReflect
    {
        Type IReflect.UnderlyingSystemType => UnderlyingSystemType;
        protected virtual Type UnderlyingSystemType => null!;

        FieldInfo? IReflect.GetField(string name, BindingFlags bindingAttr) => GetField(name, bindingAttr);
        protected virtual FieldInfo? GetField(string name, BindingFlags bindingAttr) => null;

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr) => GetFields(bindingAttr);
        protected virtual FieldInfo[] GetFields(BindingFlags bindingAttr) => Array.Empty<FieldInfo>();

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr) => GetMember(name, bindingAttr);
        protected virtual MemberInfo[] GetMember(string name, BindingFlags bindingAttr) => Array.Empty<MemberInfo>();

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr) => GetMembers(bindingAttr);
        protected virtual MemberInfo[] GetMembers(BindingFlags bindingAttr) => Array.Empty<MemberInfo>();

        MethodInfo? IReflect.GetMethod(string name, BindingFlags bindingAttr) => GetMethod(name, bindingAttr);
        protected virtual MethodInfo? GetMethod(string name, BindingFlags bindingAttr) => null;

        MethodInfo? IReflect.GetMethod(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type[] types,
            ParameterModifier[]? modifiers) => GetMethod(name, bindingAttr, binder, types, modifiers);
        protected virtual MethodInfo? GetMethod(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type[] types,
            ParameterModifier[]? modifiers) => null;

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr) => GetMethods(bindingAttr);
        protected virtual MethodInfo[] GetMethods(BindingFlags bindingAttr) => Array.Empty<MethodInfo>();

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr) => GetProperties(bindingAttr);
        protected virtual PropertyInfo[] GetProperties(BindingFlags bindingAttr) => Array.Empty<PropertyInfo>();

        PropertyInfo? IReflect.GetProperty(string name, BindingFlags bindingAttr) => GetProperty(name, bindingAttr);
        protected virtual PropertyInfo? GetProperty(string name, BindingFlags bindingAttr) => null;

        PropertyInfo? IReflect.GetProperty(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type? returnType,
            Type[] types,
            ParameterModifier[]? modifiers) => GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        protected virtual PropertyInfo? GetProperty(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type? returnType,
            Type[] types,
            ParameterModifier[]? modifiers) => null;

        object? IReflect.InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder? binder,
            object? target,
            object?[]? args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? namedParameters)
            => InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        protected virtual object? InvokeMember(
            string name,
            BindingFlags invokeAttr,
            Binder? binder,
            object? target,
            object?[]? args,
            ParameterModifier[]? modifiers,
            CultureInfo? culture,
            string[]? namedParameters) => null;
    }
}
