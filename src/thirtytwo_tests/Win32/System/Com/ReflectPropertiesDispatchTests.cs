// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Ole;
using static Windows.Win32.System.Ole.FDEX_PROP_FLAGS;

namespace Tests.Windows.Win32.System.Com;

public unsafe class ReflectPropertiesDispatchTests
{
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

    /// <remarks>
    ///  Aligns with <see cref="IReflectTests.IReflect_ObjectBehavior"/>.
    /// </remarks>
    [Theory, MemberData(nameof(ObjectBehaviorTestData))]
    public void ReflectPropertiesDispatch_ObjectBehavior(object? obj, VARENUM expected)
    {
        ReflectObjectTypes reflect = new();

        using ComScope<IDispatchEx> dispatch = new(ComHelpers.GetComPointer<IDispatchEx>(reflect));

        var dispatchIds = dispatch.Value->GetAllDispatchIds();

        dispatchIds.Keys.Should().Contain("ObjectAsInterface", "Object");

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

    /// <remarks>
    ///  Aligns with <see cref="IReflectTests.IReflect_NonObjectBehavior"/>.
    /// </remarks>
    [Fact]
    public void ReflectPropertiesDispatch_NonObjectBehavior()
    {
        ReflectNonObjectTypes reflect = new()
        {
            Color = Color.Blue
        };

        using ComScope<IDispatchEx> dispatch = new(ComHelpers.GetComPointer<IDispatchEx>(reflect));

        var dispatchIds = dispatch.Value->GetAllDispatchIds();

        dispatchIds.Keys.Should().Contain("Location", "Color", "Count");

        VARIANT result = dispatch.Value->TryGetPropertyValue(dispatchIds["Location"], out HRESULT hr);
        hr.Should().Be(HRESULT.COR_E_NOTSUPPORTED);

        VARIANT value = (VARIANT)42;
        hr = dispatch.Value->TrySetPropertyValue(dispatchIds["Count"], value);
        hr.Succeeded.Should().BeTrue();
        reflect.Count.Should().Be(42);

        result = dispatch.Value->TryGetPropertyValue(dispatchIds["Color"], out hr);
        result.vt.Should().Be(VARENUM.VT_UI4);
        ((uint)result).Should().Be(0x00FF0000);  // AABBGGRR OLECOLOR

        value = (VARIANT)(uint)0x000000FF;
        hr = dispatch.Value->TrySetPropertyValue(dispatchIds["Color"], value);
        hr.Should().Be(HRESULT.DISP_E_MEMBERNOTFOUND);

        dispatch.Value->GetMemberProperties(dispatchIds["Color"], uint.MaxValue, out var flags);
        flags.Should().Be(fdexPropCanGet | fdexPropCanPut | fdexPropCannotPutRef
            | fdexPropCannotCall | fdexPropCannotConstruct | fdexPropCannotSourceEvents);
    }


    private class ReflectObjectTypes : ReflectPropertiesDispatch, IManagedWrapper<IDispatch, IDispatchEx>
    {
        public object? ObjectAsInterface
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
            [param: MarshalAs(UnmanagedType.Interface)]
            set;
        }

        public object? Object { get; set; }
    }

    private class ReflectNonObjectTypes : ReflectPropertiesDispatch, IManagedWrapper<IDispatch, IDispatchEx>
    {
        public Point Location { get; internal set; }
        public Color Color { get; set; }
        public int Count { get; set; }
    }
}
