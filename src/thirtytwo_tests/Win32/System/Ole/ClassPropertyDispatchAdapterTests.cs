// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Variant;
using static Windows.Win32.System.Ole.FDEX_PROP_FLAGS;

namespace Windows.Win32.System.Ole;

[TestClass]
[DoNotParallelize]
public unsafe class ClassPropertyDispatchAdapterTests
{
    private const FDEX_PROP_FLAGS CommonPropertyFlags =
        fdexPropCannotPutRef | fdexPropCannotCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents;

    public static IEnumerable<object[]> UnsupportedDispatchFlags =>
    [
        [default(DISPATCH_FLAGS)],
        [DISPATCH_FLAGS.DISPATCH_METHOD],
        [DISPATCH_FLAGS.DISPATCH_PROPERTYPUTREF],
        [DISPATCH_FLAGS.DISPATCH_PROPERTYGET | DISPATCH_FLAGS.DISPATCH_PROPERTYPUT],
        [DISPATCH_FLAGS.DISPATCH_METHOD | DISPATCH_FLAGS.DISPATCH_PROPERTYGET],
        [(DISPATCH_FLAGS)PInvoke.DISPATCH_CONSTRUCT],
        [(DISPATCH_FLAGS)0x8000]
    ];

    [TestMethod]
    public void ClassPropertyDispatchAdapter_Constructor_EnumeratesOnlyPublicNonIndexedProperties()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);

        adapter.TryGetDispID(nameof(TestProperties.ReadWrite), out int readWriteId).Should().BeTrue();
        readWriteId.Should().Be(42);
        adapter.TryGetDispID(nameof(TestProperties.ReadOnly), out _).Should().BeTrue();
        adapter.TryGetDispID(nameof(TestProperties.WriteOnly), out _).Should().BeTrue();
        adapter.TryGetDispID(nameof(TestProperties.StaticValue), out _).Should().BeTrue();

        adapter.TryGetDispID("PrivateProperty", out _).Should().BeFalse();
        adapter.TryGetDispID("Item", out _).Should().BeFalse();
        adapter.TryGetDispID(nameof(TestProperties.Method), out _).Should().BeFalse();
    }

    [TestMethod]
    public void TryGetDispID_NameMatching_IsCaseInsensitive()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);

        adapter.TryGetDispID("readwrite", out int lowerCaseId).Should().BeTrue();
        adapter.TryGetDispID("READWRITE", out int upperCaseId).Should().BeTrue();
        upperCaseId.Should().Be(lowerCaseId);
    }

    [TestMethod]
    public void TryGetMemberName_KnownAndUnknownIds_ReturnsExpectedValues()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));

        adapter.TryGetMemberName(id, out string? name).Should().BeTrue();
        name.Should().Be(nameof(TestProperties.ReadWrite));

        adapter.TryGetMemberName(int.MaxValue, out name).Should().BeFalse();
        name.Should().BeNull();
    }

    [TestMethod]
    public void TryGetNextDispId_FromStart_EnumeratesEveryAdvertisedProperty()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        HashSet<string> names = [];
        int current = PInvoke.DISPID_STARTENUM;

        while (adapter.TryGetNextDispId(current, out int next))
        {
            adapter.TryGetMemberName(next, out string? name).Should().BeTrue();
            names.Add(name!).Should().BeTrue();
            current = next;
        }

        names.Should().BeEquivalentTo(
            nameof(TestProperties.ReadWrite),
            nameof(TestProperties.ReadOnly),
            nameof(TestProperties.WriteOnly),
            nameof(TestProperties.LastWritten),
            nameof(TestProperties.PrivateSetter),
            nameof(TestProperties.PrivateGetter),
            nameof(TestProperties.StaticValue),
            nameof(TestProperties.ThrowingGet),
            nameof(TestProperties.ThrowingSet));

        adapter.TryGetNextDispId(current, out int finalId).Should().BeFalse();
        finalId.Should().Be(PInvoke.DISPID_UNKNOWN);
    }

    [TestMethod]
    public void TryGetMemberProperties_PublicAccessorVisibility_ReportsExactCapabilities()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);

        GetMemberProperties(adapter, nameof(TestProperties.ReadWrite))
            .Should().Be(fdexPropCanGet | fdexPropCanPut | CommonPropertyFlags);
        GetMemberProperties(adapter, nameof(TestProperties.ReadOnly))
            .Should().Be(fdexPropCanGet | fdexPropCannotPut | CommonPropertyFlags);
        GetMemberProperties(adapter, nameof(TestProperties.WriteOnly))
            .Should().Be(fdexPropCannotGet | fdexPropCanPut | CommonPropertyFlags);
        GetMemberProperties(adapter, nameof(TestProperties.PrivateSetter))
            .Should().Be(fdexPropCanGet | fdexPropCannotPut | CommonPropertyFlags);
        GetMemberProperties(adapter, nameof(TestProperties.PrivateGetter))
            .Should().Be(fdexPropCannotGet | fdexPropCanPut | CommonPropertyFlags);
    }

    [TestMethod]
    public void TryGetMemberProperties_UnknownId_ReturnsFalseAndDefaultFlags()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);

        adapter.TryGetMemberProperties(int.MaxValue, out FDEX_PROP_FLAGS flags).Should().BeFalse();
        flags.Should().Be(default);
    }

    [TestMethod]
    public void Invoke_PublicInstanceProperty_GetsValue()
    {
        TestProperties target = new() { ReadWrite = 123 };
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));

        using VARIANT result = GetProperty(adapter, id, out HRESULT hr);

        hr.Should().Be(HRESULT.S_OK);
        result.vt.Should().Be(VARENUM.VT_I4);
        ((int)result).Should().Be(123);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PublicInstanceProperty_SetsValue()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));

        HRESULT hr = SetProperty(adapter, id, (VARIANT)456);

        hr.Should().Be(HRESULT.S_OK);
        target.ReadWrite.Should().Be(456);
    }

    [TestMethod]
    public void Invoke_PublicStaticProperty_GetsAndSetsValue()
    {
        int original = TestProperties.StaticValue;
        try
        {
            TestProperties.StaticValue = 10;
            TestProperties target = new();
            ClassPropertyDispatchAdapter adapter = new(target);
            int id = GetDispatchId(adapter, nameof(TestProperties.StaticValue));

            using VARIANT result = GetProperty(adapter, id, out HRESULT getResult);
            HRESULT setResult = SetProperty(adapter, id, (VARIANT)20);

            getResult.Should().Be(HRESULT.S_OK);
            ((int)result).Should().Be(10);
            setResult.Should().Be(HRESULT.S_OK);
            TestProperties.StaticValue.Should().Be(20);
            GC.KeepAlive(target);
        }
        finally
        {
            TestProperties.StaticValue = original;
        }
    }

    [TestMethod, DynamicData(nameof(UnsupportedDispatchFlags))]
    public void Invoke_UnadvertisedOperation_ReturnsInvalidArgument(DISPATCH_FLAGS flags)
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        DISPPARAMS parameters = default;
        using VARIANT result = default;

        HRESULT hr = adapter.Invoke(id, lcid: 0, flags, &parameters, &result);

        hr.Should().Be(HRESULT.E_INVALIDARG);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_UnadvertisedAccessor_ReturnsInvalidArgument()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int readOnlyId = GetDispatchId(adapter, nameof(TestProperties.ReadOnly));
        int writeOnlyId = GetDispatchId(adapter, nameof(TestProperties.WriteOnly));
        DISPPARAMS parameters = default;
        using VARIANT result = default;

        adapter.Invoke(
            readOnlyId,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &parameters,
            result: null).Should().Be(HRESULT.E_INVALIDARG);
        adapter.Invoke(
            writeOnlyId,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &parameters,
            &result).Should().Be(HRESULT.E_INVALIDARG);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_UnknownDispatchId_ReturnsMemberNotFound()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);

        adapter.Invoke(
            int.MaxValue,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            parameters: null,
            result: null).Should().Be(PInvoke.DISP_E_MEMBERNOTFOUND);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_NullParameters_ReturnsPointerError()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        using VARIANT result = default;

        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            parameters: null,
            &result).Should().Be(HRESULT.E_POINTER);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PropertyGetWithNullResult_ReturnsPointerError()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        DISPPARAMS parameters = default;

        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &parameters,
            result: null).Should().Be(HRESULT.E_POINTER);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PropertyPutWithNullArgument_ReturnsPointerError()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        DISPPARAMS parameters = new() { cArgs = 1, rgvarg = null };

        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &parameters,
            result: null).Should().Be(HRESULT.E_POINTER);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PropertyGetWithArguments_ReturnsBadParameterCount()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        VARIANT argument = (VARIANT)1;
        DISPPARAMS parameters = new() { cArgs = 1, rgvarg = &argument };
        using VARIANT result = default;

        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &parameters,
            &result).Should().Be(PInvoke.DISP_E_BADPARAMCOUNT);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PropertyPutWithIncorrectArgumentCount_ReturnsBadParameterCount()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        VARIANT arguments = (VARIANT)1;
        DISPPARAMS noArguments = default;
        DISPPARAMS twoArguments = new() { cArgs = 2, rgvarg = &arguments };

        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &noArguments,
            result: null).Should().Be(PInvoke.DISP_E_BADPARAMCOUNT);
        adapter.Invoke(
            id,
            lcid: 0,
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &twoArguments,
            result: null).Should().Be(PInvoke.DISP_E_BADPARAMCOUNT);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void Invoke_PropertyPutWithWrongValueType_ReturnsInvalidArgument()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int id = GetDispatchId(adapter, nameof(TestProperties.ReadWrite));
        using VARIANT value = (VARIANT)"not an integer";

        HRESULT hr = SetProperty(adapter, id, value);

        hr.Should().Be(HRESULT.E_INVALIDARG);
        target.ReadWrite.Should().Be(0);
    }

    [TestMethod]
    public void Invoke_AccessorThrows_ReturnsTargetInvocationExceptionHResult()
    {
        TestProperties target = new();
        ClassPropertyDispatchAdapter adapter = new(target);
        int getId = GetDispatchId(adapter, nameof(TestProperties.ThrowingGet));
        int setId = GetDispatchId(adapter, nameof(TestProperties.ThrowingSet));
        int expected = new TargetInvocationException(new InvalidOperationException()).HResult;

        using VARIANT result = GetProperty(adapter, getId, out HRESULT getResult);
        HRESULT setResult = SetProperty(adapter, setId, (VARIANT)1);

        getResult.Should().Be((HRESULT)expected);
        setResult.Should().Be((HRESULT)expected);
        GC.KeepAlive(target);
    }

    private static FDEX_PROP_FLAGS GetMemberProperties(ClassPropertyDispatchAdapter adapter, string name)
    {
        int id = GetDispatchId(adapter, name);
        adapter.TryGetMemberProperties(id, out FDEX_PROP_FLAGS flags).Should().BeTrue();
        return flags;
    }

    private static int GetDispatchId(ClassPropertyDispatchAdapter adapter, string name)
    {
        adapter.TryGetDispID(name, out int id).Should().BeTrue();
        return id;
    }

    private static VARIANT GetProperty(ClassPropertyDispatchAdapter adapter, int id, out HRESULT hr)
    {
        DISPPARAMS parameters = default;
        VARIANT result = default;
        hr = adapter.Invoke(id, lcid: 0, DISPATCH_FLAGS.DISPATCH_PROPERTYGET, &parameters, &result);
        return result;
    }

    private static HRESULT SetProperty(ClassPropertyDispatchAdapter adapter, int id, VARIANT value)
    {
        int propertyPutId = PInvoke.DISPID_PROPERTYPUT;
        DISPPARAMS parameters = new()
        {
            cArgs = 1,
            cNamedArgs = 1,
            rgdispidNamedArgs = &propertyPutId,
            rgvarg = &value
        };

        return adapter.Invoke(id, lcid: 0, DISPATCH_FLAGS.DISPATCH_PROPERTYPUT, &parameters, result: null);
    }

    private sealed class TestProperties
    {
        private int _writeOnly;

        [DispId(42)]
        public int ReadWrite { get; set; }

        public int ReadOnly => 10;

        public int WriteOnly
        {
            set => _writeOnly = value;
        }

        public int LastWritten => _writeOnly;

        public int PrivateSetter { get; private set; }

        public int PrivateGetter { private get; set; }

        public static int StaticValue { get; set; }

        public int ThrowingGet => throw new InvalidOperationException();

        public int ThrowingSet
        {
            set => throw new InvalidOperationException();
        }

        private int PrivateProperty { get; set; }

        public int this[int index]
        {
            get => index;
            set => _writeOnly = value;
        }

        public void Method()
        {
        }
    }
}
