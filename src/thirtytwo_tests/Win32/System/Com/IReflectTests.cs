// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.DotNet;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.Marshal;
using Windows.Win32.System.Ole;

namespace Tests.Windows.Win32.System.Com;

public unsafe class IReflectTests
{
    // Interestingly IReflect only generates a useful DispatchEx with TypeInfo if the type is *not* public.

    [Fact]
    public void SimpleClass_Public_CanGetDispatch()
    {
        PublicSimpleClass publicSimple = new();

        // Can get IUnknown for any class
        nint unknown = Marshal.GetIUnknownForObject(publicSimple);
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

        Marshal.Release(unknown);
    }

    [Fact]
    public void SimpleClass_Public_DispatchBehavior()
    {
        PublicSimpleClass publicSimple = new();

        using ComScope<IDispatch> dispatch = new((IDispatch*)Marshal.GetIDispatchForObject(publicSimple));
        using ComScope<ITypeInfo> typeInfo = new(null);
        HRESULT hr = dispatch.Value->GetTypeInfo(0, Interop.GetThreadLocale(), typeInfo);
        hr.Should().Be(HRESULT.TLBX_E_LIBNOTREGISTERED);
    }

    [Fact]
    public void SimpleClass_Private_CannotGetDispatch()
    {
        PrivateSimpleClass privateSimple = new();

        // Can get IUnknown for any class
        nint unknown = Marshal.GetIUnknownForObject(privateSimple);
        unknown.Should().NotBe(0);

        // Can not query it for IDispatch when it is private
        IDispatch* dispatch;
        HRESULT hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatch>(), (void**)&dispatch);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        // Can not query it for IDispatchEx when it is private
        IDispatchEx* dispatchEx;
        hr = ((IUnknown*)unknown)->QueryInterface(IID.Get<IDispatchEx>(), (void**)&dispatchEx);
        hr.Should().Be(HRESULT.E_NOINTERFACE);

        Marshal.Release(unknown);
    }

    [Fact]
    public void IReflect_GetDispatch()
    {
        ReflectClass reflect = new();
        nint unknown = Marshal.GetIUnknownForObject(reflect);
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

        Marshal.Release(unknown);

        // IDispatchEx is generated for Type, EnumBuilder, TypeBuilder, and any class that derives from IReflect.
    }

    [Fact]
    public void IReflect_ISupportErrorInfo()
    {
        ReflectClass reflect = new();
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));

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
        using ComScope<IDispatch> dispatch = new((IDispatch*)Marshal.GetIDispatchForObject(invoke));
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

        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatch = unknown.QueryInterface<IDispatchEx>();

        // When Invoking via enumerated ids "ToString" is passed to IReflect.InvokeMember (as opposed to "[DISPID=]").

        var dispatchIds = EnumerateDispatchIds(dispatch);
        dispatchIds.Keys.Should().BeEquivalentTo("GetType", "ToString", "Equals", "GetHashCode");

        using VARIANT result = default;
        DISPPARAMS dispparams = default;
        HRESULT hr = dispatch.Value->InvokeEx(
            dispatchIds["ToString"],
            0,
            (ushort)DISPATCH_FLAGS.DISPATCH_METHOD,
            &dispparams,
            &result,
            null,
            null);

        hr.Succeeded.Should().BeTrue();

        result.vt.Should().Be(VARENUM.VT_BSTR);
        result.data.bstrVal.ToString().Should().Be("Tests.Windows.Win32.System.Com.IReflectTests+ReflectSelf");
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
        using ComScope<IUnknown> unknown = new((IUnknown*)Marshal.GetIUnknownForObject(reflect));
        using ComScope<IDispatchEx> dispatch = unknown.QueryInterface<IDispatchEx>();

        // Only explicitly provided member info in IReflect is exposed.

        var dispatchIds = EnumerateDispatchIds(dispatch);
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
        using ComScope<IDispatch> dispatch = new((IDispatch*)Marshal.GetIDispatchForObject(reflect));
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

        [ComVisible(true)]
        public bool Foo() => true;

        [DispId(1066)]
        public bool Bar() => true;

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
        protected override Type UnderlyingSystemType => typeof(ReflectSelf);

        protected override FieldInfo? GetField(string name, BindingFlags bindingAttr)
        {
            return typeof(ReflectSelf).GetField(name, bindingAttr);
        }

        protected override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            FieldInfo[] fields = typeof(ReflectSelf).GetFields(bindingAttr);
            return fields;
        }

        protected override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return typeof(ReflectSelf).GetMember(name, bindingAttr);
        }

        protected override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return typeof(ReflectSelf).GetMembers(bindingAttr);
        }

        protected override MethodInfo? GetMethod(string name, BindingFlags bindingAttr)
        {
            return typeof(ReflectSelf).GetMethod(name, bindingAttr);
        }

        protected override MethodInfo? GetMethod(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type[] types,
            ParameterModifier[]? modifiers)
        {
            return typeof(ReflectSelf).GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        protected override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            MethodInfo[] methods = typeof(ReflectSelf).GetMethods(bindingAttr);
            return methods;
        }

        protected override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            PropertyInfo[] properties = typeof(ReflectSelf).GetProperties(bindingAttr);
            return properties;
        }

        protected override PropertyInfo? GetProperty(string name, BindingFlags bindingAttr)
        {
            return typeof(ReflectSelf).GetProperty(name, bindingAttr);
        }

        protected override PropertyInfo? GetProperty(
            string name,
            BindingFlags bindingAttr,
            Binder? binder,
            Type? returnType,
            Type[] types,
            ParameterModifier[]? modifiers)
        {
            return typeof(ReflectSelf).GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
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
            return typeof(ReflectSelf).InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
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

    private static IDictionary<string, int> EnumerateDispatchIds(IDispatchEx* dispatch)
    {
        Dictionary<string, int> dispatchIds = new();
        int dispid = Interop.DISPID_STARTENUM;
        while (dispatch->GetNextDispID(Interop.fdexEnumAll, dispid, &dispid) == HRESULT.S_OK)
        {
            BSTR name = default;
            HRESULT hr = dispatch->GetMemberName(dispid, &name);
            dispatchIds.Add(name.ToStringAndFree(), dispid);
        }

        return dispatchIds;
    }
}
